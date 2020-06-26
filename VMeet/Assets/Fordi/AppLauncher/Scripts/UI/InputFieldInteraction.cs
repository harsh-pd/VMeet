using AL;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Fordi.UI.MenuControl;
using Fordi.UI;

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
                selectable.Select();
                HardSelect();
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            try
            {
                if (EventSystem.current.currentSelectedGameObject == selectable.gameObject)
                    UIEngine.s_InputSelectedFlag = false;
            }
            catch
            {

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
                shadow.effectColor = m_appTheme.GetSelectedTheme(m_platform).colorMix2;
            else
                shadow.effectColor = m_appTheme.GetSelectedTheme(m_platform).panelInteractionOutline;
            image.color = m_appTheme.GetSelectedTheme(m_platform).InputFieldNormalColor;
        }

        public override void ToggleBackgroundHighlight(bool val)
        {
            //Debug.LogError(name + " ToggleBackgroundHighlight: " + val);
            if (val && selectable.interactable)
                image.color = m_appTheme.GetSelectedTheme(m_platform).InputFieldSelection;
            else if (!((TMPro.TMP_InputField)selectable).isFocused)
                image.color = m_appTheme.GetSelectedTheme(m_platform).InputFieldNormalColor;
        }

        // Disabled for now
        public virtual void OnSelect(BaseEventData eventData)
        {
            UIEngine.s_InputSelectedFlag = true;
            HardSelect();
        }

        public override void ToggleOutlineHighlight(bool val)
        {
            //Debug.LogError(name +  " ToggleOutlineHighlight: " + val);
            if (val && selectable.interactable)
                shadow.effectColor = m_appTheme.GetSelectedTheme(m_platform).colorMix2;
            else if (!((TMPro.TMP_InputField)selectable).isFocused)
                shadow.effectColor = m_appTheme.GetSelectedTheme(m_platform).panelInteractionOutline;
        }

        // Input field interaction flag disabled for now
        public virtual void OnDeselect(BaseEventData baseEventData)
        {
            //UIInteractionManager.insideInputField = false;
            //Hotkeys.HotKeyEnabled = true;
            ToggleOutlineHighlight(false);
            ToggleBackgroundHighlight(false);
            UIEngine.s_InputSelectedFlag = false;
        }
    }
}
