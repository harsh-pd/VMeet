using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using VRExperience.Common;
using VRExperience.UI.MenuControl;

namespace VRExperience.UI
{
    public struct PopupInfo
    {
        public string Title;
        public Sprite Preview;
        public string Content;
        public bool Blocked;
        public bool Persist;
    }

    public class Popup : MonoBehaviour, IScreen
    {
        [SerializeField]
        private TextMeshProUGUI m_title, m_text;
        [SerializeField]
        private Image m_icon;
        [SerializeField]
        private Button m_okButton, m_closeButton;

        public bool Blocked { get; private set; }
        public bool Persist { get; private set; }

        private Action m_closed = null;
        private Action m_ok = null;
        private IVRMenu m_vrMenu;

        public GameObject Gameobject { get { return gameObject; } }


        private void Awake()
        {
            m_vrMenu = IOC.Resolve<IVRMenu>();
        }

        public void Show(PopupInfo popupInfo, Action Ok  = null)
        {
            gameObject.SetActive(true);
            m_ok = Ok;

            Blocked = popupInfo.Blocked;
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
    }
}
