﻿using AL;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AL.UI
{
    public class InputFieldInteraction : UIInteractionBase, IDeselectHandler, ISelectHandler
    {
        [SerializeField]
        private bool m_selectOnEnable = false;

        public override void OnEnable()
        {
            base.OnEnable();
            if (m_selectOnEnable && selectable.interactable)
            {
                Debug.LogError("OnEnable: Activating inputfield again");
                selectable.Select();
                HardSelect();
            }
        }

        public override void Init()
        {
            if (!m_selectOnEnable)
            {
                ToggleBackgroundHighlight(false);
                ToggleOutlineHighlight(false);
            }
        }

        public override void OnReset()
        {
            if (mouseHovering)
                shadow.effectColor = m_appTheme.SelectedTheme.colorMix2;
            else
                shadow.effectColor = m_appTheme.SelectedTheme.panelInteractionOutline;
            image.color = m_appTheme.SelectedTheme.InputFieldNormalColor;
        }

        public override void ToggleBackgroundHighlight(bool val)
        {
            //Debug.LogError(name + " ToggleBackgroundHighlight: " + val);
            if (val && selectable.interactable)
                image.color = m_appTheme.SelectedTheme.InputFieldSelection;
            else if (!((TMPro.TMP_InputField)selectable).isFocused)
                image.color = m_appTheme.SelectedTheme.InputFieldNormalColor;
        }

        // Disabled for now
        public virtual void OnSelect(BaseEventData eventData)
        {
            HardSelect();
        }

        public override void ToggleOutlineHighlight(bool val)
        {
            //Debug.LogError(name +  " ToggleOutlineHighlight: " + val);
            if (val && selectable.interactable)
                shadow.effectColor = m_appTheme.SelectedTheme.colorMix2;
            else if (!((TMPro.TMP_InputField)selectable).isFocused)
                shadow.effectColor = m_appTheme.SelectedTheme.panelInteractionOutline;
        }

        // Input field interaction flag disabled for now
        public virtual void OnDeselect(BaseEventData baseEventData)
        {
            //UIInteractionManager.insideInputField = false;
            //Hotkeys.HotKeyEnabled = true;
            ToggleOutlineHighlight(false);
            ToggleBackgroundHighlight(false);
        }
    }
}
