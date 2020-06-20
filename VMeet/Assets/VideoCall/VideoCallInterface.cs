using agora_gaming_rtc;
using Fordi.VideoCall;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.UI.MenuControl
{
    public class VideoCallInterface : MenuScreen
    {
        [SerializeField]
        private VideoItem m_preseterVideoItem;

        public override void OpenMenu(IUserInterface userInterface, MenuArgs args)
        {
            base.OpenMenu(userInterface, args);
            if (m_title != null)
                m_title.text = args.Title;
        }

        internal void AddVideo(MenuItemInfo item)
        {
            SpawnMenuItem(item, m_menuItem, m_contentRoot);
        }

        public void Present(MenuItemInfo item)
        {
            m_preseterVideoItem.DataBind(m_userInterface, item);
            m_preseterVideoItem.OnVideoMute(false);
        }

        public void StopPresenting()
        {
            m_preseterVideoItem.OnVideoMute(true);
        }
    }
}
