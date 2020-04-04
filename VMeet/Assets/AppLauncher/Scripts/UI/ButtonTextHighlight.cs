//Attach this script to the GameObject you would like to have mouse hovering detected on
//This script outputs a message to the Console when the mouse pointer is currently detected hovering over the GameObject and also when the pointer leaves.

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace AL.UI
{
    public class ButtonTextHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        TextMeshProUGUI text;

        void Start()
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
            text.color = Coordinator.instance.appTheme.SelectedTheme.buttonNormalTextColor;
        }
        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            text.color = Coordinator.instance.appTheme.SelectedTheme.buttonHighlightTextColor;
        }

        public void OnPointerExit(PointerEventData pointerEventData)
        {
            text.color = Coordinator.instance.appTheme.SelectedTheme.buttonNormalTextColor;
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            text.color = Coordinator.instance.appTheme.SelectedTheme.buttonNormalTextColor;
        }
    }
}