using agora_gaming_rtc;
using Fordi.Common;
using Fordi.UI;
using Fordi.UI.MenuControl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fordi.VideoCall
{
    public class VideoItem : MonoBehaviour, IMenuItem
    {
        [SerializeField]
        private VideoSurface m_videoSurface;
        [SerializeField]
        private Button m_button;
        [SerializeField]
        private Image m_micIndicator;
        [SerializeField]
        private Sprite m_micOn, m_micOff;
        [SerializeField]
        private Image m_muteDisplay;

        protected MenuItemInfo m_item;
        public MenuItemInfo Item
        {
            get { return m_item; }
            //set
            //{
            //    if (m_item != value)
            //    {
            //        m_item = value;
            //        DataBind();
            //    }
            //}
        }

        public Selectable Selectable { get { return m_button; } }

        private IUserInterface m_userInterface;
        private IVideoCallEngine m_videoCallEngine;

        public GameObject Gameobject { get { return gameObject; } }

        private AgoraUserInfo m_userInfo;

        private void Awake()
        {
            m_videoCallEngine = IOC.Resolve<IVideoCallEngine>();
            m_videoCallEngine.VideoPauseToggle += VideoPauseToggle;
        }

        private void OnDestroy()
        {
            m_videoCallEngine.VideoPauseToggle -= VideoPauseToggle;
        }

        private void VideoPauseToggle(object sender, VideoEventArgs e)
        {
            if (m_userInfo == null)
                return;
            
            if (e.UserId == m_userInfo.UserId)
                OnVideoMute(e.Pause);
        }

        public void DataBind(IUserInterface userInterface, MenuItemInfo item)
        {
            m_userInterface = userInterface;

            m_userInfo = (AgoraUserInfo)item.Data;
            m_videoSurface.SetForUser(m_userInfo.UserId);
            m_videoSurface.SetEnable(true);
            m_videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            m_videoSurface.SetGameFps(30);
            
        }

        private void OnMicToggle(bool val)
        {
            m_micIndicator.sprite = val ? m_micOn : m_micOff;
        }

        public void OnVideoMute(bool val)
        {
            m_muteDisplay.gameObject.SetActive(val);
            m_videoSurface.gameObject.SetActive(!val);
        }
    }
}
