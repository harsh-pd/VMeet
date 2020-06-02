using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fordi.UI.MenuControl;

namespace Fordi.Core
{
    public class AbstractExperience : Gameplay
    {
        private ExperienceGroup[] m_locations;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_locations = m_commonResource.AssetDb.AbstractLocationsGroups;
            m_music = m_commonResource.AssetDb.AbstractMusic;
        }

        public override ExperienceResource[] GetResource(ResourceType resourceType, string category)
        {
            ExperienceResource[] resources = base.GetResource(resourceType, category);
            if (resources != null)
                return resources;

            if (resourceType == ResourceType.LOCATION)
                return Array.Find(m_locations, item => item.Name.Equals(category)).Resources;

            if (resourceType == ResourceType.MUSIC)
                return Array.Find(m_music, item => item.Name.Equals(category)).Resources;

            return null;
        }

        public override ResourceComponent[] GetCategories(ResourceType resourceType)
        {
            ResourceComponent[] categories = base.GetCategories(resourceType);
            if (categories != null)
                return categories;

            if (resourceType == ResourceType.LOCATION)
                return m_locations;
            if (resourceType == ResourceType.MUSIC)
                return m_music;

            return null;
        }
    }
}
