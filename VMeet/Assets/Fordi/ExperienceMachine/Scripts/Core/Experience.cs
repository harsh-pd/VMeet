using Cornea.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRExperience.Common;
using VRExperience.UI.MenuControl;

namespace VRExperience.Core
{
    public enum TooltipOperation
    {
        AND,
        OR
    }

    public enum ResourceType
    {
        MUSIC,
        COLOR,
        MANDALA,
        LOCATION,
        AUDIO,
        EXPERIENCE,
        GUIDE_AUDIO,
        SFX,
        AMBIENCE_MUSIC,
        COLOR_AUDIO,
        OBJECT,
        MEETING,
        USER
    }

    [Serializable]
    public struct VRButton
    {
        public string Tip;
        public OVRInput.Button Button;
        public OVRInput.Controller Controller;
        public AudioClip GuideClip;
    }

    [Serializable]
    public class VRButtonGroup
    {
        public string Name;
        public TooltipOperation TooltipOperation;
        public string CommonTip;
        public VRButton[] VRButtons;
        public Condition Condition;
    }

    [Serializable]
    public class ExperienceResource : ResourceComponent
    {
        public static implicit operator MenuItemInfo (ExperienceResource resource)
        {
            return new MenuItemInfo
            {
                Path = resource.Name,
                Text = resource.Name,
                Command = resource.Name,
                Icon = resource.Preview,
                Data = resource,
                CommandType = MenuCommandType.SELECTION
            };
        }
    }

    [Serializable]
    public class ColorResource : ExperienceResource, ICloneable
    {
        public Color Color;
        public string PrimaryDescription;
        public string SecondaryDescription;
        public string TertiaryDescription;
        [HideInInspector]
        public string ShortDescription;

        public object Clone()
        {
            return new ColorResource
            {
                Name = Name,
                Description = Description,
                ResourceType = ResourceType,
                Preview = Preview,
                LargePreview = LargePreview,
                Color = Color,
                PrimaryDescription = PrimaryDescription,
                SecondaryDescription = SecondaryDescription,
                TertiaryDescription = TertiaryDescription
            };
        }
    }

    [Serializable]
    public class AudioResource : ExperienceResource
    {
        public AudioClip Clip;
    }

    [Serializable]
    public class ObjectResource : ExperienceResource
    {
        public GameObject ObjectUIPrefab;
        public GameObject ObjectPrefab;
    }

    [Serializable]
    public class GuideAudioResource : AudioResource
    {
        public MenuCommandType CommandType;
    }

    [Serializable]
    public class MandalaResource : ExperienceResource
    {
        public string SceneName;
        public GameObject Mandala;
        public ColorResource[] Preset1;
        private ColorResource[] m_customPreset = null;
        public ColorResource[] CustomPreset { get { return m_customPreset; } set { m_customPreset = value; } }
        //public AudioClip Clip;
    }

    [RequireComponent(typeof(Menu))]
    public abstract class Experience : MonoBehaviour, IExperience
    {
        protected IExperienceMachine m_experienceMachine;
        protected IVRMenu m_vrMenu;
        protected IMenuSelection m_menuSelection;
        protected ISettings m_settings;
        protected IAudio m_audio;
        protected ICommonResource m_commonResource;
        protected IPlayer m_player;
        protected IWebInterface m_webInterace;

        [SerializeField]
        protected Menu m_menu;

        [SerializeField]
        private Transform[] m_teleportAnchors;

        [SerializeField]
        private List<VRButtonGroup> m_usedButtons;

        protected AudioGroup[] m_music;

        private int teleportAnchorIndex = -1;

        public static bool AudioSelectionFlag { get; protected set; } = false;

        protected void Awake()
        {
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_menu = GetComponent<Menu>();
            m_vrMenu = IOC.Resolve<IVRMenu>();
            m_menuSelection = IOC.Resolve<IMenuSelection>();
            m_audio = IOC.Resolve<IAudio>();
            m_commonResource = IOC.Resolve<ICommonResource>();
            m_player = IOC.Resolve<IPlayer>();
            m_settings = IOC.Resolve<ISettings>();
            m_webInterace = IOC.Resolve<IWebInterface>();
            AwakeOverride();
        }

        private void OnDestroy()
        {
            OnDestroyOverride();
        }

        protected virtual void AwakeOverride()
        {
            Init();
        }

        protected virtual void OnDestroyOverride() { }

