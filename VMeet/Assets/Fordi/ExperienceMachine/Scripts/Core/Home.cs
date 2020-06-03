using Cornea.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Fordi.Common;
using Fordi.UI.MenuControl;
using Fordi.UI;

namespace Fordi.Core
{
    public class Home : Experience
    {
        [SerializeField]
        private ExperienceResource[] m_experiences;
        [SerializeField]
        private GameObject m_glow;

        public const string HOME_SCENE = "Home";

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_music = m_commonResource.AssetDb.HomeMusic;
        }

        public override void ExecuteMenuCommand(MenuClickArgs args)
        {
            base.ExecuteMenuCommand(args);
            if (args.CommandType == MenuCommandType.QUIT || args.CommandType == MenuCommandType.MAIN || args.CommandType == MenuCommandType.SETTINGS || args.CommandType == MenuCommandType.SAVE_PRESET || args.CommandType == MenuCommandType.LOBBY)
                return;

            if (args.CommandType == MenuCommandType.LOGOUT)
            {
                ToggleMenu();
                OpenLoginPage();
                return;
            }

            if (args.CommandType == MenuCommandType.EXPERIENCE || args.CommandType == MenuCommandType.TRAINING)
            {
                if (args.CommandType == MenuCommandType.TRAINING)
                    ExperienceMachine.AppMode = AppMode.TRAINING;
                else
                    ExperienceMachine.AppMode = AppMode.APPLICATION;

                OpenResourceWindow(m_commonResource.GetGuideClip(MenuCommandType.EXPERIENCE), m_experiences, "WHICH TYPE OF MEDITATION ARE YOU UP FOR?");
                return;
            }

            if (args.CommandType == MenuCommandType.SELECTION)
            {
                m_experienceMachine.UpdateResourceSelection(args);

                ResourceType resourceType = ResourceType.OBJECT;
                if (args.Data != null && args.Data is ExperienceResource experienceResource)
                    resourceType = experienceResource.ResourceType;
                if (!(args.Data != null && (args.Data is ColorGroup || resourceType == ResourceType.EXPERIENCE)))
                    m_uiEngine.CloseLastScreen();
            }


            var experience = m_experienceMachine.GetExperience(m_menuSelection.ExperienceType);

            List<ResourceType> sequence = new List<ResourceType>();

            if (ExperienceMachine.AppMode == AppMode.APPLICATION)
                sequence = experience.MenuSequence;
            else
                sequence = experience.LearnMenuSequence;

            if (args.CommandType == MenuCommandType.CATEGORY_SELECTION)
            {
                ResourceComponent resourceComponent = (ResourceComponent)args.Data;
                
                if (resourceComponent.SpecialCommand == MandalaExperience.ColorBasedAudioCommand)
                {
                    m_menuSelection.VoiceOver = null;
                    m_uiEngine.CloseLastScreen();
                }
                else
                {
                    if (resourceComponent.ResourceType == ResourceType.AUDIO)
                        m_menuSelection.MusicGroup = Array.Find(m_commonResource.AssetDb.AudioGroups, item => item.Name != null && item.Name.Equals(args.Command)).MusicGroupName;
                    var resourceType = resourceComponent.ResourceType;
                    m_menu.OpenGridMenu(null, ResourceToMenuItems(experience.GetResource(resourceType, args.Command)), "SELECT " + resourceType.ToString().ToUpper(), true);
                    return;
                }
            }


            var sequenceIndex = sequence.FindIndex(item => item == ((ResourceComponent)args.Data).ResourceType);

            //Debug.LogError(args.CommandType + " " + ((ExperienceResource)args.Data).ResourceType + " " + sequenceIndex);

            sequenceIndex++;

            if (sequenceIndex < sequence.Count)
            {
                var categories = experience.GetCategories(sequence[sequenceIndex]);
                var resourceType = sequence[sequenceIndex];

                if (resourceType == ResourceType.COLOR || categories.Length == 0 || (categories.Length == 1 && string.IsNullOrEmpty(categories[0].Name)))
                {
                    if (resourceType != ResourceType.COLOR)
                    {
                        m_menu.OpenGridMenu(m_commonResource.GetGuideClip(GetCommandType(resourceType)) , ResourceToMenuItems(experience.GetResource(sequence[sequenceIndex], "")), "SELECT " + sequence[sequenceIndex].ToString(), true);
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
                        m_uiEngine.OpenColorInterface(colorInterfaceArgs);
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
                        default:
                            break;
                    }

                    MenuItemInfo[] categoryItems = GetCategoryMenu(categories, resourceType);
                    m_menu.OpenGridMenu(m_commonResource.GetGuideClip(GetCommandType(resourceType)), categoryItems, categoryDescription, true);
                }
                //m_menu.OpenGridMenu(sequence[++m_sequenceIterator], sequence[0][m_sequenceIterator - 1].Text);
            }
            else
            {
                m_menu.Close();
                m_experienceMachine.LoadExperience();
                return;
            }
        }

