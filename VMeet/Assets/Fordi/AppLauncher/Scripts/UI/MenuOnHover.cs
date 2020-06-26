using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fordi.UI
{
    public class MenuOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private GameObject m_menu;

        private bool m_pointerHovering = false;

        private bool m_fulscreenMode = false;

        private void OnEnable()
        {
            if (!m_fulscreenMode || m_pointerHovering)
                m_menu.SetActive(true);
        }

        private void OnDisable()
        {
            if (m_fulscreenMode)
                m_menu.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_pointerHovering = true;
            m_menu.gameObject.SetActive(true);

        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_pointerHovering = false;
            if (m_fulscreenMode)
                m_menu.gameObject.SetActive(false);
        }

        public void ToggleFulScreen(bool val)
        {
            m_fulscreenMode = val;
            if (!val)
                m_menu.gameObject.SetActive(true);
            else
                m_menu.gameObject.SetActive(m_pointerHovering);
        }
    }
}
