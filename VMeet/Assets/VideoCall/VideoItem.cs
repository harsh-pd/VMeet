using agora_gaming_rtc;
using Fordi.UI;
using Fordi.UI.MenuControl;
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

        public void DataBind(IUserInterface userInterface, MenuItemInfo item)
        {
            m_userInterface = userInterface;

            m_videoSurface.SetForUser((uint)item.Data);
            m_videoSurface.SetEnable(true);
            m_videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            m_videoSurface.SetGameFps(30);
        }

        private void OnMicToggle(bool val)
        {
            m_micIndicator.sprite = val ? m_micOn : m_micOff;
        }
    }
}
