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
        private Transform rightHand, leftHand;
        [SerializeField]
        private Transform pen;

        public int playerId { get; private set; }
        public Trail currentDefaultTrail { get; set; }
        public Transform RightHand { get { return rightHand; } }
        public Transform LeftHand { get { return leftHand; } }
        public Transform Pen { get { return pen; } }


        public OVRInput.Controller selectedController = OVRInput.Controller.RTouch;

        public void Setup(int senderId, int playerViewId, int avatarViewId)
        {
            Debug.LogError(senderId + " " + playerViewId + " " + avatarViewId);
            name = "RemotePlayer: " + senderId; 
            m_playerSync.isRemotePlayer = true;
            m_playerSync.playerId = senderId;
            m_playerPhotonView.ViewID = playerViewId;
            m_avatarPhotonView.ViewID = avatarViewId;
        }

        private void OnDestroy()
        {
            Debug.LogError(m_avatarPhotonView.ViewID + " " + m_playerPhotonView.ViewID);
        }
    }
}
