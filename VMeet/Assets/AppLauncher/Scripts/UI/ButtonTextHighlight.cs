//Attach this script to the GameObject you would like to have mouse hovering detected on
//This script outputs a message to the Console when the mouse pointer is currently detected hovering over the GameObject and also when the pointer leaves.

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using Fordi.Core;
using Fordi.Common;
using Fordi;

namespace AL.UI
{
    public class ButtonTextHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        TextMeshProUGUI text;
        protected IAppTheme m_appTheme;

        protected Platform m_platform = Platform.DESKTOP;

        private void Awake()
        {
            m_appTheme = IOC.Resolve<IAppTheme>();
        }

        void Start()
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
            text.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
        }
        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            text.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
        }

        public void OnPointerExit(PointerEventData pointerEventData)
        {
            text.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            text.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
        }
    }
}