        protected virtual void Init()
        {
            MenuSequence.Clear();
            LearnMenuSequence.Clear();

            foreach (var item in m_menu.Items)
            {
                switch (item.CommandType)
                {
                    case MenuCommandType.MUSIC:
                        MenuSequence.Add(ResourceType.MUSIC);
                        LearnMenuSequence.Add(ResourceType.MUSIC);
                        break;
                    case MenuCommandType.VO:
                        MenuSequence.Add(ResourceType.AUDIO);
                        break;
                    case MenuCommandType.MANDALA:
                        MenuSequence.Add(ResourceType.MANDALA);
                        LearnMenuSequence.Add(ResourceType.MANDALA);
                        break;
                    case MenuCommandType.COLOR:
                        MenuSequence.Add(ResourceType.COLOR);
                        LearnMenuSequence.Add(ResourceType.COLOR);
                        break;
                    case MenuCommandType.LOCATION:
                        MenuSequence.Add(ResourceType.LOCATION);
                        LearnMenuSequence.Add(ResourceType.LOCATION);
                        break;
                    case MenuCommandType.EXPERIENCE:
                        MenuSequence.Add(ResourceType.EXPERIENCE);
                        LearnMenuSequence.Add(ResourceType.EXPERIENCE);
                        break;
                    case MenuCommandType.MEETING:
                        MenuSequence.Add(ResourceType.MEETING);
                        LearnMenuSequence.Add(ResourceType.MEETING);
                        break;
                }
            }
        }

        protected MenuCommandType GetCommandType(ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.MUSIC:
                    return MenuCommandType.MUSIC;
                case ResourceType.COLOR:
                    return MenuCommandType.COLOR;
                case ResourceType.MANDALA:
                    return MenuCommandType.MANDALA;
                case ResourceType.LOCATION:
                    return MenuCommandType.LOCATION;
                case ResourceType.AUDIO:
                    return MenuCommandType.VO;
                case ResourceType.EXPERIENCE:
                    return MenuCommandType.EXPERIENCE;
                case ResourceType.OBJECT:
                    return MenuCommandType.INVENTORY;
            }

            return MenuCommandType.EXPERIENCE;
        }

        protected ResourceType GetResourceType(MenuCommandType commandType)
        {
            switch (commandType)
            {
                case MenuCommandType.MUSIC:
                    return ResourceType.MUSIC;
                case MenuCommandType.COLOR:
                    return ResourceType.COLOR;
                case MenuCommandType.MANDALA:
                    return ResourceType.MANDALA;
                case MenuCommandType.LOCATION:
                    return ResourceType.LOCATION;
                case MenuCommandType.VO:
                    return ResourceType.AUDIO;
                case MenuCommandType.EXPERIENCE:
                    return ResourceType.EXPERIENCE;
                case MenuCommandType.INVENTORY:
                    return ResourceType.OBJECT;
                case MenuCommandType.MEETING:
                    return ResourceType.MEETING;
            }

            Debug.LogError("Failed conversion");
            return ResourceType.MEETING;
        }


        protected MenuItemInfo[] ResourceToMenuItems(ExperienceResource[] resources)
        {
            MenuItemInfo[] menuItems = new MenuItemInfo[resources.Length];
            for (int i = 0; i < resources.Length; i++)
                menuItems[i] = resources[i];
            return menuItems;
        }

        protected void OpenResourceWindow(AudioClip guide, ExperienceResource[] resources, string windowTitle)
        {
            MenuItemInfo[] menuItems = ResourceToMenuItems(resources);
            m_menu.OpenGridMenu(guide, menuItems, windowTitle);
        }

        /// <summary>
        /// Preserve selection into MenuSelection singleton m_menuSelection
        /// </summary>
        /// <param name="args"></param>
        protected virtual void ExecuteSelectionCommand(MenuClickArgs args)
        {
            UpdateResourceSelection(args);
        }

        public virtual bool CanExecuteMenuCommand(string cmd)
        {
            return true;
        }

        public virtual void ExecuteMenuCommand(MenuClickArgs args)
        {
            //Debug.Log(args.Name + " " + args.Path + " " + args.Command + " " + args.CommandType.ToString());
            if (args.CommandType == MenuCommandType.SAVE_PRESET)
            {
                m_experienceMachine.GetExperience(ExperienceType.MANDALA).ExecuteMenuCommand(args);
                return;
            }

            if (args.CommandType == MenuCommandType.QUIT)
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Play");
                #else
                Application.Quit();
                #endif
                return;
            }

