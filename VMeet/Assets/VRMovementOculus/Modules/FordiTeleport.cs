﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using VRExperience.Core;
using VRExperience.Common;
using UnityEngine.EventSystems;

namespace VRExperience.Core
{
    public class FordiTeleport : VROculusTeleport
    {
        [Header("-Waypoint Teleport Controls-")]
        [SerializeField]
        private OVRInput.Button m_waypointTeleportButton = OVRInput.Button.Two;

        public OVRInput.Button WaypointTeleportButton { get { return m_waypointTeleportButton; } }

        private IExperienceMachine m_experienceMachine;
        private IPlayer m_player;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_player = IOC.Resolve<IPlayer>();
        }

        protected override void StartOverride()
        {
            base.StartOverride();
            WaypointTeleport();
        }

        protected override void UpdateOverride()
        {
            base.UpdateOverride();
            WaypointTeleportInput();
        }


        void WaypointTeleportInput()
        {
            if (canTeleport && FordiInput.GetDown(m_waypointTeleportButton, OVRInput.Controller.RTouch))
                WaypointTeleport();
        }

        private void WaypointTeleport()
        {
            Transform anchor = m_experienceMachine.GetNextTeleportAnchor();
            m_waypointTeleported = true;
            if (anchor == null)
                return;

            refSystem.myFade.StartFadeIn(refSystem.fadeTime);
            Vector3 holder = anchor.position;
            holder.y += refSystem.GetHeight();

            float rootRotation = m_player.RootRotation;
            float angle = anchor.transform.rotation.eulerAngles.y - rootRotation;


            refSystem.yourRig.enabled = false;
            refSystem.yourRig.transform.DOMove(holder, .1f).OnComplete(() =>
            {
                m_player.UpdateAdditionalRotation(angle);
                refSystem.yourRig.enabled = true;
            });

            refSystem.yourRig.transform.Rotate(new Vector3(0, angle, 0));

            refSystem.yourRig.transform.position = holder;
            Invoke("BumpMe", Time.deltaTime);
            return;
        }

        protected override void Teleport()
        {
            base.Teleport();
            m_teleported = true;
        }

        #region GUIDE_CONDITIONS
        private bool m_teleported = false, m_waypointTeleported = false;

        public bool Teleported()
        {
            var val = m_teleported;
            if (m_teleported)
                m_teleported = false;
            return val;
        }

        public bool WaypointTeleported()
        {
            var val = m_waypointTeleported;
            if (m_waypointTeleported)
                m_waypointTeleported = false;
            return val;
        }
        #endregion
    }
}
