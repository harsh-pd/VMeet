using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fordi.UI
{
    public class FordiContentSizeFitter : ContentSizeFitter
    {
        [SerializeField]
        private float m_maxWidth = 360;
        RectTransform m_rectTransform = null;

        public override void SetLayoutHorizontal()
        {
            base.SetLayoutHorizontal();
            if (!m_rectTransform)
                m_rectTransform = (RectTransform)transform;

            var sizeDelta = m_rectTransform.sizeDelta;
            if (m_maxWidth != -1 && sizeDelta.x > m_maxWidth)
                m_rectTransform.sizeDelta = new Vector2(m_maxWidth, sizeDelta.y);
        }
    }
}
