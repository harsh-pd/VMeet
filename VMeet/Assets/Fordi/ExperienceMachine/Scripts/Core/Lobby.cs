using Cornea.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fordi.Meeting;
using Fordi.UI.MenuControl;
using Fordi.UI;
using Fordi.Common;

namespace Fordi.Core
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
            if (args.Data != null && args.Data is AudioGroup || args.Data is AudioResource)
            {
                base.ExecuteMenuCommand(args);
                return;
            }

            if (args.CommandType == MenuCommandType.CATEGORY_SELECTION && ((ResourceComponent)args.Data).ResourceType == ResourceType.OBJECT)
            {
                base.ExecuteMenuCommand(args);
                return;
            }

            if ((!(args.Data is MeetingResource) && args.CommandType != MenuCommandType.CATEGORY_SELECTION) && args.CommandType != MenuCommandType.MEETING && args.CommandType != MenuCommandType.USER && args.CommandType != MenuCommandType.CREATE_MEETING)
                base.ExecuteMenuCommand(args);

            if (args.CommandType == MenuCommandType.LOGOUT || args.CommandType == MenuCommandType.ANNOTATION || args.CommandType == MenuCommandType.MUSIC || args.CommandType == MenuCommandType.VO || args.CommandType == MenuCommandType.QUIT || args.CommandType == MenuCommandType.MAIN || args.CommandType == MenuCommandType.SETTINGS || args.CommandType == MenuCommandType.SAVE_PRESET || args.CommandType == MenuCommandType.LOBBY)
                return;

            if (args.CommandType == MenuCommandType.CREATE_MEETING)
            {
                m_webInterace.GetCategories(ResourceType.USER, (users) =>
                {
                    m_uiEngine.OpenMeetingForm(new FormArgs()
                    {
                        Items = ResourceToMenuItems(m_webInterace.GetResource(ResourceType.USER, "").Where(item => ((UserResource)item).UserInfo.id != m_webInterace.UserInfo.id).ToArray()),
                    });
                });
               
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
                //    m_uiEngine.OpenGridMenu(null, ResourceToMenuItems(experience.GetResource(resourceType, args.Command)), "SELECT " + resourceType.ToString().ToUpper());

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
                if (args.Data != null && args.Data is MeetingResource)
                {
                    m_uiEngine.OpenMeeting(new UI.MeetingArgs()
                    {
                      MeetingInfo = ((MeetingResource)args.Data).MeetingInfo
                    });
                    return;
                }
                if (!(args.Data != null && (args.Data is ColorGroup || resourceType == ResourceType.EXPERIENCE)))
                    m_uiEngine.CloseLastScreen();
            }


            List<ResourceType> sequence = new List<ResourceType>();

            if (ExperienceMachine.AppMode == AppMode.APPLICATION)
                sequence = MenuSequence;
            else
                sequence = LearnMenuSequence;

            if (args.CommandType == MenuCommandType.CATEGORY_SELECTION)
            {
                ResourceComponent resourceComponent = (ResourceComponent)args.Data;

                if (resourceComponent.ResourceType == ResourceType.AUDIO)
                    m_menuSelection.MusicGroup = Array.Find(m_commonResource.AssetDb.AudioGroups, item => item.Name != null && item.Name.Equals(args.Command)).MusicGroupName;
                var resourceType = resourceComponent.ResourceType;
                var resources = resourceType == ResourceType.OBJECT ? m_commonResource.GetResource(resourceType, args.Command) : m_webInterace.GetResource(resourceType, args.Command);
                m_uiEngine.OpenGridMenu(new GridArgs()
                {
                    Items = ResourceToMenuItems(resources),
                    Title = "SELECT " + resourceType.ToString().ToUpper(),
                    RefreshCategory = args.Command
                });
                return;
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
                            m_uiEngine.OpenGridMenu(new GridArgs()
                            {
                                AudioClip = m_commonResource.GetGuideClip(GetCommandType(resourceType)),
                                Items = ResourceToMenuItems(m_webInterace.GetResource(sequence[sequenceIndex], "")),
                                Title = "SELECT " + sequence[sequenceIndex].ToString(),
                            });
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
                        m_uiEngine.OpenGridMenu(new GridArgs()
                        {
                            AudioClip = m_commonResource.GetGuideClip(GetCommandType(resourceType)),
                            Items = categoryItems,
                            Title = categoryDescription,
                        });
                    }
                });//m_uiEngine.OpenGridMenu(sequence[++m_sequenceIterator], sequence[0][m_sequenceIterator - 1].Text);
            }
            else
            {
                Debug.LogError(args.Data.GetType().ToString() + " " + args.CommandType.ToString());
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

        public override void OnLoad()
        {
            base.OnLoad();
            StartCoroutine(TakeASeat());
        }

        private IEnumerator TakeASeat()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            if (m_teleportAnchors.Length > 0)
                m_experienceMachine.Player.DoWaypointTeleport(m_teleportAnchors[0]);

            yield return new WaitForSeconds(.2f);
            m_uiEngine.OpenStandaloneMenu(new MenuArgs() { Items = m_menu.Items });
        }
    }
}
