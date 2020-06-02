using Fordi.Annotations;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Networking
{
    public class RemotePlayer : MonoBehaviour
    {
        [SerializeField]
        private OvrPlayerSync m_playerSync = null;
        [SerializeField]
        private PhotonView m_playerPhotonView = null;
        [SerializeField]
        private PhotonView m_avatarPhotonView = null;
        [SerializeField]
        private PhotonAvatarView m_avatarView = null;
        [SerializeField]
        private Trail m_trailPrefab = null;
        [SerializeField]
        private OvrAvatar m_avatar = null;

        [SerializeField]
        private Transform m_penPrefab;

        public int playerId { get; private set; }
        public Trail currentDefaultTrail { get; set; }
        public Transform RightHand { get { return m_rightHand; } }
        public Transform LeftHand { get { return m_leftHand; } }
        public Transform Pen { get { return m_pen; } }

        private Transform m_pen;
        private Transform m_rightHand, m_leftHand = null;

        public OVRInput.Controller selectedController = OVRInput.Controller.RTouch;

        private Photon.Realtime.Player m_photonPlayer = null;

        public void Setup(int senderId, int playerViewId, int avatarViewId)
        {
            Debug.LogError(senderId + " " + playerViewId + " " + avatarViewId);
            name = "RemotePlayer: " + senderId; 
            m_playerSync.isRemotePlayer = true;
            m_playerSync.playerId = senderId;
            m_playerPhotonView.ViewID = playerViewId;
            m_avatarPhotonView.ViewID = avatarViewId;

            m_photonPlayer = Array.Find(PhotonNetwork.PlayerListOthers, item => item.ActorNumber == senderId);

            try
            {
                m_avatar.oculusUserID = (string)m_photonPlayer.CustomProperties[Network.OculusIDString];
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }

            StartCoroutine(EnsureGameobjectIntegrity());
        }

        private IEnumerator EnsureGameobjectIntegrity()
        {
            OvrAvatarHand[] hands = null;
            do
            {
                hands = GetComponentsInChildren<OvrAvatarHand>();
                yield return null;
            }
            while (hands == null || hands.Length < 2);
            foreach (var item in hands)
            {
                if (item.isLeftHand)
                    m_leftHand = item.transform;
                else
                    m_rightHand = item.transform;
            }
            currentDefaultTrail = Instantiate(m_trailPrefab, m_rightHand);
            m_pen = Instantiate(m_penPrefab, m_rightHand);
        }

        private void OnDestroy()
        {
            //Debug.LogError(m_avatarPhotonView.ViewID + " " + m_playerPhotonView.ViewID);
        }
    }
}
