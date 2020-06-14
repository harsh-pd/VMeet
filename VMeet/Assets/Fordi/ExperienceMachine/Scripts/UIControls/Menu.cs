using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Fordi.Common;
using Fordi.UI.MenuControl;

namespace Fordi.UI.MenuControl
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
        EXPERIENCE = 0,
        MUSIC,
        VO,
        LOCATION,
        COLOR = 5,
        SELECTION,
        HOME,
        MAIN,
        CATEGORY_SELECTION,
        OTHER,
        SETTINGS,
        QUIT,
        TRAINING,
        SAVE_PRESET,
        INVENTORY,
        LOBBY,
        FORM_INPUT,
        MEETING,
        USER,
        CREATE_MEETING,
        ANNOTATION,
        LOGOUT
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
    public class MenuItemEvent<T> : UnityEvent<MenuClickArgs, T>
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
        public IUserInterface Interface;

        public string Command;
        public MenuCommandType CommandType;
        public MenuItemEvent Action;
        public bool IsValid { get; set; } = true;
        public MenuItemValidationEvent Validate;
    }

    [CreateAssetMenu(fileName = "New Menu", menuName = "Scene Menu")]
    public class Menu : ScriptableObject
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

        private IUIEngine m_uiEngine;
    }
}