        private void GetNextMenu(string currentCommand)
        {
            
        }

        protected override void ExecuteSelectionCommand(MenuClickArgs args)
        {
            base.ExecuteSelectionCommand(args);
            ExperienceResource experienceResource = null;
            if (args.Data is ExperienceResource)
                experienceResource = (ExperienceResource)args.Data;

            if (experienceResource == null)
            {
                //Debug.LogError("Experience resource null");
                return;
            }

            if (experienceResource.ResourceType == ResourceType.EXPERIENCE)
            {
                if (Enum.TryParse(experienceResource.Name.ToUpper(), out ExperienceType type))
                    m_menuSelection.ExperienceType = type;
            }
        }

        public override void Pause()
        {
            base.Pause();
            Debug.LogError("In home");
        }

        public override void Play()
        {
            throw new System.NotImplementedException();
        }

        public override void Resume()
        {
            base.Resume();
            Debug.LogError("In home");
        }

        public override void Stop()
        {
            base.Stop();
            Debug.LogError("Already in home");
        }

        public override void UpdateResourceSelection(MenuClickArgs args)
        {
            base.UpdateResourceSelection(args);
            if (args.Data != null && args.Data is ExperienceResource)
            {
                ExperienceResource resource = (ExperienceResource)args.Data;
                if (Enum.TryParse(args.Name.ToUpper(), out ExperienceType type))
                    m_menuSelection.ExperienceType = type;
            }
        }

        public override void ToggleMenu()
        {
            if (m_uiEngine == null)
            {
                m_uiEngine = IOC.Resolve<IUIEngine>();
            }

            if (!m_uiEngine.IsOpen)
            {
                base.ToggleMenu();
                //m_glow.SetActive(true);
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


        public override void OnLoad()
        {
            m_menuSelection.VoiceOver = null;
            base.OnLoad();
            Invoke("OpenLoginPage", .1f);
            if (m_player == null)
                m_player = IOC.Resolve<IPlayer>();

            m_player.RequestHaltMovement(true);
        }


        private void OpenLoginPage()
        {
            var organizationInput = new MenuItemInfo
            {
                Path = "Organization",
                Text = "Organization",
                Command = "Organization",
                Icon = null,
                Data = TMP_InputField.ContentType.Standard,
                CommandType = MenuCommandType.FORM_INPUT
            };

            var usernameInput = new MenuItemInfo
            {
                Path = "Username",
                Text = "Username",
                Command = "Username",
                Icon = null,
                Data = TMP_InputField.ContentType.Standard,
                CommandType = MenuCommandType.FORM_INPUT
            };

            var passwordInput = new MenuItemInfo
            {
                Path = "Password",
                Text = "Password",
                Command = "Password",
                Icon = null,
                Data = TMP_InputField.ContentType.Password,
                CommandType = MenuCommandType.FORM_INPUT
            };

            MenuItemInfo[] formItems = new MenuItemInfo[] {organizationInput, usernameInput, passwordInput };
            FormArgs args = new FormArgs(formItems, "LOGIN", "Login", (inputs) => { m_webInterace.ValidateUserLogin(inputs[0], inputs[1], inputs[2]); });
            m_uiEngine.OpenForm(args);
        }
    }
}
