using Fordi.Sync;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fordi.Common;
using Fordi.Core;
using Fordi.UI.MenuControl;

namespace Fordi.UI
{
    public class MessageArgs : MenuArgs
    {
        public Action OkClick = null;
        public string Text;
    }

    public class MessageScreen : MonoBehaviour, IScreen
    {
        [SerializeField]
        private TextMeshProUGUI m_text;

        [SerializeField]
        private Button m_button;

        [SerializeField]
        private GameObject m_backButton = null;

        [SerializeField]
        private List<SyncView> m_synchronizedElements = new List<SyncView>();

        [SerializeField]
        private GameObject m_loader = null;

        [SerializeField]
        private GameObject m_header = null;

        public bool Blocked { get; private set; }

        public bool Persist { get; private set; }

        public bool BackEnabled { get; private set; }

        public GameObject Gameobject { get { return gameObject; } }

        private IScreen m_pair = null;
        public IScreen Pair { get { return m_pair; } set { m_pair = value; } }

        private IUIEngine m_uiEngine = null;
        private IUserInterface m_interface = null;

        private Vector3 m_localScale;
        private void Awake()
        {
            m_uiEngine = IOC.Resolve<IUIEngine>();

            if (m_localScale == Vector3.zero)
                m_localScale = transform.localScale;

            foreach (var item in m_synchronizedElements)
            {
                FordiNetwork.RegisterPhotonView(item);
            }
            AwakeOverride();
        }

        protected virtual void Update()
        {
            if (!UIEngine.s_InputSelectedFlag && m_uiEngine.ActiveModule == InputModule.STANDALONE && Input.GetKeyDown(KeyCode.Backspace) && m_backButton != null)
                BackClick();
        }

        private void OnDestroy()
        {
            OnDestroyOverride();
        }

        protected virtual void AwakeOverride() { }
        protected virtual void OnDestroyOverride() { }

        public void Close()
        {
            if (Pair != null)
                Pair.Close();
            Destroy(gameObject);
        }

        public void Deactivate()
        {
            if (m_loader != null)
                m_loader.SetActive(false);
            gameObject.SetActive(false);
            if (Pair != null)
                Pair.Deactivate();
        }

        public void Init(IUserInterface userInterface, MessageArgs args)
        {
            Persist = args.Persist;
            Blocked = args.Block;
            BackEnabled = args.BackEnabled;

            m_interface = userInterface;
            m_text.text = args.Text;
            if (args.OkClick != null && m_button != null)
                m_button.onClick.AddListener(() => args.OkClick.Invoke());
            m_header.SetActive(args.BackEnabled);
        }

        public void BackClick()
        {
            m_uiEngine.GoBack();
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

        public void DisplayResult(Error error)
        {
            if(m_loader)
                m_loader.SetActive(false);

            if (error.HasError)
                m_text.text = error.ErrorText.Style(ExperienceMachine.ErrorTextColorStyle);
            else
                m_text.text = error.ErrorText.Style(ExperienceMachine.CorrectTextColorStyle);

            if (Pair != null)
                Pair.DisplayResult(error);

            if (BackEnabled)
                Invoke("CloseSelf", 2.0f);
        }

        private void CloseSelf()
        {
            m_interface.Close(this);
        }

        public void DisplayProgress(string text)
        {
            if (m_loader)
                m_loader.SetActive(true);
            m_text.text = text.Style(ExperienceMachine.ProgressTextColorStyle);

            if (Pair != null)
                Pair.DisplayProgress(text);
        }
    }
}