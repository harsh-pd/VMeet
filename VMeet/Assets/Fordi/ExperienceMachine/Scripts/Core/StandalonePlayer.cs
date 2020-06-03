using System.Collections;
using System.Collections.Generic;
using OculusSampleFramework;
using Photon.Pun;
using UnityEngine;

namespace Fordi.Core
{
    public interface IPlayer
    {
        GameObject PlayerController { get; }
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

        public int PlayerViewId { get { return m_playerPhotonView.ViewID; } set { m_playerPhotonView.ViewID = value; } }

        public int AvatarViewId { get { return m_avatarPhotonView.ViewID; } set { m_avatarPhotonView.ViewID = value; } }

        public GameObject PlayerController { get { return m_playerController; } }

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