            if (args.CommandType == MenuCommandType.HOME)
            {
                m_menuSelection.Location = args.Path;
                m_menuSelection.ExperienceType = ExperienceType.HOME;
                ToggleMenu();
                m_experienceMachine.LoadExperience();
            }

            if (args.CommandType == MenuCommandType.LOBBY)
            {
                m_menuSelection.Location = args.Path;
                m_menuSelection.ExperienceType = ExperienceType.LOBBY;
                m_vrMenu.Close();
                m_experienceMachine.LoadExperience();
            }

            if (args.CommandType == MenuCommandType.SETTINGS)
            {
                m_vrMenu.OpenSettingsInterface(m_commonResource.GetGuideClip(MenuCommandType.SETTINGS));
            }

            if (args.CommandType == MenuCommandType.INVENTORY)
            {
                var resourceType = ResourceType.OBJECT;
                var categories = m_commonResource.GetCategories(ResourceType.OBJECT);

                if (categories.Length == 0 || (categories.Length == 1 && string.IsNullOrEmpty(categories[0].Name)))
                {
                    m_vrMenu.OpenObjectInterface(m_commonResource.GetGuideClip(GetCommandType(resourceType)), ResourceToMenuItems(m_commonResource.GetResource(resourceType, "")), "PICK ITEM");
                }
                else
                {
                    MenuItemInfo[] categoryItems = GetCategoryMenu(categories, resourceType);
                    m_menu.OpenGridMenu(m_commonResource.GetGuideClip(GetCommandType(resourceType)), categoryItems, "WHAT KIND OF ITEM WOULD YOU LIKE TO LOAD?");
                }
            }

