using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System;
using Fordi.Core;
using Fordi.Common;
using UnityEngine.SceneManagement;
using Fordi.UI.MenuControl;
using System.Linq;
using Fordi.ScreenSharing;
using ExitGames.Client.Photon;
using Fordi.Annotations;
using Cornea.Web;
using Fordi.UI;
using UniRx;
using Fordi.VideoCall;

namespace Fordi.Networking
{
    public interface INetwork
    {
        void CreateRoom(string roomName);
        void JoinRoom(string roomName);
        void LeaveRoom();
        EventHandler RoomListUpdateEvent { get; set; }
        void ToggleScreenStreaming(bool val);
        RemotePlayer GetRemotePlayer(int actorNumber);
    }

    public class Network : MonoBehaviourPunCallbacks, INetwork, IOnEventCallback
    {
        [SerializeField]
        private RemotePlayer m_remotePlayerPrefab = null;

        public const byte trailBegin = 137;
        public const byte trailFinish = 138;
        public const byte deletePreviousTrail = 139;
        public const byte whiteboardNoteBegan = 140;
        public const byte videoMuteToggle = 141;

        public const string MeetingRoom = "Meeting";
        public const string LobbyRoom = "Lobby";
        public const string ActorNumberString = "ActorNumber";
        public const string OculusIDString = "OculusID";

        private IUIEngine m_uiEngine = null;
        private IMenuSelection m_menuSelection = null;
        private IExperienceMachine m_experienceMachine = null;
        private IScreenShare m_screenShare = null;
        private IAnnotation m_annotation = null;
        private IWebInterface m_webInterface = null;

        private static List<RoomInfo> m_rooms = new List<RoomInfo>();
        public static RoomInfo[] Rooms { get { return m_rooms.ToArray(); } }

        public EventHandler RoomListUpdateEvent { get; set; }

        private Dictionary<int, RemotePlayer> m_remotePlayers = new Dictionary<int, RemotePlayer>();

        private bool m_debug = false;

        #region INITIALIZATIONS
        private void Awake()
        {
            m_uiEngine = IOC.Resolve<IUIEngine>();
            m_menuSelection = IOC.Resolve<IMenuSelection>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_screenShare = IOC.Resolve<IScreenShare>();
            m_annotation = IOC.Resolve<IAnnotation>();
            m_webInterface = IOC.Resolve<IWebInterface>();

            if (!PhotonNetwork.IsConnectedAndReady)
                PhotonNetwork.ConnectUsingSettings();
        }

        private void OnLevelWasLoaded(int level)
        {
            if (PhotonNetwork.InRoom)
            {
                Log("Level: " + level + " In room: " + PhotonNetwork.InRoom);
                Observable.TimerFrame(20).Subscribe(_ => RaisePlayerSpawnEvent()); 
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

        private static bool m_autoJoined = false;
        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Log("OnJoinedLobby");


            ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable();
            playerCustomProperties.Add(OculusIDString, Fordi.Core.Player.s_OculusID);
            playerCustomProperties.Add(ActorNumberString, PhotonNetwork.LocalPlayer.ActorNumber);
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerCustomProperties);
            PhotonNetwork.LocalPlayer.NickName = m_webInterface.UserInfo.userName;

            //if (!m_autoJoined)
            //{
            //    if (PhotonNetwork.CountOfRooms > 0)
            //    {
            //        JoinRoom("Test");
            //    }
            //    else
            //        CreateRoom("Test");
            //}
        }

        public void CreateRoom(string roomName)
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                m_uiEngine.DisplayResult(new Error()
                {
                    ErrorCode = Error.E_InvalidOperation,
                    ErrorText = "Not connected to multiplayer server yet."
                });
                return;
            }

