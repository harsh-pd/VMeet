using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.UI.MenuControl;

namespace VRExperience.UI
{
    public class MessageScreen : MonoBehaviour, IScreen
    {
        [SerializeField]
        private TextMeshProUGUI m_text;

        [SerializeField]
        private Button m_button;

        public bool Blocked { get; private set; }

        public bool Persist { get; private set; }

        public GameObject Gameobject { get { return gameObject; } }

        public void Close()
        {
            m_button.onClick.Invoke();
            Destroy(gameObject);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public void Init(string text, bool blocked = true, bool persist = false, Action okClick = null)
        {
            m_text.text = text;
            if (okClick != null)
                m_button.onClick.AddListener(() => okClick.Invoke());
        }

        public void Reopen()
        {
            gameObject.SetActive(true);
        }

        public void ShowPreview(Sprite sprite)
        {
            
        }

        public void ShowTooltip(string tooltip)
        {
            
        }
    }
}