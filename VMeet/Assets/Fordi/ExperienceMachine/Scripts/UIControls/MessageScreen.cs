using Fordi.Sync;
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

        [SerializeField]
        private List<SyncView> m_synchronizedElements = new List<SyncView>();


        public bool Blocked { get; private set; }

        public bool Persist { get; private set; }

        public GameObject Gameobject { get { return gameObject; } }

        private IScreen m_pair = null;
        public IScreen Pair { get { return m_pair; } set { m_pair = value; } }

        private Vector3 m_localScale;
        private void Awake()
        {
            if (m_localScale == Vector3.zero)
                m_localScale = transform.localScale;

            foreach (var item in m_synchronizedElements)
            {
                FordiNetwork.RegisterPhotonView(item);
            }
        }

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
    }
}