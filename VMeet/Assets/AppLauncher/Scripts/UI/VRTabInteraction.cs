using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VRExperience.UI
{
    public class VRTabInteraction : VRToggleInteraction
    {
        [SerializeField]
        private GameObject m_page;

        protected override void OnValueChange(bool val)
        {
            base.OnValueChange(val);
            if (m_page != null)
                m_page.SetActive(val);
        }
    }
}
