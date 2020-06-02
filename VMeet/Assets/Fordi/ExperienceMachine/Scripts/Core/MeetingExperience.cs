using Fordi.ScreenSharing;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fordi.Common;
using Fordi.UI.MenuControl;

namespace Fordi.Core
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
            m_vrMenu.LoadRemoteDesktopView(m_insceneMenuItems);
            StartCoroutine(TakeASeat());
        }

        private IEnumerator TakeASeat()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            //Debug.LogError(PhotonNetwork.LocalPlayer.ActorNumber);
            if (PhotonNetwork.LocalPlayer.ActorNumber < 1)
            {
                if (m_teleportAnchors.Length > 0)
                    m_player.DoWaypointTeleport(m_teleportAnchors[0]);
                yield break;
            }
            if (m_teleportAnchors.Length > PhotonNetwork.LocalPlayer.ActorNumber - 1)
                m_player.DoWaypointTeleport(m_teleportAnchors[PhotonNetwork.LocalPlayer.ActorNumber - 1]);
        }
    }
}
