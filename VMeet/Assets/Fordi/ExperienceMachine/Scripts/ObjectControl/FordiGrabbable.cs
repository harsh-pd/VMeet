using System;
using UnityEngine;
using OVRTouchSample;
using OculusSampleFramework;
using Fordi.UI.MenuControl;
using Fordi.Common;
using Fordi.UI;

namespace Fordi.ObjectControl
{
    public class FordiGrabbable : DistanceGrabbable
    {
        private IUserInterface m_vrMenu;
        private Rigidbody m_rigidbody;

        /// <summary>
        /// Duplicate version of m_grabbedKinematic, initialized on awake.
        /// </summary>
        private bool m_initiallyKinematic = false;

        protected override void AwakeOverride()
        {
            if (m_grabPoints == null)
                m_grabPoints = new Collider[] { };

            base.AwakeOverride();
            if (m_rigidbody == null)
                m_rigidbody = GetComponent<Rigidbody>();
            if (m_rigidbody != null)
                m_initiallyKinematic = m_rigidbody.isKinematic;
        }


        protected override void Start()
        {
            base.Start();
            m_vrMenu = IOC.Resolve<IUserInterface>();
        }

        public void SetGrabPoints(Collider[] grabPoints)
        {
            m_grabPoints = grabPoints;
        }

        public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
        {
            base.GrabBegin(hand, grabPoint);
        }

        public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
        {
            base.GrabEnd(linearVelocity, angularVelocity);

            if (m_rigidbody == null)
                m_rigidbody = GetComponent<Rigidbody>();
            if (m_rigidbody != null)
                m_rigidbody.isKinematic = m_initiallyKinematic;

            //if (m_vrMenu == null)
            //    m_vrMenu = IOC.Resolve<IVRMenu>();
            //m_vrMenu.ShowUI();
        }
    }
}
