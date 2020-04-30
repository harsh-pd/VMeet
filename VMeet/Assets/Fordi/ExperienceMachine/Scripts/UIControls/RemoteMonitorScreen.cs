using agora_gaming_rtc;
using Fordi.ChatEngine;
using Fordi.VoiceChat;
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
        private VideoSurface m_remoteMonitorViewPrefab = null;
        [SerializeField]
        private Chat m_chatPrefab;
        [SerializeField]
        private Transform m_chatRoot = null;

        private Toggle m_micToggle = null;
        private Toggle m_screenShareToggle = null;
        private Toggle m_chatToggle = null;

        private IAppTheme m_appTheme = null;
        private IScreenShare m_screenShare = null;
        private IVoiceChat m_voiceChat = null;

        private VideoSurface m_remoteMonitorView;
        private Chat m_chat = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_appTheme = IOC.Resolve<IAppTheme>();
            m_screenShare = IOC.Resolve<IScreenShare>();
            m_voiceChat = IOC.Resolve<IVoiceChat>();
            m_screenShare.OtherUserJoinedEvent += RemoteUserJoinedChannel;
            m_screenShare.RemoteScreenShareEvent += RemoteScreenShareNotification;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            m_screenShare.OtherUserJoinedEvent -= RemoteUserJoinedChannel;
            m_screenShare.RemoteScreenShareEvent -= RemoteScreenShareNotification;
        }

        private void RemoteUserJoinedChannel(object sender, uint e)
        {
            if (m_screenShare.ReceivingRemoteStream)
            {
                ToggleMonitor(true);
                m_remoteMonitorView.SetForUser(e);
                m_remoteMonitorView.SetEnable(true);
            }
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
            m_screenShareToggle.isOn = false;
            m_screenShareToggle.onValueChanged.AddListener(ScreenSharingToggle);
            toggleMenu.GetComponentInChildren<TextMeshProUGUI>().text = "Screen: ";

            toggleMenu = Instantiate(m_TogglePrefab, m_contentRoot);
            m_chatToggle = toggleMenu.GetComponentInChildren<Toggle>();
            m_chatToggle.isOn = false;
            m_chatToggle.onValueChanged.AddListener(ToggleChat);
            toggleMenu.GetComponentInChildren<TextMeshProUGUI>().text = "Chat: ";


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

        public void ExternallyCloseChat()
        {
            m_chatToggle.isOn = false;
        }

        private void OnMicToggle(bool val)
        {
            m_voiceChat.ToggleMute(val);
        }

        private void ScreenSharingToggle(bool val)
        {
            if (m_screenShare == null)
                m_screenShare = IOC.Resolve<IScreenShare>();
            if (m_screenShare != null)
                m_screenShare.ToggleScreenSharing(val);
        }

        private void ToggleMonitor(bool val)
        {
            if (!val && m_remoteMonitorView != null)
            {
                Destroy(m_remoteMonitorView.gameObject);
                m_screenShareToggle.interactable = true;
            }
            else if (val)
            {
                m_remoteMonitorView = Instantiate(m_remoteMonitorViewPrefab, transform);
                m_remoteMonitorView.transform.SetSiblingIndex(transform.childCount - 1);
                m_screenShareToggle.interactable = false;
            }
        }

        private void RemoteScreenShareNotification(object sender, ScreenEventArgs e)
        {
            if (!e.Streaming)
                ToggleMonitor(false);
            Debug.LogError(e.Streaming);
        }

        private void ToggleChat(bool val)
        {
            if (!val)
            {
                if (m_chat != null)
                    m_chat.gameObject.SetActive(false);
                return;
            }
            if (m_chat == null)
                m_chat = Instantiate(m_chatPrefab, m_chatRoot);
            else
                m_chat.gameObject.SetActive(true);
        }
    }
}
