using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace VRExperience.UI
{
    public class ButtonTip : MonoBehaviour
    {
        [SerializeField]
        private Image m_background;
        [SerializeField]
        private TextMeshProUGUI m_content;
        [SerializeField]
        private ButtonController m_buttonController;

        private Color m_textColor, m_backgroundColor;
        private string m_defaultText = null;

        private float fadeLength = 1.0f;

        private Tween m_textColorTween, m_backgroudColorTween;

        public string Text
        {
            set
            {
                if (m_defaultText == null)
                    m_defaultText = m_content.text;
                m_content.text = value;
            }
        }

        public ButtonController ButtonController { get { return m_buttonController; } }

        private void Awake()
        {
            m_textColor = m_content.color;
            m_backgroundColor = m_background.color;
        }

        private void OnEnable()
        {
            Toggle(true);
        }

        public void Toggle(bool val)
        {
            if (m_textColorTween != null)
                m_textColorTween.Kill();
            if (m_backgroudColorTween != null)
                m_backgroudColorTween.Kill();

            if (val)
            {
                m_background.color = Color.clear;
                m_content.color = Color.clear;
                m_backgroudColorTween = m_background.DOColor(m_backgroundColor, fadeLength);
                m_textColorTween = m_content.DOColor(m_textColor, fadeLength);
            }
            else
            {
                m_backgroudColorTween = m_background.DOColor(Color.clear, fadeLength);
                m_textColorTween = m_content.DOColor(Color.clear, fadeLength);
            }
        }

        public void OnReset()
        {
            if (m_defaultText != null)
                m_content.text = m_defaultText;
        }
    }
}
