using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRExperience.Core
{
    public class Lobby : Gameplay
    {
        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_music = m_commonResource.AssetDb.LobbyMusic;
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
    }
}
