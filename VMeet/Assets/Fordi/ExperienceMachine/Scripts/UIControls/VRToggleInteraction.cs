using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fordi.UI
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
                Pop(true);
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

        public override void OnPointerEnter(PointerEventData eventData)
        {
            var toggle = (Toggle)selectable;
            if (toggle.interactable && !toggle.isOn)
                base.OnPointerEnter(eventData);
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
            selection.color = val ? m_appTheme.GetSelectedTheme(m_platform).toggleSelectionColor : m_appTheme.GetSelectedTheme(m_platform).toggleNormalColor;
        }

        public override void ToggleOutlineHighlight(bool val)
        {
            base.ToggleOutlineHighlight(val);
        }

        protected virtual void OnValueChange(bool val)
        {
            ToggleBackgroundHighlight(val);
            ToggleOutlineHighlight(val);
            Pop(val);

            if (m_image != null)
                m_image.gameObject.SetActive(val);

            try
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            catch (System.Exception)
            {

                
            }
        }

        protected override void OnDisableOverride()
        {
            Toggle toggle = (Toggle)selectable;

            if (toggle != null && !toggle.isOn)
            {
                ToggleOutlineHighlight(false);
                ToggleBackgroundHighlight(false);
                Pop(false);
            }
            try
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            catch (System.Exception)
            {


            }
        }
    }
}