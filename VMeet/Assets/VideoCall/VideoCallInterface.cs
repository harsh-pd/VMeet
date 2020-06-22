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

        [SerializeField]
        private ProcessButton m_videoToggle, m_micToggle, m_fulscreenToggle, m_chatToggle;

        private IVideoCallEngine m_videoCallEngine;
        private IVoiceChat m_voiceChat;

        private Chat m_chat;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_videoCallEngine = IOC.Resolve<IVideoCallEngine>();
            m_voiceChat = IOC.Resolve<IVoiceChat>();

            m_videoToggle.onValueChangeRequest.AddListener(ToggleVideo);
            m_micToggle.onValueChangeRequest.AddListener(ToggleMic);
            m_fulscreenToggle.onValueChangeRequest.AddListener(ToggleFulscreen);
            if (m_chatToggle)
                m_chatToggle.onValueChangeRequest.AddListener(ToggleChat);
            m_videoToggle.IsOn = m_videoCallEngine.VideoEnabled;

            m_videoCallEngine.VideoPauseToggle += VideoPauseTOggle;
        }

        private void VideoPauseTOggle(object sender, VideoEventArgs e)
        {
            if (e.UserId == 0)
                m_videoToggle.IsOn = !e.Pause;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            m_videoCallEngine.VideoPauseToggle -= VideoPauseTOggle;
            m_videoToggle.onValueChangeRequest.RemoveAllListeners();
            m_micToggle.onValueChangeRequest.RemoveAllListeners();
            m_fulscreenToggle.onValueChangeRequest.RemoveAllListeners();
            if (m_chatToggle)
                m_chatToggle.onValueChangeRequest.RemoveAllListeners();
        }

        internal void AddVideo(MenuItemInfo item)
        {
            Debug.LogError("AddVideo");
            SpawnMenuItem(item, m_menuItem, m_contentRoot);
        }

        public void RemoveVideo(uint userId)
        {
            var videoItem = m_menuItems.Find(item => ((AgoraUserInfo)item.Item.Data).UserId == userId);

            if (videoItem != null)
            {
                m_menuItems.Remove(videoItem);
                Destroy(videoItem.Gameobject);
            }
        }

        public void Present(MenuItemInfo item)
        {
            m_presenterVideoItem.DataBind(m_userInterface, item);
        }

        public void StopPresenting()
        {
            m_presenterVideoItem.OnVideoMute(true);
        }

        public void ToggleVideo(bool val, Action<bool> done)
        {
            var result = m_videoCallEngine.EnableVideo(!val);
        }

        public void ToggleFulscreen(bool val, Action<bool> done)
        {
            m_participantsGrid.gameObject.SetActive(!val);
            m_onHoverMenu.ToggleFulScreen(val);
            done.Invoke(true);
        }

        public void ToggleMic(bool val, Action<bool> done)
        {
            m_voiceChat.ToggleMute(val);
            done.Invoke(true);
        }

        public void ToggleChat(bool val, Action<bool> done)
        {
            if (!val)
            {
                if (m_chat != null)
                    m_chat.gameObject.SetActive(false);

                done.Invoke(true);
                return;
            }
            if (m_chat == null)
                m_chat = Instantiate(m_chatPrefab, m_chatRoot);
            else
                m_chat.gameObject.SetActive(true);
            done.Invoke(true);
        }
    }
}
