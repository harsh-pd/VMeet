using Cornea.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRExperience.Meeting;
using VRExperience.UI.MenuControl;

namespace VRExperience.Core
{
    public class Lobby : Gameplay
    {
        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_music = m_commonResource.AssetDb.LobbyMusic;
        }

        public override void ExecuteMenuCommand(MenuClickArgs args)
        {
            if (args.CommandType == MenuCommandType.QUIT || args.CommandType == MenuCommandType.MAIN || args.CommandType == MenuCommandType.SETTINGS || args.CommandType == MenuCommandType.SAVE_PRESET || args.CommandType == MenuCommandType.LOBBY)
            {
                base.ExecuteMenuCommand(args);
                return;
            }

            if (args.CommandType == MenuCommandType.MEETING || args.CommandType == MenuCommandType.TRAINING)
            {
                if (args.CommandType == MenuCommandType.TRAINING)
                    ExperienceMachine.AppMode = AppMode.TRAINING;
                else
                    ExperienceMachine.AppMode = AppMode.APPLICATION;

                //m_webInterace.ListAllMeetingDetails(MeetingFilter.Rejected).OnRequestComplete(
                //(isNetworkError, message) =>
                //{
                //    var allCreatedMeetings = m_webInterace.ParseMeetingListJson(message, MeetingCategory.CREATED);

                //    if (resourceComponent.ResourceType == ResourceType.AUDIO)
                //        m_menuSelection.MusicGroup = Array.Find(m_commonResource.AssetDb.AudioGroups, item => item.Name != null && item.Name.Equals(args.Command)).MusicGroupName;
                //    var resourceType = resourceComponent.ResourceType;
                //    m_menu.OpenGridMenu(null, ResourceToMenuItems(experience.GetResource(resourceType, args.Command)), "SELECT " + resourceType.ToString().ToUpper());

                //    OpenResourceWindow(m_commonResource.GetGuideClip(MenuCommandType.EXPERIENCE), m_webInterace.Meetings, "WHICH TYPE OF MEDITATION ARE YOU UP FOR?");
                //});
                //return;
            }

            if (args.CommandType == MenuCommandType.SELECTION)
            {
                m_experienceMachine.UpdateResourceSelection(args);

                ResourceType resourceType = ResourceType.OBJECT;
                if (args.Data != null && args.Data is ExperienceResource experienceResource)
                    resourceType = experienceResource.ResourceType;
                if (!(args.Data != null && (args.Data is ColorGroup || resourceType == ResourceType.EXPERIENCE)))
                    m_vrMenu.CloseLastScreen();
            }


            List<ResourceType> sequence = new List<ResourceType>();

            if (ExperienceMachine.AppMode == AppMode.APPLICATION)
                sequence = MenuSequence;
            else
                sequence = LearnMenuSequence;

            if (args.CommandType == MenuCommandType.CATEGORY_SELECTION)
            {
                ResourceComponent resourceComponent = (ResourceComponent)args.Data;

                if (resourceComponent.SpecialCommand == MandalaExperience.ColorBasedAudioCommand)
                {
                    m_menuSelection.VoiceOver = null;
                    m_vrMenu.CloseLastScreen();
                }
                else
                {
                    if (resourceComponent.ResourceType == ResourceType.AUDIO)
                        m_menuSelection.MusicGroup = Array.Find(m_commonResource.AssetDb.AudioGroups, item => item.Name != null && item.Name.Equals(args.Command)).MusicGroupName;
                    var resourceType = resourceComponent.ResourceType;
                    m_menu.OpenGridMenu(null, ResourceToMenuItems(m_webInterace.GetResource(resourceType, args.Command)), "SELECT " + resourceType.ToString().ToUpper());
                    return;
                }
            }

            //Debug.LogError(args.Data.GetType().ToString());

            int sequenceIndex;

            if (args.Data == null)
                sequenceIndex = sequence.FindIndex(item => item == GetResourceType(args.CommandType));
            else
                sequenceIndex = sequence.FindIndex(item => item == ((ResourceComponent)args.Data).ResourceType);

            //Debug.LogError(args.CommandType + " " + ((ExperienceResource)args.Data).ResourceType + " " + sequenceIndex);

            if (sequenceIndex < sequence.Count)
            {
                var resourceType = sequence[sequenceIndex];
                m_webInterace.GetCategories(resourceType, (categories) =>
                {
                    if (resourceType == ResourceType.COLOR || (categories.Length == 1 && string.IsNullOrEmpty(categories[0].Name)))
                    {
                        if (resourceType != ResourceType.COLOR)
                        {
                            m_menu.OpenGridMenu(m_commonResource.GetGuideClip(GetCommandType(resourceType)), ResourceToMenuItems(m_webInterace.GetResource(sequence[sequenceIndex], "")), "SELECT " + sequence[sequenceIndex].ToString());
                        }
                        else
                        {
                            ColorInterfaceArgs colorInterfaceArgs = new ColorInterfaceArgs
                            {
                                Blocked = false,
                                Persist = true,
                                ColorGroup = new ColorGroup
                                {
                                    ResourceType = ResourceType.COLOR,
                                    Preview = null,
                                    Resources = (ColorResource[])m_experienceMachine.GetExperience(ExperienceType.MANDALA).GetResource(ResourceType.COLOR, MandalaExperience.MainColor)
                                },
                                Preset1 = new ColorGroup
                                {
                                    ResourceType = ResourceType.COLOR,
                                    Preview = null,
                                    Resources = m_menuSelection.MandalaResource.Preset1
                                },
                                CustomPreset = new ColorGroup
                                {
                                    ResourceType = ResourceType.COLOR,
                                    Preview = null,
                                    Resources = m_menuSelection.MandalaResource.CustomPreset
                                },
                                Title = "CHOOSE YOUR COLOR COMBINATION",
                                GuideClip = m_commonResource.GetGuideClip(MenuCommandType.COLOR)
                            };
                            m_vrMenu.OpenColorInterface(colorInterfaceArgs);
                        }
                    }
                    else
                    {
                        string categoryDescription = "";

                        switch (resourceType)
                        {
                            case ResourceType.MUSIC:
                                categoryDescription = "WHAT MUSIC IS THE RIGHT FIT?";
                                break;
                            case ResourceType.COLOR:
                                categoryDescription = "WHAT COLOR SUITS YOUR MOOD?";
                                break;
                            case ResourceType.MANDALA:
                                categoryDescription = "WHICH MANDALA SHOULD BE GOOD?";
                                break;
                            case ResourceType.LOCATION:
                                categoryDescription = "WHERE WOULD YOU LIKE TO RELAX?";
                                break;
                            case ResourceType.AUDIO:
                                categoryDescription = "WHICH MEDITATION SUITS YOUR MOOD?";
                                break;
                            case ResourceType.MEETING:
                                categoryDescription = "SELECT MEETING TYPE TO PROCEED";
                                break;
                            default:
                                break;
                        }

                        MenuItemInfo[] categoryItems = GetCategoryMenu(categories, resourceType);
                        m_menu.OpenGridMenu(m_commonResource.GetGuideClip(GetCommandType(resourceType)), categoryItems, categoryDescription);
                    }
                });//m_menu.OpenGridMenu(sequence[++m_sequenceIterator], sequence[0][m_sequenceIterator - 1].Text);
            }
            else
            {
                Debug.LogError(sequenceIndex + " " + sequence.Count);
                m_menu.Close();
                m_experienceMachine.LoadExperience();
                return;
            }
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

        //public override void ToggleMenu()
        //{
            
        //}
    }
}
