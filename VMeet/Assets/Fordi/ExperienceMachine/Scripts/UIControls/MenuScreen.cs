using Fordi.Sync;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.Common;
using VRExperience.Core;

namespace VRExperience.UI.MenuControl
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
        private GameObject m_backButton;

        [SerializeField]
        private Image m_preview;

        [SerializeField]
        private TextMeshProUGUI m_description;

        [SerializeField]
        private List<SyncView> m_synchronizedElements = new List<SyncView>();

        [SerializeField]
        protected GameObject m_loader = null;

        [SerializeField]
        private GameObject m_standaloneMenu = null;
 
        protected IVRMenu m_vrMenu;
        protected IExperienceMachine m_experienceMachine;
        protected ISettings m_settings;
        protected IMenuSelection m_menuSelection;

        public bool Blocked { get; protected set; }

        public bool Persist { get; protected set; }

        public GameObject Gameobject { get { return gameObject; } }

        private IScreen m_pair = null;
        public IScreen Pair { get { return m_pair; } set { m_pair = value; } }

        private Vector3 m_localScale = Vector3.zero;

        void Awake()
        {
            m_settings = IOC.Resolve<ISettings>();
            m_vrMenu = IOC.Resolve<IVRMenu>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_menuSelection = IOC.Resolve<IMenuSelection>();
            if (m_localScale == Vector3.zero)
                m_localScale = transform.localScale;

            foreach (var item in m_synchronizedElements)
            {
                FordiNetwork.RegisterPhotonView(item);
            }

            AwakeOverride();
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


        public virtual void Deactivate()
        {
            if (m_loader != null)
                m_loader.SetActive(false);
            if (m_description != null)
                m_description.text = "";
            gameObject.SetActive(false);
            if (Pair != null)
                Pair.Deactivate();
        }

        public virtual void Reopen()
        {
            gameObject.SetActive(true);
            if (Pair != null)
                Pair.Reopen();
        }


        public virtual void SpawnMenuItem(MenuItemInfo menuItemInfo, GameObject prefab, Transform parent)
        {
            MenuItem menuItem = Instantiate(prefab, parent, false).GetComponentInChildren<MenuItem>();
            //menuItem.name = "MenuItem";
            menuItem.Item = menuItemInfo;
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
            Blocked = blocked;
            Persist = persist;
            gameObject.SetActive(true);
            foreach (var item in items)
                SpawnMenuItem(item, m_menuItem, m_contentRoot);

            if (m_vrMenu == null)
                m_vrMenu = IOC.Resolve<IVRMenu>();

            if (m_okButton != null)
                m_okButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());
            if (m_closeButton != null)
                m_closeButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());
        }

        public virtual void OpenMenu(string text, bool blocked, bool persist)
        {
            Clear();
            Blocked = blocked;
            Persist = persist;
            gameObject.SetActive(true);
            m_description.text = text;

            if (m_vrMenu == null)
                m_vrMenu = IOC.Resolve<IVRMenu>();

            if (m_okButton != null)
                m_okButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());
            if (m_closeButton != null)
                m_closeButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());
        }

        public virtual void OpenGridMenu(MenuItemInfo[] items, string title, bool blocked, bool persist, bool backEnabled = true)
        {
            if (m_backButton != null)
                m_backButton.gameObject.SetActive(backEnabled);

            if (m_title != null)
                m_title.text = title;
            OpenMenu(items, blocked, persist);
        }

        public void Close()
        {
            if (Pair != null)
                Pair.Close();
            Destroy(gameObject);
        }

        public void CloseAllScreen()
        {
            m_vrMenu.Close();
        }

        public virtual void BackClick()
        {
            m_vrMenu.GoBack();
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
            m_loader.SetActive(false);

            if (error.HasError)
                m_description.text = error.ErrorText.Style(ExperienceMachine.ErrorTextColorStyle);
            else
                m_description.text = error.ErrorText.Style(ExperienceMachine.CorrectTextColorStyle);

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
            m_description.text = text.Style(ExperienceMachine.ProgressTextColorStyle);

            if (Pair != null)
                Pair.DisplayProgress(text);
        }

    }
}