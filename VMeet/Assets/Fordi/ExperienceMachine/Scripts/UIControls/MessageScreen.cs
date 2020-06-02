﻿using Fordi.Sync;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.Common;
using VRExperience.Core;
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
        private GameObject m_backButton = null;

        [SerializeField]
        private List<SyncView> m_synchronizedElements = new List<SyncView>();

        [SerializeField]
        private GameObject m_loader = null;

        [SerializeField]
        private GameObject m_header = null;

        public bool Blocked { get; private set; }

        public bool Persist { get; private set; }

        public GameObject Gameobject { get { return gameObject; } }

        private IScreen m_pair = null;
        public IScreen Pair { get { return m_pair; } set { m_pair = value; } }

        private IUserInterface m_vrMenu = null;

        private Vector3 m_localScale;
        private void Awake()
        {
            m_vrMenu = IOC.Resolve<IUserInterface>();

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
            if (!VRMenu.s_InputSelectedFlag && m_vrMenu.ActiveModule == InputModule.STANDALONE && Input.GetKeyDown(KeyCode.Backspace) && m_backButton != null)
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

        public void Init(string text, bool blocked = true, bool persist = false, bool backEnabled = false, Action okClick = null)
        {
            m_text.text = text;
            if (okClick != null && m_button != null)
                m_button.onClick.AddListener(() => okClick.Invoke());
            m_header.SetActive(backEnabled);
        }

        public void BackClick()
        {
            m_vrMenu.GoBack();
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

            Invoke("CloseSelf", 2.0f);
        }

        private void CloseSelf()
        {
            m_vrMenu.Close(this);
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