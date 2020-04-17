using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System;
using VRExperience.Core;
using VRExperience.Common;
using UnityEngine.SceneManagement;
using VRExperience.UI.MenuControl;
using System.Linq;
using Fordi.ScreenSharing;

namespace Fordi.Networking
{
    public interface INetwork
    {
        void CreateRoom(string roomName);
        void JoinRoom(string roomName);
        void LeaveRoom(Action done);
        EventHandler RoomListUpdateEvent { get; set; }
        void ToggleScreenStreaming(bool val);
    }

    public class Network : MonoBehaviourPunCallbacks, INetwork
    {
        [SerializeField]
        private bool m_debug = true;

        [SerializeField]
        private RemotePlayer m_remotePlayerPrefab = null;

        private IPlayer m_player = null;
        private IVRMenu m_vrMenu = null;
        private IMenuSelection m_menuSelection = null;
        private IExperienceMachine m_experienceMachine = null;
        private IScreenShare m_screenShare = null;

        private const string MeetingRoom = "Meeting";

        private static List<RoomInfo> m_rooms = new List<RoomInfo>();
        public static RoomInfo[] Rooms { get { return m_rooms.ToArray(); } }

        public EventHandler RoomListUpdateEvent { get; set; }

        private Dictionary<int, RemotePlayer> m_remotePlayers = new Dictionary<int, RemotePlayer>();

        #region INITIALIZATIONS
        private void Awake()
        {
            m_player = IOC.Resolve<IPlayer>();
            m_vrMenu = IOC.Resolve<IVRMenu>();
            m_menuSelection = IOC.Resolve<IMenuSelection>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            if (!PhotonNetwork.IsConnectedAndReady)
                PhotonNetwork.ConnectUsingSettings();
        }

        private void OnLevelWasLoaded(int level)
        {
            if (PhotonNetwork.InRoom)
            {
                Debug.LogError("Level: " + level + " In room: " + PhotonNetwork.InRoom);
                RaisePlayerSpawnEvent();
            }
            //    if (PhotonNetwork.InRoom)
            //        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Cube"), Vector3.one, Quaternion.identity, 0);
        }
        #endregion

        #region CORE_NETWORKING
        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Log("OnConnectedToMaster");
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Log("OnJoinedLobby");
        }

        public void CreateRoom(string roomName)
        {
            m_vrMenu.DisplayProgress("Creating room: " + roomName);
            RoomOptions options = new RoomOptions
            {
                IsVisible = true,
                IsOpen = true,
                MaxPlayers = 10
            };

            PhotonNetwork.CreateRoom(roomName, options);
        }

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            Log("Created");
            m_rooms.Add(PhotonNetwork.CurrentRoom);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            Log("OnJoinedRoom");
            m_menuSelection.Location = MeetingRoom;
            m_menuSelection.ExperienceType = ExperienceType.MEETING;
            if (PhotonNetwork.IsMasterClient)
                m_experienceMachine.LoadExperience();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            Error error = new Error(Error.E_Exception);
            error.ErrorText = message;
            m_vrMenu.DisplayResult(error);
            Log(message);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Error error = new Error(Error.E_Exception);
            error.ErrorText = message;
            m_vrMenu.DisplayResult(error);
            base.OnJoinRoomFailed(returnCode, message);
        }

        public void JoinRoom(string roomName)
        {
            m_vrMenu.DisplayProgress("Joining room: " + roomName);
            PhotonNetwork.JoinRoom(roomName);
        }

        private Action m_onLeftRoom = null;
        public void LeaveRoom(Action done)
        {
            if (PhotonNetwork.InRoom)
            {
                m_onLeftRoom = done;
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                done?.Invoke();
            }
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            m_rooms = roomList.Where(room => !room.RemovedFromList).ToList();
            RoomListUpdateEvent?.Invoke(this, EventArgs.Empty);
            //if (m_rooms.Count > 0)
            //    JoinRoom(m_rooms[0].Name);
            //Debug.LogError("Recieved room udate: " + m_rooms.Count);
            //foreach (var item in m_rooms)
            //{
            //    Log(item.Name);
            //}
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            Destroy(m_remotePlayers[otherPlayer.ActorNumber].gameObject);
            m_remotePlayers.Remove(otherPlayer.ActorNumber);
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            m_onLeftRoom?.Invoke();
            m_onLeftRoom = null;
        }

        private void Log(string message)
        {
            if (m_debug)
                Debug.LogError(message);
        }
        #endregion

        #region SPAWN
        [PunRPC]
        private void RPC_SpawnPlayer(int senderId, int playerViewId, int avatarViewId, bool firstHand)
        {
            Debug.LogError(senderId + " " + firstHand);
            var remotePlayer = Instantiate(m_remotePlayerPrefab);
            remotePlayer.Setup(senderId, playerViewId, avatarViewId);
            m_remotePlayers[senderId] = remotePlayer;
            if (firstHand)
                RaiseSecondHandPlayerSpawnEvent(senderId);
            
        }
        
        public void RaiseSecondHandPlayerSpawnEvent(int targetPlayerId)
        {
            var targetPlayer = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == targetPlayerId);
            photonView.RPC("RPC_SpawnPlayer", targetPlayer, PhotonNetwork.LocalPlayer.ActorNumber, m_player.PlayerViewId, m_player.AvatarViewId, false);
        }

        public void RaisePlayerSpawnEvent()
        {
            int viewAvatarId = PhotonNetwork.AllocateViewID(false);
            int viewPlayerId = PhotonNetwork.AllocateViewID(false);
            Debug.LogError(SceneManager.GetActiveScene().name + " " + viewAvatarId + " " + viewPlayerId);

            try
            {
                var playerSync = m_player.PlayerController.GetComponent<OvrPlayerSync>();
                playerSync.Init(true, false, PhotonNetwork.LocalPlayer.ActorNumber);
                m_player.PlayerViewId = viewPlayerId;
                m_player.AvatarViewId = viewAvatarId;
            }
            catch (NullReferenceException)
            {
                Debug.LogError("Networking scripts not steup properly on local player");
                return;
            }

            //RaiseEventOptions options = new RaiseEventOptions
            //{
            //    CachingOption = EventCaching.AddToRoomCache,
            //    Receivers = ReceiverGroup.All
            //};
            photonView.RPC("RPC_SpawnPlayer", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, viewPlayerId, viewAvatarId, true);
        }

        public void ToggleScreenStreaming(bool val)
        {
            photonView.RPC("RPC_StopScreenShare", RpcTarget.Others, val);
        }

        [PunRPC]
        private void RPC_StopScreenShare(int sender, bool val)
        {
            m_screenShare.ToggleScreenReceiving(val);
        }
        #endregion
    }
}
