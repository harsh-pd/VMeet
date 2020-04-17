using agora_gaming_rtc;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.Common;
using VRExperience.Core;
using VRExperience.UI.MenuControl;

namespace Fordi.ScreenSharing
{
    public class RemoteMonitorScreen : MenuScreen
    {
        [SerializeField]
        private GameObject m_TogglePrefab;
        [SerializeField]
        private GameObject m_menuBorderPrefab = null;
        [SerializeField]
        private VideoSurface m_remoteMonitorView;
        [SerializeField]
        private VideoSurface m_remoteMonitorViewPrefab = null;

        private Toggle m_micToggle = null;
        private Toggle m_screenShareToggle = null;

        private IAppTheme m_appTheme = null;
        private IScreenShare m_screenShare = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_appTheme = IOC.Resolve<IAppTheme>();
            m_screenShare = IOC.Resolve<IScreenShare>();
            m_screenShare.OtherUserJoinedEvent += RemoteUserJoinedChannel;
        }

        private void RemoteUserJoinedChannel(object sender, uint e)
        {
            ToggleMonitor(true);
            m_remoteMonitorView.SetForUser(e);
            m_remoteMonitorView.SetEnable(true);
        }

        public override void OpenMenu(MenuItemInfo[] items, bool blocked, bool persist)
        {
            ToggleMonitor(false);
            var toggleMenu = Instantiate(m_TogglePrefab, m_contentRoot);
            m_micToggle = toggleMenu.GetComponentInChildren<Toggle>();
            m_micToggle.isOn = true;
            m_micToggle.onValueChanged.AddListener(OnMicToggle);
            toggleMenu.GetComponentInChildren<TextMeshProUGUI>().text = "Mic: ";

            toggleMenu = Instantiate(m_TogglePrefab, m_contentRoot);
            m_screenShareToggle = toggleMenu.GetComponentInChildren<Toggle>();
            m_screenShareToggle.isOn = true;
            m_screenShareToggle.onValueChanged.AddListener(ScreenSharingToggle);
            toggleMenu.GetComponentInChildren<TextMeshProUGUI>().text = "Share Screen: ";

            var border = Instantiate(m_menuBorderPrefab, m_contentRoot);


            if (m_standaloneMenu != null)
                m_standaloneMenu.SetActive(false);
            
            Blocked = blocked;
            Persist = persist;
            gameObject.SetActive(true);
            foreach (var item in items)
                SpawnMenuItem(item, m_menuItem, m_contentRoot);

            if (m_vrMenu == null)
                m_vrMenu = IOC.Resolve<IVRMenu>();

            if (m_okButton != null)
                m_okButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());
            if (m_closeButton != null)
                m_closeButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());

        }

        private void OnMicToggle(bool val)
        {

        }

        private void ScreenSharingToggle(bool val)
        {
            if (m_screenShare == null)
                m_screenShare = IOC.Resolve<IScreenShare>();
            if (m_screenShare != null)
                m_screenShare.BroadcastScreen = val;
        }

        public void ToggleMonitor(bool val)
        {
            if (val && m_remoteMonitorView != null)
            {
                Destroy(m_remoteMonitorView.gameObject);
            }
            else if (!val)
            {
                m_remoteMonitorView = Instantiate(m_remoteMonitorViewPrefab, transform);
                m_remoteMonitorView.transform.SetSiblingIndex(transform.childCount - 1);
            }
        }
    }
}
