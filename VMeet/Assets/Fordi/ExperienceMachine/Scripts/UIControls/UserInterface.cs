using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Fordi.Common;
using Fordi.Core;
using AudioType = Fordi.Core.AudioType;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fordi.Meeting;
using Fordi.Meetings.UI;
using Fordi.ScreenSharing;
using Fordi.UI.MenuControl;

namespace Fordi.UI
{
    public interface IUserInterface
    {
        bool IsOpen { get; }
        BaseInputModule InputModule { get; }

        IScreen OpenMenu(MenuItemInfo[] menuItemInfos, bool block = true, bool persist = true);
        IScreen OpenGridMenu(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool backEnabled = true, bool block = false, bool persist = true);
        IScreen OpenGridMenu(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool backEnabled = true, bool block = false, bool persist = true, string refreshCategory = null);
        IScreen OpenInventory(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true);
        IScreen OpenColorInterface(ColorInterfaceArgs args);
        IScreen OpenSettingsInterface(AudioClip clip);
        IScreen OpenAnnotationInterface(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool backEnabled = true, bool block = false, bool persist = true);
        IScreen OpenCalendar(Action<string> onClick, ITimeForm timeForm);
        IScreen OpenMeeting(MeetingInfo meetingInfo, bool block = true, bool persist = false);
        IScreen OpenMeetingForm(MenuItemInfo[] menuItemInfos, AudioClip clip);
        IScreen OpenObjectInterface(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool block = false, bool persist = true, bool backEnabled = true);
        IScreen Popup(PopupInfo popupInfo);
        IScreen OpenForm(FormArgs args, bool block = true, bool persist = true);
        IScreen DisplayResult(Error error, bool freshScreen = false);
        IScreen DisplayProgress(string text, bool freshScreen = false);
        IScreen Block(string message);
        void CloseLastScreen();
        void Close(IScreen screen);
        void Close();
        void GoBack();
        void ShowTooltip(string text);
        void ShowPreview(Sprite sprite);
        void DeactivateUI();
        void ShowUI();
        void Hide();
        void Unhide();
        void Unblock();
    }

    public abstract class UserInterface : MonoBehaviour, IUserInterface
    {
        #region INSPECTOR_REFRENCES
        [SerializeField]
        protected MenuScreen m_mainMenuPrefab, m_gridMenuPrefab, m_inventoryMenuPrefab, m_textBoxPrefab, m_formPrefab, m_annotationInterface;
        [SerializeField]
        protected ColorInterface m_colorInterfacePrefab;
        [SerializeField]
        private RemoteMonitorScreen m_remoteMonitorView;
        [SerializeField]
        protected MeetingPage m_meetingPagePrefab;
        [SerializeField]
        protected MeetingForm m_meetingFormPrefab;
        [SerializeField]
        protected ObjectInterface m_objectInterfacePrefab;
        [SerializeField]
        protected SettingsPanel m_settingsInterfacePrefab;
        [SerializeField]
        protected MessageScreen m_genericLoader;
        [SerializeField]
        protected CalendarController m_calendarPrefab;
        [SerializeField]
        protected Transform m_screensRoot;
        [SerializeField]
        protected Popup m_popupPrefab;
        [SerializeField]
        protected Popup m_popup;
        [SerializeField]
        protected BaseInputModule m_inputModule = null;
        #endregion

        private const string YOUTUBE_PAGE = "https://www.youtube.com/telecomatics";
        private const string WEBSITE = "http://telecomatics.com/";
        private const string FACEBOOK_PAGE = "http://telecomatics.com/";
        private const string INSTAGRAM_PAGE = "http://telecomatics.com/";

        public bool IsOpen { get { return m_screenStack.Count != 0; } }

        public EventHandler AudioInterruptionEvent { get; set; }
        public EventHandler ScreenChangeInitiated { get; set; }
        public EventHandler InputModuleChangeEvent { get; set; }

        public BaseInputModule InputModule { get { return m_inputModule; } }

        protected Stack<IScreen> m_screenStack = new Stack<IScreen>();

        protected IPlayer m_player;
        protected IAudio m_audio;
        protected IMenuSelection m_menuSelection;
        protected IExperienceMachine m_experienceMachine;
        protected ICommonResource m_commonResource;
        protected ISettings m_settings;
        protected IUIEngine m_uiEngine = null;

        private MenuScreen m_blocker;

        protected virtual void Awake()
        {
            m_player = IOC.Resolve<IPlayer>();
            m_audio = IOC.Resolve<IAudio>();
            m_menuSelection = IOC.Resolve<IMenuSelection>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_commonResource = IOC.Resolve<ICommonResource>();
            m_settings = IOC.Resolve<ISettings>();
            m_uiEngine = IOC.Resolve<IUIEngine>();
        }

        protected virtual IEnumerator Start()
        {
            yield return null;
            StartOverride();
        }

        protected virtual void StartOverride()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        #region CORE
        protected virtual void PrepareForNewScreen()
        {
            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                if (screen.Persist)
                    screen.Deactivate();
                else
                    m_screenStack.Pop().Close();
            }

