using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Fordi.Core;

namespace Fordi.UI.MenuControl
{
    public class VerticalMenuItem : MenuItem
    {
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            
            if (m_allowTextScroll && m_textOverflowing && !string.IsNullOrEmpty(m_text.text))
            {
                if (m_textScrollEnumerator != null)
                    StopCoroutine(m_textScrollEnumerator);
                m_textScrollEnumerator = TextScroll();
                StartCoroutine(m_textScrollEnumerator);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (m_textScrollEnumerator != null)
            {
                StopCoroutine(m_textScrollEnumerator);
                ((RectTransform)m_text.transform).localPosition = m_initialTextPosition;
                if (m_clonedText != null)
                    m_clonedText.gameObject.SetActive(false);
            }
        }

        protected override void OnDisableOverride()
        {
            base.OnDisableOverride();

            if (m_textScrollEnumerator != null)
            {
                StopCoroutine(m_textScrollEnumerator);
                ((RectTransform)m_text.transform).localPosition = m_initialTextPosition;
                if (m_clonedText != null)
                    m_clonedText.gameObject.SetActive(false);
            }
        }

    }
}
