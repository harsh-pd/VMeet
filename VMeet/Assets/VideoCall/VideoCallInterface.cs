using agora_gaming_rtc;
using Fordi.ChatEngine;
using Fordi.Common;
using Fordi.Core;
using Fordi.VideoCall;
using Fordi.Voice;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.UI.MenuControl
{
    public class VideoCallInterface : MenuScreen
    {
        [SerializeField]
        private VideoItem m_presenterVideoItem;

        [SerializeField]
        private GameObject m_participantsGrid;

        [SerializeField]
        private MenuOnHover m_onHoverMenu;

        [SerializeField]
        private Chat m_chatPrefab;

        [SerializeField]
        private Transform m_chatRoot;

        private IVideoCallEngine m_videoCallEngine;
        private IVoiceChat m_voiceChat;

        private Chat m_chat;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_videoCallEngine = IOC.Resolve<IVideoCallEngine>();
            m_voiceChat = IOC.Resolve<IVoiceChat>();
        }

        internal void AddVideo(MenuItemInfo item)
        {
            Debug.LogError("AddVideo");
            SpawnMenuItem(item, m_menuItem, m_contentRoot);
        }

        public void Present(MenuItemInfo item)
        {
            m_presenterVideoItem.DataBind(m_userInterface, item);
            m_presenterVideoItem.OnVideoMute(false);
        }

        public void StopPresenting()
        {
            m_presenterVideoItem.OnVideoMute(true);
        }

        public void ToggleVideo(bool val)
        {
            m_videoCallEngine.EnableVideo(!val);
        }

        public void ToggleFulscreen(bool val)
        {
            m_participantsGrid.gameObject.SetActive(!val);
            m_onHoverMenu.ToggleFulScreen(val);
        }

        public void ToggleMic(bool val)
        {
            m_voiceChat.ToggleMute(val);
        }

        public void ToggleChat(bool val)
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