            if (m_player == null)
                m_player = IOC.Resolve<IPlayer>();
        }

        protected virtual IScreen SpawnScreen(IScreen screenPrefab, bool external = false)
        {
            PrepareForNewScreen();
            var menu = Instantiate(screenPrefab.Gameobject, m_screensRoot).GetComponent<IScreen>();
            if (!external)
                m_screenStack.Push(menu);
            return menu;
        }

        public virtual IScreen OpenMenu(MenuItemInfo[] items, bool block = true, bool persist = false)
        {
            var menu = (MenuScreen)SpawnScreen(m_mainMenuPrefab);
            menu.OpenMenu(items, block, persist);
            m_menuOn = true;
            return menu;
        }

        public virtual IScreen OpenGridMenu(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            var menu = (MenuScreen)SpawnScreen(m_gridMenuPrefab);
            menu.OpenGridMenu(items, title, block, persist, backEnabled);
            return menu;
        }

        public virtual IScreen OpenGridMenu(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true, string refreshCategory = null)
        {
            var menu = (MenuScreen)SpawnScreen(m_gridMenuPrefab);
            menu.OpenGridMenu(items, title, block, persist, backEnabled, refreshCategory);
            return menu;
        }

        public virtual IScreen OpenAnnotationInterface(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            var menu = (MenuScreen)SpawnScreen(m_mainMenuPrefab);
            menu.OpenGridMenu(items, title, block, persist, backEnabled);
            return menu;
        }

        public virtual IScreen OpenInventory(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            var menu = (MenuScreen)SpawnScreen(m_mainMenuPrefab);
            menu.OpenGridMenu(items, title, block, persist, backEnabled);
            m_inventoryOpen = true;
            return menu;
        }

        public virtual IScreen OpenMeeting(MeetingInfo meetingInfo, bool block = true, bool persist = false)
        {
            m_screensRoot.gameObject.SetActive(true);
            MeetingPage menu = (MeetingPage)SpawnScreen(m_meetingPagePrefab);
            menu.OpenMeeting(meetingInfo);
            m_menuOn = true;
            return menu;
        }

        public virtual IScreen OpenObjectInterface(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            var menu = (MenuScreen)SpawnScreen(m_mainMenuPrefab);
            menu.OpenGridMenu(items, title, block, persist, backEnabled);
            return menu;
        }

        public virtual IScreen Popup(PopupInfo popupInfo)
        {
            var popup = (Popup)SpawnScreen(m_popupPrefab);
            popup.Show(popupInfo, null);
            return popup;
        }