            m_uiEngine.DisplayProgress("Creating room: " + roomName);
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
            Debug.LogError("OnJoinedRoom");
            m_autoJoined = true;
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
            m_uiEngine.DisplayResult(error);
            Log(message);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Error error = new Error(Error.E_Exception);
            error.ErrorText = message;
            m_uiEngine.DisplayResult(error);
            base.OnJoinRoomFailed(returnCode, message);
        }

        public void JoinRoom(string roomName)
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                m_uiEngine.DisplayResult(new Error()
                {
                    ErrorCode = Error.E_InvalidOperation,
                    ErrorText = "Not connected to multiplayer server yet."
                });
                return;
            }

            m_uiEngine.DisplayProgress("Joining room: " + roomName);
            PhotonNetwork.JoinRoom(roomName);
        }

        public void LeaveRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
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
            
            m_menuSelection.Location = LobbyRoom;
            m_menuSelection.ExperienceType = ExperienceType.LOBBY;
            m_uiEngine.Close();
            m_experienceMachine.LoadExperience();
        }


        public void OnEvent(EventData photonEvent)
        {
            if (m_annotation == null)
                m_annotation = IOC.Resolve<IAnnotation>();

            switch (photonEvent.Code)
            {
                case trailBegin:
                    Debug.Log("_____trailBegin event");
                    object[] trailDrawData = (object[])photonEvent.CustomData;

                    Color selectedColor = new Color((float)trailDrawData[0], (float)trailDrawData[1], (float)trailDrawData[2], (float)trailDrawData[3]);
                    float selectedThickness = (float)trailDrawData[4];
                    int trailViewId = (int)trailDrawData[5];
                    m_annotation.RemoteStartNewTrail(photonEvent.Sender, selectedColor, selectedThickness, trailViewId, photonEvent.Sender);
                    break;
                case trailFinish:
                    Debug.Log("_____trailFinish event");
                    m_annotation.RemoteFinishTrail(photonEvent.Sender);
                    break;
                case deletePreviousTrail:
                    Debug.Log("_____deletePreviousTrail event");
                    m_annotation.RemoteDeletePreviousTrail(photonEvent.Sender);
                    break;
                case whiteboardNoteBegan:
                    Debug.Log("_____noteBegin event");
                    object[] whiteboardTrailDrawData = (object[])photonEvent.CustomData;

                    Color chosenColor = new Color((float)whiteboardTrailDrawData[0], (float)whiteboardTrailDrawData[1], (float)whiteboardTrailDrawData[2], (float)whiteboardTrailDrawData[3]);
                    Vector3 startPosition = (Vector3)whiteboardTrailDrawData[4];
                    float selectedNoteThickness = (float)whiteboardTrailDrawData[5];
                    int noteViewId = (int)whiteboardTrailDrawData[6];
                    m_annotation.RemoteStartNewNote(startPosition, photonEvent.Sender, chosenColor, selectedNoteThickness, noteViewId, photonEvent.Sender);
                    break;
            }
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
            Debug.LogError("RPC_SpawnPlayer: " + senderId + " " + firstHand);
            var remotePlayer = Instantiate(m_remotePlayerPrefab);

            var player = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == senderId);

            object oculusId = "";

            if (player != null)
            {
                var customProperties = player.CustomProperties;
                if (customProperties != null)
                    customProperties.TryGetValue(OculusIDString, out oculusId);
            }

            remotePlayer.Setup(senderId, playerViewId, avatarViewId, (string)oculusId);
            m_remotePlayers[senderId] = remotePlayer;
            if (firstHand)
                RaiseSecondHandPlayerSpawnEvent(senderId);

        }

        public void RaiseSecondHandPlayerSpawnEvent(int targetPlayerId)
        {
            Debug.LogError("RaisePlayerSpawnEvent: " + SceneManager.GetActiveScene().name + " " + m_experienceMachine.Player.AvatarViewId + " " + m_experienceMachine.Player.PlayerViewId);
            var targetPlayer = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == targetPlayerId);
            photonView.RPC("RPC_SpawnPlayer", targetPlayer, PhotonNetwork.LocalPlayer.ActorNumber, m_experienceMachine.Player.PlayerViewId, m_experienceMachine.Player.AvatarViewId, false);
        }

        public void RaisePlayerSpawnEvent()
        {
            Debug.LogError("RaisePlayerSpawnEvent: " + SceneManager.GetActiveScene().name + " " + m_experienceMachine.Player.AvatarViewId + " " + m_experienceMachine.Player.PlayerViewId);

            try
            {
                var playerSync = m_experienceMachine.Player.PlayerController.GetComponent<OvrPlayerSync>();
                playerSync.Init(true, false, PhotonNetwork.LocalPlayer.ActorNumber);
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
            photonView.RPC("RPC_SpawnPlayer", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, m_experienceMachine.Player.PlayerViewId, m_experienceMachine.Player.AvatarViewId, true);
        }

        public void ToggleScreenStreaming(bool val)
        {
            photonView.RPC("RPC_RemoteScreenShareNotification", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, val);
        }

        [PunRPC]
        private void RPC_RemoteScreenShareNotification(int sender, bool val)
        {
            Debug.LogError("RPC_RemoteScreenShareNotification: " + val);
            m_screenShare.ToggleScreenReceiving(val);
        }

        public RemotePlayer GetRemotePlayer(int actorNumber)
        {
            if (m_remotePlayers.ContainsKey(actorNumber))
                return m_remotePlayers[actorNumber];
            return null;
        }
        #endregion
    }
}
