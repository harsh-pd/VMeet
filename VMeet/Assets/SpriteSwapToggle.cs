using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fordi.UI
{
    public class SpriteSwapToggle : MonoBehaviour
    {

        [SerializeField]
        private Toggle m_toggle;

        [SerializeField]
        private Image m_rootImage;

        [SerializeField]
        private Sprite m_onToggle, m_offToggle;

        private void Awake()
        {
            m_toggle.onValueChanged.AddListener(OnToggle);
            OnToggle(m_toggle.isOn);
        }

        private void OnToggle(bool val)
        {
            m_rootImage.sprite = val ? m_onToggle : m_offToggle;
        }
    }

}