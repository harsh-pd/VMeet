using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fordi.Common;
using Fordi.UI.MenuControl;
using System;
using TMPro;
using Fordi.UI;
using Fordi.VideoCall;
using Photon.Pun;

namespace Fordi.Core
{
    public abstract class Gameplay : Experience
    {
        public override void ExecuteMenuCommand(MenuClickArgs args)
        {
            base.ExecuteMenuCommand(args);

            if (args.CommandType == MenuCommandType.LOGOUT)
            {
                m_menuSelection.Location = Home.HOME_SCENE;
                m_menuSelection.ExperienceType = ExperienceType.HOME;
                ToggleMenu();
                m_experienceMachine.LoadExperience();
            }

            if (args.CommandType == MenuCommandType.VIDEO_CONFERENCE)
            {
                m_uiEngine.AddVideo(new MenuItemInfo()
                {
                    Data = new AgoraUserInfo()
                    {
                        UserId = 0,
                        Name = PhotonNetwork.NickName
                    },
                });
            }

            if (args.CommandType == MenuCommandType.CATEGORY_SELECTION)
            {
                var resourceType = ((ResourceComponent)args.Data).ResourceType;
                if (resourceType == ResourceType.OBJECT)
                    m_uiEngine.OpenObjectInterface(new GridArgs()
                    {
                        AudioClip = null,
                        Items = ResourceToMenuItems(m_commonResource.GetResource(resourceType, args.Command)),
                        Title = "PICK ITEM",
                    });
                else
                    m_uiEngine.OpenGridMenu(new GridArgs()
                    {
                        Items = ResourceToMenuItems(GetResource(resourceType, args.Command)),
                        Title = "SELECT " + resourceType.ToString().ToUpper(),
                    });
                return;
            }

            if (args.CommandType == MenuCommandType.VO)
            {
                var categories = GetCategories(ResourceType.AUDIO);
                if (categories.Length == 0 || (categories.Length == 1 && string.IsNullOrEmpty(categories[0].Name)))
                    m_uiEngine.OpenGridMenu(new GridArgs()
                    {
                        AudioClip = m_commonResource.GetGuideClip(MenuCommandType.VO),
                        Items = ResourceToMenuItems(GetResource(ResourceType.AUDIO, "")),
                        Title = "SELECT AUDIO",
                    });
                else
                    m_uiEngine.OpenGridMenu(new GridArgs()
                    {
                        AudioClip = m_commonResource.GetGuideClip(MenuCommandType.VO),
                        Items = GetCategoryMenu(categories, ResourceType.AUDIO),
                        Title = "WHICH MEDITATION SUITS YOUR MOOD?",
                    });
            }

            else if (args.CommandType == MenuCommandType.SELECTION)
            {
                m_uiEngine.Close();
                AudioSelectionFlag = false;
            }
        }

        /// <summary>
        /// Gameplay resource selection happens here.
        /// </summary>
        /// <param name="args"></param>
        public override void UpdateResourceSelection(MenuClickArgs args)
        {
            base.UpdateResourceSelection(args);
            if (args.Data != null && args.Data is ExperienceResource)
            {
                ExperienceResource resource = (ExperienceResource)args.Data;

                if (resource.ResourceType == ResourceType.AUDIO)
                {
                    m_menuSelection.VoiceOver = ((AudioResource)resource).Clip;

                    AudioArgs voArgs = new AudioArgs(m_menuSelection.VoiceOver, AudioType.VO);
                    voArgs.FadeTime = 2;
                    m_audio.Play(voArgs);
                    AudioSelectionFlag = true;
                }
            }

            //Apply selection to gameplay.
            //Selection can be accessed from m_menuSelection object.

        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (m_menuSelection.VoiceOver == null || ExperienceMachine.AppMode == AppMode.TRAINING)
                return;
            AudioArgs voArgs = new AudioArgs(m_menuSelection.VoiceOver, AudioType.VO);
            voArgs.FadeTime = 2;
            m_audio.Play(voArgs);
        }
    }
}
