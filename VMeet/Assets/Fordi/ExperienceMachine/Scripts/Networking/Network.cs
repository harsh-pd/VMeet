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

namespace Fordi.Networking
{
    public interface INetwork
    {
        void CreateRoom(string roomName);
        void JoinRoom(string roomName);
        void LeaveRoom();
    }

    public class Network : MonoBehaviourPunCallbacks, INetwork
    {
        [SerializeField]
        private bool m_debug = true;

        [SerializeField]
        private RemotePlayer m_remotePlayerPrefab = null;

        private IPlayer m_player = null;

        private static List<RoomInfo> m_rooms = new List<RoomInfo>();
        public static RoomInfo[] Rooms { get { return m_rooms.ToArray(); } }

        #region INITIALIZATIONS
        private void Awake()
        {
            m_player = IOC.Resolve<IPlayer>();
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
            //if (PhotonNetwork.CountOfRooms > 0)
            //    JoinRoom("Test");
            //else
            //    CreateRoom("Test");
        }

        public void CreateRoom(string roomName)
        {
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
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel("Multiplayer");
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            Log(message);
        }

        public void JoinRoom(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
        }

        public void LeaveRoom()
        {
            if (PhotonNetwork.InRoom)
                PhotonNetwork.LeaveRoom();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            m_rooms = roomList;
            //foreach (var item in m_rooms)
            //{
            //    Log(item.Name);
            //}
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
            if (firstHand)
                RaiseSecondHandPlayerSpawnEvent(senderId);
            
        }

        //private void SpawnAvatar(int senderId, int viewId)
        //{
        //    GameObject go = null;
        //    Debug.LogError("_____spawnAvatar event received:  " + PhotonNetwork.player.ID.ToString() + "--" + senderId.ToString());
        //    Coordinator.instance.audioManager.Play(Cornea.AudioManager.playerJoin);
        //    if (PhotonNetwork.LocalPlayer.ActorNumber == senderId)
        //    {
        //        go = Instantiate(Resources.Load("LocalOvrAvatar")) as GameObject;
        //        go.transform.parent = ovrCam.transform.GetChild(0);
        //        go.transform.localPosition = Vector3.zero;
        //        go.transform.localRotation = Quaternion.identity;

        //        locOvrAvatarRef = go;

        //        go.GetComponent<OvrAvatar>().oculusUserID = sceneMgr.myOculusId;
        //        go.SetActive(true);
        //        GameObject gt = Instantiate(usrTxtMesh, go.transform.GetComponentInChildren<OvrAvatarBase>().transform) as GameObject;
        //        gt.transform.localPosition = new Vector3(0.22f, 1.9f, 0);
        //        gt.transform.localScale = new Vector3(-1, 1, 1);
        //        gt.GetComponent<TextMesh>().text = sceneMgr.myOculusName;

        //        gt = Instantiate(remoteLaser, go.transform.GetChild(4)) as GameObject;
        //        //gt.transform.parent = ovrCam.transform.GetChild(0);
        //        gt.transform.localPosition = Vector3.zero;
        //        gt.transform.localRotation = Quaternion.identity;


        //        if (ovrCam.transform.parent.gameObject.GetComponent<OvrPlayerSync>() != null)
        //        {
        //            ovrCam.transform.parent.gameObject.GetComponent<OvrPlayerSync>().rLsr = gt.GetComponent<RemoteLaser>();
        //        }
        //    }
        //    else
        //    {
        //        InSceneNetworkManager inSceneNetworkManager = transform.GetComponent<InSceneNetworkManager>();
        //        if (inSceneNetworkManager.remoteOvrPlayers != null && inSceneNetworkManager.remoteOvrPlayers.ContainsKey(PhotonPlayer.Find(senderId).NickName))
        //        {
        //            return;
        //        }


        //        go = Instantiate(Resources.Load("RemoteOvrAvatar")) as GameObject;

        //        //Coordinator.instance.annotation.InitializeRemotePlayerAnnotation(senderid);

        //        go.GetComponent<OvrAvatar>().oculusUserID = (string)PhotonPlayer.Find(senderId).CustomProperties["oculusID"];

        //        OvrPlayerSync[] ps = FindObjectsOfType<OvrPlayerSync>();

        //        //remotePlyrSync.Clear();
        //        //foreach (OvrPlayerSync op in ps)
        //        //    remotePlyrSync.Add(op);

        //        OvrPlayerSync pps = null;
        //        foreach (OvrPlayerSync os in ps)
        //        {
        //            if (!os.avatarSet)
        //            {
        //                os.gameObject.transform.rotation = go.transform.rotation;
        //                go.transform.parent = os.gameObject.transform.GetChild(0);
        //                go.transform.localPosition = new Vector3(0, 0, 0);//Vector3.zero;
        //                                                                  //go.transform.localRotation = Quaternion.identity;
        //                os.avatarSet = true;
        //                pps = os;
        //                break;
        //            }
        //        }

        //        go.SetActive(true);
        //        inSceneNetworkManager.remoteOvrPlayers.Add(PhotonPlayer.Find(senderId).NickName, go.transform.parent.gameObject);

        //        GameObject gt = Instantiate(usrTxtMesh, go.transform.GetComponentInChildren<OvrAvatarBase>().transform) as GameObject;
        //        //gt.transform.localPosition = new Vector3(0, 1.7f, 0);
        //        gt.transform.localPosition = new Vector3(0.22f, 1.9f, 0);

        //        gt.transform.localScale = new Vector3(-1, 1, 1);
        //        gt.GetComponent<TextMesh>().text = (string)PhotonPlayer.Find(senderId).CustomProperties["oculusName"];


        //        gt = Instantiate(remoteLaser, go.transform.GetChild(3)) as GameObject;
        //        //gt.transform.parent = go.transform.parent;
        //        gt.transform.localPosition = Vector3.zero;
        //        gt.transform.localRotation = Quaternion.identity;

        //        if (pps != null)
        //        {
        //            pps.rLsr = gt.GetComponent<RemoteLaser>();
        //        }
        //    }
        //    //Debug.Log("Spawned ovr player");
        //    if (go != null)
        //    {

        //        PhotonView pView = go.GetComponent<PhotonView>();
        //        go.GetComponent<UniqueIdentifier>().playerId = senderId;

        //        if (pView != null)
        //        {
        //            pView.viewID = viewId;
        //        }

        //        voiceMgr.EnableVoIP(go);
        //    }
        //}

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
        #endregion
    }
}
