using System.Collections;
using System.Collections.Generic;
using Fordi.Common;
using OculusSampleFramework;
using Photon.Pun;
using UnityEngine;

namespace Fordi.Core
{
    public interface IPlayer
    {
        GameObject PlayerController { get; }
        GameObject GameObject { get; }
        //void RequestHaltMovement(bool val);
        //Transform PlayerCanvas { get; }
        //OVRCameraRig CameraRig { get; }
        //Transform RightHand { get; }
        //Transform LeftHand { get; }
        int PlayerViewId { get; set; }
        int AvatarViewId { get; set; }
        //void PrepareForSpawn();
        //void UpdateAdditionalRotation(float angle);
        //float RootRotation { get; }
        //void StartTooltipRoutine(List<VRButtonGroup> buttonGroups);
        void ApplyTooltipSettings();
        void StartTooltipRoutine(List<VRButtonGroup> m_usedButtons);
        void DoWaypointTeleport(Transform transform);
        void RequestHaltMovement(bool v);
        void FadeOut();
        //void Grab(DistanceGrabbable grabbable, OVRInput.Controller controller);
        //void ToogleGrabGuide(OVRInput.Controller controller, bool val);
        //bool GuideOn { get; }
        //void DoWaypointTeleport(Transform anchor);
        //void FadeOut();
    }

    public class StandalonePlayer : MonoBehaviour, IPlayer
    {
        [SerializeField]
        private PhotonView m_avatarPhotonView, m_playerPhotonView;

        [SerializeField]
        private GameObject m_playerController;
        public int PlayerViewId
        {
            get
            {
                if (!m_playerViewIdAllocated)
                {
                    m_playerPhotonView.ViewID = PhotonNetwork.AllocateViewID(false);
                    m_playerPhotonView.Synchronization = ViewSynchronization.UnreliableOnChange;
                    m_playerViewIdAllocated = true;
                }

                return m_playerPhotonView.ViewID;
            }
            set
            {
                m_playerPhotonView.ViewID = value;
            }
        }

        private bool m_avatarViewIdAllocated = false;
        private bool m_playerViewIdAllocated = false;
        public int AvatarViewId
        {
            get
            {
                if (!m_avatarViewIdAllocated)
                {
                    m_avatarPhotonView.ViewID = PhotonNetwork.AllocateViewID(false);
                    m_avatarViewIdAllocated = true;
                }
                return m_avatarPhotonView.ViewID;
            }
            set
            {
                m_avatarPhotonView.ViewID = value;
            }
        }

        public GameObject PlayerController { get { return m_playerController; } }

        public GameObject GameObject { get { return gameObject; } }

        private void Awake()
        {
            if (IOC.Resolve<IPlayer>() == null)
                IOC.Register<IPlayer>(this);
        }

        private void OnDestroy()
        {
            IOC.Unregister<IPlayer>(this);
        }

        public void ApplyTooltipSettings()
        {
            
        }

        public void DoWaypointTeleport(Transform transform)
        {
            
        }

        public void FadeOut()
        {
            
        }

        public void RequestHaltMovement(bool v)
        {
            
        }

        public void StartTooltipRoutine(List<VRButtonGroup> m_usedButtons)
        {
            
        }
    }
}
