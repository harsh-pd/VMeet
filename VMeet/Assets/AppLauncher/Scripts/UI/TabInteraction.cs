using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AL.UI
{
    public class TabInteraction : ToggleInteraction
    {
        [SerializeField]
        private GameObject m_page;

        public static EventHandler TabChangeInitiated = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            var toggle = (Toggle)selectable;
            if (toggle != null)
                toggle.onValueChanged.AddListener(OnValueChange);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            var toggle = (Toggle)selectable;
            if (toggle != null)
                toggle.onValueChanged.RemoveAllListeners();
        }

        protected virtual void OnValueChange(bool val)
        {
            if (!val && TabChangeInitiated != null && m_page != null)
            {
                TabChangeInitiated.Invoke(this, EventArgs.Empty);
            }

            if (m_page != null)
                m_page.SetActive(val);
        }

        public override void ToggleBackgroundHighlight(bool val)
        {
            //base.ToggleBackgroundHighlight(val);
        }

        public override void ToggleOutlineHighlight(bool val)
        {
            //base.ToggleOutlineHighlight(val);
        }
    }
}