        public virtual void CloseLastScreen()
        {
            //if (m_screenStack.Count > 0)
            //{
            //    Debug.LogError("Closing: " + m_screenStack.Peek().Gameobject.name);
            //}

            ScreenChangeInitiated?.Invoke(this, EventArgs.Empty);

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Pop();
                screen.Close();
            }

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                screen.Reopen();
                if (!(screen is IForm))
                   m_uiEngine.RefreshDesktopMode();
            }
            else
            {
                m_screensRoot.gameObject.SetActive(false);
                m_uiEngine.RefreshDesktopMode();
            }
        }

        public virtual void Close(IScreen screenToBeClosed)
        {
            //Debug.LogError("Close last screen");
            if (m_screenStack.Count == 0 || m_screenStack.Peek() != screenToBeClosed)
            {
                if (!m_screenStack.Contains(screenToBeClosed))
                    screenToBeClosed.Close();
                return;
            }

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Pop();
                screen.Close();
            }

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                screen.Reopen();
                if (!(screen is IForm))
                   m_uiEngine.RefreshDesktopMode();
                //Debug.LogError("opening: " + screen.Gameobject.name);
            }
            else
            {
                m_screensRoot.gameObject.SetActive(false);
               m_uiEngine.RefreshDesktopMode();
            }
        }

        public virtual void Close()
        {
            //Debug.LogError("Close");
            foreach (var item in m_screenStack)
                item.Close();
            if (m_menuSelection == null)
                m_menuSelection = IOC.Resolve<IMenuSelection>();

            m_screenStack.Clear();
            m_menuOff = true;

            m_uiEngine.RefreshDesktopMode();
        }

        public void GoBack()
        {
            //if (m_screenStack.Count > 0)
            //    m_screenStack.Pop().Close();
            //if (m_screenStack.Count > 0)
            //    m_screenStack.Peek().Reopen();
            CloseLastScreen();
        }

        public void ShowTooltip(string text)
        {
            if (m_screenStack.Count > 0)
            {
                m_screenStack.Peek().ShowTooltip(text);
            }
        }

        public void ShowPreview(Sprite sprite)
        {
            if (m_screenStack.Count > 0)
            {
                m_screenStack.Peek().ShowPreview(sprite);
            }
        }

        public IScreen OpenColorInterface(ColorInterfaceArgs args)
        {
            var menu = (ColorInterface)SpawnScreen(m_colorInterfacePrefab);
            menu.OpenColorInterface(args);
            return menu;
        }

        public IScreen OpenSettingsInterface(AudioClip clip)
        {
           return OpenInterface(m_settingsInterfacePrefab, m_settingsInterfacePrefab);
        }

        public virtual IScreen OpenMeetingForm(MenuItemInfo[] items, AudioClip clip)
        {
            var menu = (MeetingForm)SpawnScreen(m_meetingFormPrefab);
            menu.OpenForm(items);
            return menu;
        }

        //Not handled properly for VR screen
        public abstract IScreen OpenCalendar(Action<string> onClick, ITimeForm timeForm);

        private IScreen OpenInterface(MenuScreen screenPrefab, MenuScreen dScreenPrefab, bool block = true, bool persist = false)
        {
            m_screensRoot.gameObject.SetActive(true);
            var menu = (MenuScreen)SpawnScreen(screenPrefab);
            menu.Init(block, persist);
            return menu;
        }
        #endregion

        #region GUIDE_CONDITIONS
        protected bool m_menuOn = false, m_menuOff = false, m_inventoryOpen = false;

        public bool MenuOn()
        {
            var val = m_menuOn;
            if (m_menuOn)
                m_menuOn = false;
            return val;
        }

        public bool MenuOff()
        {
            var val = m_menuOff;
            if (m_menuOff)
                m_menuOff = false;
            return val;
        }

        public bool InventoryOpen()
        {
            var val = m_inventoryOpen;
            if (m_inventoryOpen)
                m_inventoryOpen = false;
            return val;
        }
        #endregion

        public void DeactivateUI()
        {
            if (m_screenStack.Count == 0)
                return;
            m_screenStack.Peek().Deactivate();
        }

        public void ShowUI()
        {
            if (m_screenStack.Count == 0)
                return;
            m_screenStack.Peek().Reopen();
        }

        #region SOCIAL_MEDIA
        public void OpenYoutube()
        {
            OpenLinkPopup(YOUTUBE_PAGE);
        }


        public void OpenWebsite()
        {
            OpenLinkPopup(WEBSITE);
        }

        public void OpenFacebookPage()
        {
            OpenLinkPopup(FACEBOOK_PAGE);
        }

        public void OpenInstagramPage()
        {
            OpenLinkPopup(INSTAGRAM_PAGE);
        }

        private void OpenLinkPopup(string link)
        {
            var popup = Instantiate(m_popup, m_screensRoot);
            popup.transform.SetParent(m_screensRoot.parent);
            var popupInfo = new PopupInfo
            {
                Content = "Open <#FF004D>" + link + "</color> in browser?"
            };
            popup.Show(popupInfo, () => System.Diagnostics.Process.Start(link));
        }
        #endregion

        public virtual IScreen OpenForm(FormArgs args, bool block = true, bool persist = true)
        {
            m_screensRoot.gameObject.SetActive(true);

            var menu = SpawnScreen(m_formPrefab);
            if (!(menu is Form))
                throw new InvalidOperationException();

            ((Form)menu).OpenForm(args, block, persist);
            m_menuOn = true;
            return menu;
        }

        public IScreen DisplayResult(Error error, bool freshScreen = false)
        {
            if (!freshScreen && m_screenStack.Count > 0)
            {
                m_screenStack.Peek().DisplayResult(error);
                return m_screenStack.Peek();
            }
            return null;
        }

        public virtual IScreen DisplayProgress(string text, bool freshScreen = false)
        {
            if (!freshScreen && m_screenStack.Count > 0)
            {
                //Debug.LogError(m_screenStack.Peek().Gameobject.name);
                m_screenStack.Peek().DisplayProgress(text);
                return m_screenStack.Peek();
            }
            else if (freshScreen)
            {
                m_screensRoot.gameObject.SetActive(true);

                var menu = (MessageScreen)SpawnScreen(m_genericLoader);
                menu.Init(text, true, false);
                return menu;
            }
            return null;
        }


        public virtual void Hide()
        {
            //Debug.LogError("Hide");
            foreach (var item in m_screenStack)
                item.Hide();
        }

        public virtual void Unhide()
        {
            //Debug.LogError("Unhide");
            foreach (var item in m_screenStack)
                item.UnHide();
        }

        public virtual void Unblock()
        {
            if (m_blocker != null)
                m_blocker.Close();

            if (m_screenStack.Count > 0)
                m_screenStack.Peek().Reopen();

            if (m_screenStack.Count == 0)
                m_menuOff = true;
        }

        public virtual IScreen Block(string message)
        {

            if (m_screenStack.Count > 0)
                m_screenStack.Peek().Deactivate();

            m_screensRoot.gameObject.SetActive(true);

            if (m_blocker != null)
            {
                m_blocker.Reopen();
                return m_blocker;
            }
            else
            {
                var menu = (MenuScreen)SpawnScreen(m_textBoxPrefab, true);
                menu.OpenMenu(message, true, false);
                m_blocker = menu;
                m_menuOn = true;
                return menu;
            }
        }
    }
}
