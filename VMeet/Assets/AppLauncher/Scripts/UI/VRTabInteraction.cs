using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Fordi.UI
{
    public class VRTabInteraction : VRToggleInteraction
    {
        [SerializeField]
        private GameObject m_page;

        public static EventHandler TabChangeInitiated = null;

        protected override void OnValueChange(bool val)
        {
            base.OnValueChange(val);

            if (XRDevice.userPresence == UserPresenceState.Present && !val && TabChangeInitiated != null && m_page != null)
                TabChangeInitiated.Invoke(this, EventArgs.Empty);

            if (m_page != null)
                m_page.SetActive(val);
        }
    }
}
