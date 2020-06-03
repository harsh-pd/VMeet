using Cornea.Web;
using Fordi.Sync;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fordi.Common;
using Fordi.Core;
using Fordi.Meeting;

namespace Fordi.UI.MenuControl
{
    public interface IScreen
    {
        void Reopen();
        void Deactivate();
        void Close();
        bool Blocked { get; }
        bool Persist { get; }
        void ShowPreview(Sprite sprite);
        void ShowTooltip(string tooltip);
        void Hide();
        void UnHide();
        GameObject Gameobject { get; }
        IScreen Pair { get; }
        void AttachSyncView(SyncView syncView);
        void DisplayResult(Error error);
        void DisplayProgress(string text);
    }

    [DisallowMultipleComponent]
    public class MenuScreen : MonoBehaviour, IScreen
    {
        [SerializeField]
        protected Transform m_contentRoot;

        [SerializeField]
        protected GameObject m_menuItem;

        [SerializeField]
        protected TextMeshProUGUI m_title;

        [SerializeField]
        protected Button m_closeButton, m_okButton;

        [SerializeField]
        private GameObject m_backButton, m_refreshButton;

        [SerializeField]
        protected Image m_preview;

        [SerializeField]
        protected TextMeshProUGUI m_description;

        [SerializeField]
        private List<SyncView> m_synchronizedElements = new List<SyncView>();

        [SerializeField]
        protected GameObject m_loader = null;

        [SerializeField]
        protected GameObject m_standaloneMenu = null;

        [SerializeField]
        protected Blocker m_blocker;
 
        protected IUIEngine m_uiEngine;
        protected IExperienceMachine m_experienceMachine;
        protected ISettings m_settings;
        protected IMenuSelection m_menuSelection;
        protected IWebInterface m_webInterface = null;

        public bool Blocked { get; protected set; }

        public bool Persist { get; protected set; }

        public GameObject Gameobject { get { return gameObject; } }

        private IScreen m_pair = null;
        public IScreen Pair { get { return m_pair; } set { m_pair = value; } }

        private string m_refreshCategory = null;

        private Vector3 m_localScale = Vector3.zero;

        protected List<IMenuItem> m_menuItems = new List<IMenuItem>();

        public static EventHandler ExternalChangesDone = null;

        void Awake()
        {
            m_settings = IOC.Resolve<ISettings>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_menuSelection = IOC.Resolve<IMenuSelection>();
            m_webInterface = IOC.Resolve<IWebInterface>();
            m_uiEngine = IOC.Resolve<IUIEngine>();

            if (m_localScale == Vector3.zero)
                m_localScale = transform.localScale;

            foreach (var item in m_synchronizedElements)
            {
                FordiNetwork.RegisterPhotonView(item);
            }

            AwakeOverride();
        }

        protected virtual void OnEnable()
        {
            ExternalChangesDone += OnExternalChanges;
        }

        protected virtual void OnDisable()
        {
            ExternalChangesDone -= OnExternalChanges;
        }

        protected virtual void Update()
        {
            if (!UIEngine.s_InputSelectedFlag && m_uiEngine.ActiveModule == InputModule.STANDALONE && Input.GetKeyDown(KeyCode.Backspace) && m_backButton != null)
            {
                if (m_blocker == null || !m_blocker.gameObject.activeInHierarchy)
                    BackClick();
            }
        }

        private void OnDestroy()
        {
            OnDestroyOverride();
        }

        protected virtual void AwakeOverride()
        {

        }

        protected virtual void OnDestroyOverride()
        {

        }

        public virtual void Init(bool block, bool persist)
        {
            Blocked = block;
            Persist = persist;
        }


        protected virtual void OnExternalChanges(object sender, EventArgs e)
        {
            if (m_refreshCategory == null)
                return;

            ExperienceResource[] resources = new ExperienceResource[] { };
            resources = m_webInterface.GetResource(ResourceType.MEETING, m_refreshCategory);

            MenuItemInfo[] items = ResourceToMenuItems(resources);
            Clear();
            m_menuItems.Clear();

            //Debug.LogError("Refreshed: " + m_refreshCategory + " " + items.Length);

            foreach (var item in items)
                SpawnMenuItem(item, m_menuItem, m_contentRoot);
        }

        public virtual void Deactivate()
        {
            if (m_loader != null)
                m_loader.SetActive(false);
            if (m_blocker != null)
                m_blocker.gameObject.SetActive(false);
            if (m_description != null)
                m_description.text = "";
            gameObject.SetActive(false);
            if (Pair != null)
                Pair.Deactivate();
        }

        public virtual void Reopen()
        {
            gameObject.SetActive(true);
            if (m_preview != null)
                m_preview.gameObject.SetActive(m_preview.sprite != null);

            if (m_refreshCategory != null)
                Refresh();

            if (Pair != null)
                Pair.Reopen();
        }

        private void Refresh()
        {
            if (m_menuItems.Count == 0 || m_menuItems[0].Item.Data == null || m_menuItems[0].Item.Data.GetType() != typeof(MeetingResource))
                return;

            var sampleItem = m_menuItems[0].Item;
            ExperienceResource[] resources = new ExperienceResource[] { };
            resources = m_webInterface.GetResource(ResourceType.MEETING, m_refreshCategory);

            MenuItemInfo[] items = ResourceToMenuItems(resources);
            Clear();
            m_menuItems.Clear();

            //Debug.LogError("Refreshed: " + m_refreshCategory + " " + items.Length);

            foreach (var item in items)
                SpawnMenuItem(item, m_menuItem, m_contentRoot);
        }

