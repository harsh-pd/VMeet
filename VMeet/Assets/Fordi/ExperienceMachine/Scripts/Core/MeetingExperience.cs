using Fordi.ScreenSharing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRExperience.Common;
using VRExperience.UI.MenuControl;

namespace VRExperience.Core
{
    public class MeetingExperience : Gameplay
    {
        protected IScreenShare m_screenShare = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_music = m_commonResource.AssetDb.MeetingMusic;
            m_screenShare = IOC.Resolve<IScreenShare>();
        }

        public override ExperienceResource[] GetResource(ResourceType resourceType, string category)
        {
            ExperienceResource[] resources = base.GetResource(resourceType, category);
            if (resources != null)
                return resources;

            if (resourceType == ResourceType.MUSIC)
                return Array.Find(m_music, item => item.Name.Equals(category)).Resources;

            return null;
        }

        public override ResourceComponent[] GetCategories(ResourceType resourceType)
        {
            ResourceComponent[] categories = base.GetCategories(resourceType);
            if (categories != null)
                return categories;

            if (resourceType == ResourceType.MUSIC)
                return m_music;

            return null;
        }

        public override void OnLoad()
        {
            base.OnLoad();
            Debug.LogError("OnLoad");
            m_vrMenu.LoadRemoteDesktopView(m_insceneMenuItems);
            m_screenShare.Initialize();
        }
    }
}
