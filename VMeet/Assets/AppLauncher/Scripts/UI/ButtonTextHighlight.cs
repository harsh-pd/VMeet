//Attach this script to the GameObject you would like to have mouse hovering detected on
//This script outputs a message to the Console when the mouse pointer is currently detected hovering over the GameObject and also when the pointer leaves.

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using VRExperience.Core;
using VRExperience.Common;

namespace AL.UI
{
    public class ButtonTextHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        TextMeshProUGUI text;
        protected IAppTheme m_appTheme;

        private void Awake()
        {
            m_appTheme = IOC.Resolve<IAppTheme>();
        }

        void Start()
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
            text.color = m_appTheme.SelectedTheme.buttonNormalTextColor;
        }
        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            text.color = m_appTheme.SelectedTheme.buttonHighlightTextColor;
        }

        public void OnPointerExit(PointerEventData pointerEventData)
        {
            text.color = m_appTheme.SelectedTheme.buttonNormalTextColor;
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            text.color = m_appTheme.SelectedTheme.buttonNormalTextColor;
        }
    }
}