using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fordi.UI
{
    public class SpriteSwapToggle : MonoBehaviour, IPointerClickHandler
    {

        [SerializeField]
        private Toggle m_toggle;

        [SerializeField]
        private Image m_onImage, m_offImage;

        public void OnPointerClick(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void Awake()
        {
            m_toggle.onValueChanged.AddListener(OnToggle);
            OnToggle(m_toggle.isOn);
        }

        private void OnToggle(bool val)
        {
            m_onImage.gameObject.SetActive(val);
            m_offImage.gameObject.SetActive(!val);
        }
    }

}