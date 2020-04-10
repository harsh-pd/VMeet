using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRExperience.Common;
using VRExperience.UI.MenuControl;
using System;
using TMPro;

namespace VRExperience.Core
{
    public abstract class Gameplay : Experience
    {
        [SerializeField]
        private MenuItemInfo[] m_insceneMenuItems = new MenuItemInfo[] { };

        public override void ExecuteMenuCommand(MenuClickArgs args)
        {
            base.ExecuteMenuCommand(args);

            if (args.CommandType == MenuCommandType.CATEGORY_SELECTION)
            {
                var resourceType = ((ResourceComponent)args.Data).ResourceType;
                if (resourceType == ResourceType.OBJECT)
                    m_vrMenu.OpenObjectInterface(null, ResourceToMenuItems(m_commonResource.GetResource(resourceType, args.Command)), "PICK ITEM");
                else
                    m_menu.OpenGridMenu(null, ResourceToMenuItems(GetResource(resourceType, args.Command)), "SELECT " + resourceType.ToString().ToUpper());
                return;
            }

            if (args.CommandType == MenuCommandType.VO)
            {
                var categories = GetCategories(ResourceType.AUDIO);
                if (categories.Length == 0 || (categories.Length == 1 && string.IsNullOrEmpty(categories[0].Name)))
                    m_menu.OpenGridMenu(m_commonResource.GetGuideClip(MenuCommandType.VO), ResourceToMenuItems(GetResource(ResourceType.AUDIO, "")), "SELECT AUDIO");
                else
                    m_menu.OpenGridMenu(m_commonResource.GetGuideClip(MenuCommandType.VO), GetCategoryMenu(categories, ResourceType.AUDIO), "WHICH MEDITATION SUITS YOUR MOOD?");
            }

            else if (args.CommandType == MenuCommandType.SELECTION)
            {
                m_vrMenu.Close();
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

        public override void ToggleMenu()
        {
            if (m_vrMenu.IsOpen)
                m_menu.Close();
            else
                m_menu.Open(m_insceneMenuItems);
        }

        public override void OnLoad()
        {
            base.OnLoad();

            m_vrMenu.SwitchStandaloneMenu();

            if (m_menuSelection.VoiceOver == null || ExperienceMachine.AppMode == AppMode.TRAINING)
                return;
            AudioArgs voArgs = new AudioArgs(m_menuSelection.VoiceOver, AudioType.VO);
            voArgs.FadeTime = 2;
            m_audio.Play(voArgs);
        }
    }
}
