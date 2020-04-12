using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;

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
        private GameObject m_playerPrefab = null;

        private void Awake()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void OnLevelWasLoaded(int level)
        {
            Debug.LogError("Loaded: " + level);
            if (PhotonNetwork.InRoom)
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Cube"), Vector3.one, Quaternion.identity, 0);
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Log("OnConnectedToMaster");
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Log("OnJoinedLobby");
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
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            Log("OnJoinedRoom");
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

        private void Log(string message)
        {
            if (m_debug)
                Debug.LogError(message);
        }
    }
}
