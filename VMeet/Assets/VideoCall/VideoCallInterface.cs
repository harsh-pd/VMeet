using agora_gaming_rtc;
using Fordi.Common;
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
        private VideoItem m_presenterVideoItem;

        [SerializeField]
        private GameObject m_participantsGrid;

        [SerializeField]
        private MenuOnHover m_onHoverMenu;

        private IVideoCallEngine m_videoCallEngine;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_videoCallEngine = IOC.Resolve<IVideoCallEngine>();
        }

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
    }
}
