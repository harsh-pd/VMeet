using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using VRExperience.Common;
using VRExperience.UI.MenuControl;

namespace VRExperience.UI.MenuControl
{
    [Serializable]
    public class MenuItemValidationArgs
    {
        public bool IsValid
        {
            get;
            set;
        }

        public bool IsVisible
        {
            get;
            set;
        }

        public string Command
        {
            get;
            private set;
        }

        public MenuItemValidationArgs(string command)
        {
            IsValid = true;
            IsVisible = true;
            Command = command;
        }
    }

    /// <summary>
    /// While adding new types, make sure not to change the order of existing types
    /// </summary>
    public enum MenuCommandType
    {
        EXPERIENCE,
        MUSIC,
        VO,
        LOCATION,
        MANDALA,
        COLOR,
        SELECTION,
        HOME,
        MAIN,
        CATEGORY_SELECTION,
        OTHER,
        SETTINGS,
        QUIT,
        TRAINING,
        SAVE_PRESET,
        INVENTORY
    }

    [Serializable]
    public class MenuClickArgs
    {
        public string Path { get; }
        public string Name { get; }
        public string Command { get; }
        public MenuCommandType CommandType { get; }
        public object Data { get; }

        public MenuClickArgs(string path, string name, string command, MenuCommandType commandType, object data)
        {
            Path = path;
            Name = name;
            Command = command;
            CommandType = commandType;
            Data = data;
        }
    }

    [Serializable]
    public class MenuItemValidationEvent : UnityEvent<MenuItemValidationArgs>
    {
    }

    [Serializable]
    public class MenuItemEvent : UnityEvent<MenuClickArgs>
    {
    }

    [Serializable]
    public class MenuItemInfo
    {
        public string Path;
        public string Text;
        public Sprite Icon;
        [HideInInspector]
        public object Data = null;

        public string Command;
        public MenuCommandType CommandType;
        public MenuItemEvent Action;
        public bool IsValid { get; set; } = true;
        public MenuItemValidationEvent Validate;
    }

    public class Menu : MonoBehaviour
    {
        [SerializeField]
        private MenuItemInfo[] m_items = null;
        public MenuItemInfo[] Items { get { return m_items; } }

        //public void SetMenuItems(MenuItemInfo[] menuItems, bool databind = true)
        //{
        //    m_items = menuItems;
        //    if(databind)
        //    {
        //        DataBind();
        //    }
        //}

        private IVRMenu m_vrMenu;

        private Transform m_root;
     
        private void Awake()
        {
            m_vrMenu = IOC.Resolve<IVRMenu>();
            m_root = transform.parent;
        }

        public void Open()
        {
            if (m_vrMenu == null)
                m_vrMenu = IOC.Resolve<IVRMenu>();
            m_vrMenu.OpenMenu(m_items);
        }

        public void Open(MenuItemInfo[] itemInfos)
        {
            m_vrMenu.OpenMenu(itemInfos);
        }

        public void OpenGridMenu(AudioClip guide, MenuItemInfo[] itemInfos, string title, bool backEnabled = true)
        {
            m_vrMenu.OpenGridMenu(guide, itemInfos, title, backEnabled);
        }

        public void Close()
        {
            m_vrMenu.Close();
        }
    }
}
