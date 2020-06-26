using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fordi.UI
{
    public class ProcessButton : Button
    {
        [SerializeField]
        private Image m_onImage, m_loadingImage, m_offImage;

        [SerializeField]
        private bool m_isOn = true;

        [SerializeField]
        private bool m_autoToggle = false;

        [SerializeField]
        private ToggleRequestEvent m_onValueChangeRequest = new ToggleRequestEvent();

        public ToggleRequestEvent onValueChangeRequest { get { return m_onValueChangeRequest; } }

        public bool IsOn
        {
            get
            {
                return m_isOn;
            }
            set
            {
                m_isOn = value;
                OnToggle();
            }
        }

        protected override void Awake()
        {
            OnToggle();
        }

        private void OnToggle()
        {
            m_loadingImage.gameObject.SetActive(false);
            m_onImage.gameObject.SetActive(m_isOn);
            m_offImage.gameObject.SetActive(!m_isOn);
        }

        private void SwitchToLoading()
        {
            m_onImage.gameObject.SetActive(false);
            m_offImage.gameObject.SetActive(false);
            m_loadingImage.gameObject.SetActive(true);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                SwitchToLoading();
                if (IsActive() && IsInteractable())
                {
                    m_onValueChangeRequest.Invoke(!m_isOn, (success) =>
                    {
                        if (success)
                            IsOn = !IsOn;
                        OnToggle();
                    });
                }
            }

            EventSystem.current.SetSelectedGameObject(null);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (IsActive() && IsInteractable())
            {

                SwitchToLoading();
                m_onValueChangeRequest.Invoke(!m_isOn, (success) =>
                {
                    if (success)
                        IsOn = !IsOn;
                    OnToggle();
                });
            }

            base.OnSubmit(eventData);
        }

        public class ToggleRequestEvent : UnityEvent<bool, Action<bool>>
        {
           
        }
    }

}