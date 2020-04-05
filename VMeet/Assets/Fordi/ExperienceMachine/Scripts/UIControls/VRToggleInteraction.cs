using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VRExperience.UI
{
    public class VRToggleInteraction : VRUIInteraction
    {
        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            var toggle = (Toggle)selectable;
            toggle.onValueChanged.AddListener(OnValueChange);
        }

        public override void Init()
        {
            base.Init();
            var toggle = (Toggle)selectable;
            if (toggle.isOn)
            {
                ToggleBackgroundHighlight(true);
                ToggleOutlineHighlight(true);
            }
            if (m_image != null)
                m_image.gameObject.SetActive(toggle.isOn);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            var toggle = (Toggle)selectable;
            toggle.onValueChanged.RemoveAllListeners();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            var toggle = (Toggle)selectable;
            if (toggle.interactable && !toggle.isOn)
                base.OnPointerExit(eventData);
        }

        public override void ToggleBackgroundHighlight(bool val)
        {
            base.ToggleBackgroundHighlight(val);
            selection.color = val ? m_appTheme.SelectedTheme.toggleSelectionColor : m_appTheme.SelectedTheme.toggleNormalColor;
        }

        public override void ToggleOutlineHighlight(bool val)
        {
            base.ToggleOutlineHighlight(val);
        }

        protected virtual void OnValueChange(bool val)
        {
            ToggleBackgroundHighlight(val);
            ToggleOutlineHighlight(val);

            if (m_image != null)
                m_image.gameObject.SetActive(val);
            EventSystem.current.SetSelectedGameObject(null);
        }

        protected override void OnDisableOverride()
        {
            Toggle toggle = (Toggle)selectable;

            if (toggle != null && !toggle.isOn)
            {
                ToggleOutlineHighlight(false);
                ToggleBackgroundHighlight(false);
            }
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }
}