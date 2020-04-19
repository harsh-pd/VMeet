using Fordi.Annotation;
using Photon.Pun;
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
        private Transform m_penPrefab;

        public int playerId { get; private set; }
        public Trail currentDefaultTrail { get; set; }
        public Transform RightHand { get { return m_rightHand; } }
        public Transform LeftHand { get { return m_leftHand; } }
        public Transform Pen { get { return m_pen; } }

        private Transform m_pen;
        private Transform m_rightHand, m_leftHand = null;
        private Transform m_currentDefaultTrail = null;

        public OVRInput.Controller selectedController = OVRInput.Controller.RTouch;

        public void Setup(int senderId, int playerViewId, int avatarViewId)
        {
            Debug.LogError(senderId + " " + playerViewId + " " + avatarViewId);
            name = "RemotePlayer: " + senderId; 
            m_playerSync.isRemotePlayer = true;
            m_playerSync.playerId = senderId;
            m_playerPhotonView.ViewID = playerViewId;
            m_avatarPhotonView.ViewID = avatarViewId;

            StartCoroutine(EnsureGameobjectIntegrity());
        }

        private IEnumerator EnsureGameobjectIntegrity()
        {
            m_rightHand = transform.Find("right_hand");
            m_leftHand = transform.Find("left_hand");
            m_currentDefaultTrail = Instantiate(m_trailPrefab, m_rightHand).transform;
            m_pen = Instantiate(m_penPrefab, m_rightHand);
            yield return null;
        }

        private void OnDestroy()
        {
            Debug.LogError(m_avatarPhotonView.ViewID + " " + m_playerPhotonView.ViewID);
        }
    }
}
