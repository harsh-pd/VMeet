using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.UI.MenuControl;

namespace Fordi.ScreenSharing
{
    public class RemoteMonitorScreen : MenuScreen
    {
        [SerializeField]
        private GameObject m_TogglePrefab;
        [SerializeField]
        private GameObject m_menuBorderPrefab = null;

        private Toggle m_micToggle = null;

        public override void OpenMenu(MenuItemInfo[] items, bool blocked, bool persist)
        {
            base.OpenMenu(items, blocked, persist);
            var toggleMenu = Instantiate(m_TogglePrefab, m_contentRoot);
            m_micToggle = toggleMenu.GetComponentInChildren<Toggle>();
            if (m_micToggle == null)
            {
                Debug.LogError("Invalid m_TogglePrefab");
                return;
            }
            m_micToggle.isOn = true;
            m_micToggle.onValueChanged.AddListener(OnMicToggle);
            toggleMenu.GetComponentInChildren<TextMeshProUGUI>().text = "Mic: ";
            Instantiate(m_menuBorderPrefab, toggleMenu.transform);
            toggleMenu.transform.SetSiblingIndex(0);
        }

        private void OnMicToggle(bool val)
        {

        }
    }
}
