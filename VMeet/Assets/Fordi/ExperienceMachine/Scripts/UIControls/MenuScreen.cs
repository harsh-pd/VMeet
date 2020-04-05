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
 
        protected IVRMenu m_vrMenu;
        protected IExperienceMachine m_experienceMachine;

        public bool Blocked { get; protected set; }

        public bool Persist { get; protected set; }

        public GameObject Gameobject { get { return gameObject; } }

        private Vector3 m_localScale = Vector3.zero;

        void Awake()
        {
            m_vrMenu = IOC.Resolve<IVRMenu>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            if (m_localScale == Vector3.zero)
                m_localScale = transform.localScale;

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


        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public void Reopen()
        {
            gameObject.SetActive(true);
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
            Destroy(gameObject);
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
        }

        public void ShowTooltip(string tooltip)
        {
            m_description.text = tooltip;
        }

        public void Hide()
        {
            transform.localScale = Vector3.zero;
        }

        public void UnHide()
        {
            transform.localScale = m_localScale;
        }
    }
}