            if (args.CommandType == MenuCommandType.MUSIC)
            {
                var categories = GetCategories(ResourceType.MUSIC);
                if (categories.Length == 0 || (categories.Length == 1 && string.IsNullOrEmpty(categories[0].Name)))
                    m_menu.OpenGridMenu(m_commonResource.GetGuideClip(MenuCommandType.MUSIC), ResourceToMenuItems(GetResource(ResourceType.MUSIC, "")), "SELECT MUSIC");
                else
                    m_menu.OpenGridMenu(m_commonResource.GetGuideClip(MenuCommandType.MUSIC), GetCategoryMenu(categories, ResourceType.MUSIC), "WHAT MUSIC IS THE RIGHT FIT?");
            }
            else if (args.CommandType == MenuCommandType.SELECTION)
            {
                ExecuteSelectionCommand(args);
            }
        }

        protected MenuItemInfo[] GetCategoryMenu(ResourceComponent[] categories, ResourceType resourceType)
        {
            MenuItemInfo[] categoryItems = new MenuItemInfo[categories.Length];
            for (int i = 0; i < categories.Length; i++)
            {
                categoryItems[i] = new MenuItemInfo
                {
                    Path = categories[i].Name,
                    Text = categories[i].Name,
                    Command = categories[i].Name,
                    Icon = categories[i].Preview,
                    Data = categories[i],
                    CommandType = MenuCommandType.CATEGORY_SELECTION
                };
            }
            return categoryItems;
        }

        public virtual void Pause() { }
       
        public virtual void Play() { }
        
        public virtual void Resume() { }
        
        public virtual void Stop() { }

        public virtual void ToggleMenu()
        {
            if (m_vrMenu.IsOpen)
                m_menu.Close();
            else
                m_menu.Open();
        }

        public void GoBack()
        {
            m_menu.Close();
            m_menu.Open();
        }

        public virtual void OpenMenu()
        {
            m_menu.Open();
        }

        public void OpenGridMenu(MenuCommandType commandType)
        {

        }

        public virtual void UpdateResourceSelection(MenuClickArgs args)
        {
            if (args.Data != null && args.Data is ExperienceResource)
            {
                ExperienceResource resource = (ExperienceResource)args.Data;
                if (resource.ResourceType == ResourceType.MUSIC)
                {   //m_menuSelection.Music = Array.Find(m_musicList, item => item.Clip == ((AudioResource)resource).Clip).Clip;
                    m_menuSelection.Music = ((AudioResource)resource).Clip;

                    AudioArgs audioStopArgs = new AudioArgs(null, AudioType.MUSIC)
                    {
                        FadeTime = 1.0f,
                        Done = () =>
                        {
                            AudioClip audioClip = m_menuSelection.Music;
                            AudioArgs audioArgs = new AudioArgs(audioClip, AudioType.MUSIC)
                            {
                                FadeTime = 1
                            };
                            m_audio.Play(audioArgs);
                        }
                    };
                   
                    m_audio.Stop(audioStopArgs);

                    //    var music = m_commonResource.GetResource(ResourceType.MUSIC, resource.Category);
                    //    m_menuSelection.Music = ((AudioResource)Array.Find(music, item => ((AudioResource)item).Clip == ((AudioResource)resource).Clip)).Clip;
                }


                if (resource.ResourceType == ResourceType.LOCATION)
                    m_menuSelection.Location = resource.Name;

            }
        }

        public virtual void OnLoad()
        {
            AudioSelectionFlag = false;
            if (m_settings == null)
                m_settings = IOC.Resolve<ISettings>();
            if (m_menuSelection == null)
                m_menuSelection = IOC.Resolve<IMenuSelection>();
            if (m_audio == null)
                m_audio = IOC.Resolve<IAudio>();
            if (m_experienceMachine == null)
                m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            if (m_player == null)
                m_player = IOC.Resolve<IPlayer>();
            if (m_commonResource == null)
                m_commonResource = IOC.Resolve<ICommonResource>();
            if (ExperienceMachine.AppMode == AppMode.TRAINING)
                m_player.StartTooltipRoutine(m_usedButtons);

            var musicCategories = GetCategories(ResourceType.MUSIC);
            if (musicCategories == null || musicCategories.Length == 0)
                return;

            ExperienceResource[] resources = null;

            if (m_menuSelection.MusicGroup != null && Array.FindIndex(musicCategories, item => item != null && item.Name.Equals(m_menuSelection.MusicGroup)) != -1)
                resources = GetResource(ResourceType.MUSIC, m_menuSelection.MusicGroup);
            else
                resources = GetResource(ResourceType.MUSIC, musicCategories[UnityEngine.Random.Range(0, musicCategories.Length)].Name);

            if (resources == null || resources.Length == 0)
                return;

            AudioClip clip = ((AudioResource)resources[UnityEngine.Random.Range(0, resources.Length)]).Clip;
            m_menuSelection.Music = clip;
            AudioArgs args = new AudioArgs(clip, AudioType.MUSIC);
            m_audio.Play(args);
        }

        public virtual ExperienceResource[] GetResource(ResourceType resourceType, string category)
        {
            if (resourceType == ResourceType.AUDIO)
                return m_commonResource.GetResource(resourceType, category);
            else
                return null;
        }

        public virtual ResourceComponent[] GetCategories(ResourceType resourceType)
        {
            if (resourceType == ResourceType.AUDIO)
                return m_commonResource.GetCategories(resourceType);
            else
                return null;
        }

        public Transform GetNextTeleportAnchor()
        {
            teleportAnchorIndex++;
            if (teleportAnchorIndex == m_teleportAnchors.Length)
                teleportAnchorIndex = 0;

            if (teleportAnchorIndex > m_teleportAnchors.Length - 1)
                return null;
            else
                return m_teleportAnchors[teleportAnchorIndex];
        }

        public void ToggleInventory()
        {
            var resourceType = ResourceType.OBJECT;
            var categories = m_commonResource.GetCategories(ResourceType.OBJECT);

            if (categories.Length == 0 || (categories.Length == 1 && string.IsNullOrEmpty(categories[0].Name)))
            {
                m_vrMenu.OpenObjectInterface(m_commonResource.GetGuideClip(GetCommandType(resourceType)), ResourceToMenuItems(m_commonResource.GetResource(resourceType, "")), "PICK ITEM");
            }
            else
            {
                MenuItemInfo[] categoryItems = GetCategoryMenu(categories, resourceType);
                m_vrMenu.OpenInventory(m_commonResource.GetGuideClip(GetCommandType(resourceType)), categoryItems, "WHAT KIND OF ITEM WOULD YOU LIKE TO LOAD?", false);
            }
        }

        public virtual void ResumeGuide() { }

        public List<ResourceType> MenuSequence { get; protected set; } = new List<ResourceType>();
        public List<ResourceType> LearnMenuSequence { get; protected set; } = new List<ResourceType>();

        public Experience experience { get { return this; } }
    }
}
