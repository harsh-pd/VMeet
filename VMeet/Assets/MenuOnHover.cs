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

        private void OnEnable()
        {
            if (m_pointerHovering)
                m_menu.SetActive(true);
        }

        private void OnDisable()
        {
            m_menu.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_menu.SetActive(true);
            m_pointerHovering = true;

        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_menu.SetActive(false);
            m_pointerHovering = false;
        }
    }
}