        protected MenuItemInfo[] ResourceToMenuItems(ExperienceResource[] resources)
        {
            MenuItemInfo[] menuItems = new MenuItemInfo[resources.Length];
            for (int i = 0; i < resources.Length; i++)
                menuItems[i] = resources[i];
            return menuItems;
        }


        public virtual IMenuItem SpawnMenuItem(MenuItemInfo menuItemInfo, GameObject prefab, Transform parent)
        {
            IMenuItem menuItem = Instantiate(prefab, parent, false).GetComponentInChildren<IMenuItem>();
            //menuItem.name = "MenuItem";
            menuItem.Item = menuItemInfo;
            m_menuItems.Add(menuItem);
            return menuItem;
        }

        public virtual void Clear()
        {
            foreach (Transform child in m_contentRoot)
            {
                Destroy(child.gameObject);
            }
            m_contentRoot.DetachChildren();
        }

        public virtual void OpenMenu(MenuItemInfo[] items, bool blocked, bool persist)
        {
            if (m_experienceMachine.CurrentExperience == ExperienceType.HOME && m_standaloneMenu != null)
                m_standaloneMenu.SetActive(false);
            Clear();
            m_menuItems.Clear();
            Blocked = blocked;
            Persist = persist;
            gameObject.SetActive(true);
            foreach (var item in items)
                SpawnMenuItem(item, m_menuItem, m_contentRoot);

            if (m_uiEngine == null)
                m_uiEngine = IOC.Resolve<IUIEngine>();

            if (m_okButton != null)
                m_okButton.onClick.AddListener(() => m_uiEngine.CloseLastScreen());
            if (m_closeButton != null)
                m_closeButton.onClick.AddListener(() => m_uiEngine.CloseLastScreen());
        }

        public virtual void OpenMenu(string text, bool blocked, bool persist)
        {
            Clear();
            Blocked = blocked;
            Persist = persist;
            gameObject.SetActive(true);
            m_description.text = text;

            if (m_uiEngine == null)
                m_uiEngine = IOC.Resolve<IUIEngine>();

            if (m_okButton != null)
                m_okButton.onClick.AddListener(() => m_uiEngine.CloseLastScreen());
            if (m_closeButton != null)
                m_closeButton.onClick.AddListener(() => m_uiEngine.CloseLastScreen());
        }

        public virtual void OpenGridMenu(MenuItemInfo[] items, string title, bool blocked, bool persist, bool backEnabled = true, string refreshCategory = null)
        {
            if (m_backButton != null)
                m_backButton.gameObject.SetActive(backEnabled);
            m_refreshCategory = refreshCategory;
            if (m_refreshCategory != null && m_refreshButton != null)
                m_refreshButton.SetActive(true);

            if (m_title != null)
                m_title.text = title;
            OpenMenu(items, blocked, persist);
        }

        public virtual void WebRefresh()
        {
            if (m_refreshCategory == null)
                return;

            m_webInterface.GetCategories(ResourceType.MEETING, (val) =>
            {
                ExternalChangesDone?.Invoke(this, EventArgs.Empty);
            }, true);
        }

        public virtual void Close()
        {
            if (Pair != null)
                Pair.Close();
            Destroy(gameObject);
        }

        public void CloseAllScreen()
        {
            m_uiEngine.Close();
        }

        public virtual void BackClick()
        {
            m_uiEngine.GoBack();
        }

        public void ShowPreview(Sprite sprite)
        {
            if (m_preview == null)
                return;
            m_preview.gameObject.SetActive(sprite != null);
            m_preview.sprite = sprite;

            if (Pair != null)
                Pair.ShowPreview(sprite);
        }

        public void ShowTooltip(string tooltip)
        {
            if (m_description)
                m_description.text = tooltip;
            if (Pair != null)
                Pair.ShowTooltip(tooltip);
        }

        public void Hide()
        {
            if (m_localScale == Vector3.zero)
                m_localScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }

        public void UnHide()
        {
            transform.localScale = m_localScale;
        }

        public void AttachSyncView(SyncView syncView)
        {
            if (!m_synchronizedElements.Contains(syncView))
                m_synchronizedElements.Add(syncView);
        }

        public virtual void DisplayResult(Error error)
        {
            if (m_preview != null && m_preview.sprite != null)
                m_preview.gameObject.SetActive(true);

            if (m_loader)
                m_loader.SetActive(false);
            if (m_blocker)
                m_blocker.gameObject.SetActive(false);

            if (error.HasError)
                m_description.text = error.ErrorText.Style(ExperienceMachine.ErrorTextColorStyle);
            else
                m_description.text = error.ErrorText.Style(ExperienceMachine.CorrectTextColorStyle);

            //Debug.LogError("Dislay result " + m_description.text);
            if (Pair != null)
                Pair.DisplayResult(error);
        }

        public virtual void DisplayProgress(string text)
        {
            if (m_preview)
                m_preview.gameObject.SetActive(false);
            if (m_loader == null)
            {
                Debug.LogError(this.name);
                return;
            }
            //Debug.LogError("Loadr activating: " + name);
            m_loader.SetActive(true);
            m_blocker.gameObject.SetActive(true);
            m_description.text = text.Style(ExperienceMachine.ProgressTextColorStyle);

            if (Pair != null)
                Pair.DisplayProgress(text);
        }

    }
}