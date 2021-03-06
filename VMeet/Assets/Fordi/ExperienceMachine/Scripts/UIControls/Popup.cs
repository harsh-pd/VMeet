using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Fordi.Common;
using Fordi.UI.MenuControl;
using Fordi.Sync;
using Fordi.Core;

namespace Fordi.UI
{
    public class PopupInfo : MenuArgs
    {
        public Sprite Preview;
        public string Content;
    }

    public class Popup : MonoBehaviour, IScreen
    {
        [SerializeField]
        private TextMeshProUGUI m_title, m_text;
        [SerializeField]
        private Image m_icon;
        [SerializeField]
        private Button m_okButton, m_closeButton;
        [SerializeField]
        private GameObject m_loader = null;

        public bool Blocked { get; private set; }
        public bool Persist { get; private set; }

        private Action m_closed = null;
        private Action m_ok = null;
        private IUserInterface m_vrMenu;

        public GameObject Gameobject { get { return gameObject; } }

        private IScreen m_pair = null;
        public IScreen Pair { get { return m_pair; } set { m_pair = value; } }

        private Vector3 m_localScale = Vector3.zero;

        [SerializeField]
        private List<SyncView> m_synchronizedElements = new List<SyncView>();

        private void Awake()
        {
            m_vrMenu = IOC.Resolve<IUserInterface>();
            if (m_localScale == Vector3.zero)
                m_localScale = transform.localScale;

            foreach (var item in m_synchronizedElements)
            {
                FordiNetwork.RegisterPhotonView(item);
            }
        }

        public void Show(PopupInfo popupInfo, Action Ok  = null)
        {
            gameObject.SetActive(true);
            m_ok = Ok;

            Blocked = popupInfo.Block;
            Persist = popupInfo.Persist;

            if (m_title != null && !string.IsNullOrEmpty(popupInfo.Title))
                m_title.text = popupInfo.Title;
            else if(m_title != null)
                m_title.text = "";

            if (!string.IsNullOrEmpty((string)popupInfo.Content))
                m_text.text = (string)popupInfo.Content;
            else
                m_text.text = "";
            if (popupInfo.Preview != null)
            {
                m_icon.sprite = popupInfo.Preview;
                m_icon.transform.parent.gameObject.SetActive(true);
            }
            else if(m_icon != null)
                m_icon.transform.parent.gameObject.SetActive(false);
            if (m_okButton != null)
                m_okButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());
            if (m_closeButton != null)
                m_closeButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());
        }

        public void Close()
        {
            m_closed?.Invoke();
            Destroy(gameObject);
        }

        public void Reopen()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            m_loader.SetActive(false);
            gameObject.SetActive(false);
        }

        public void ShowPreview(Sprite sprite)
        {
            
        }

        public void ShowTooltip(string tooltip)
        {
        
        }

        public void OkClick()
        {
            m_ok?.Invoke();
            Destroy(gameObject);
        }

        public void Hide()
        {
            transform.localScale = Vector3.zero;
        }

        public void UnHide()
        {
            transform.localScale = m_localScale;
        }

        public void AttachSyncView(SyncView syncView)
        {
            if (m_synchronizedElements.Contains(syncView))
                m_synchronizedElements.Add(syncView);
        }

        public void DisplayResult(Error error)
        {
            m_loader.SetActive(false);

            if (error.HasError)
                m_text.text = error.ErrorText.Style(ExperienceMachine.ErrorTextColorStyle);
            else
                m_text.text = error.ErrorText.Style(ExperienceMachine.CorrectTextColorStyle);

            if (Pair != null)
                Pair.DisplayResult(error);
        }

        public void DisplayProgress(string text)
        {
            m_loader.SetActive(true);
            m_text.text = text.Style(ExperienceMachine.ProgressTextColorStyle);

            if (Pair != null)
                Pair.DisplayProgress(text);
        }
    }
}
