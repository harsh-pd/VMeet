using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

namespace AL.UI
{
    public class DropdownInteraction : UIInteractionBase
    {
        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            ((TMP_Dropdown)selectable).onValueChanged.RemoveAllListeners();
        }
        public override void ToggleBackgroundHighlight(bool val)
        {
            if (val && selectable.interactable)
                image.color = m_appTheme.GetSelectedTheme(m_platform).colorMix2;
            else
                image.color = m_appTheme.GetSelectedTheme(m_platform).panelInteractionBackground;
        }

        public override void OnReset()
        {
            if (mouseHovering)
                shadow.effectColor = m_appTheme.GetSelectedTheme(m_platform).colorMix2;
            else
                shadow.effectColor = m_appTheme.GetSelectedTheme(m_platform).panelInteractionOutline;
            image.color = m_appTheme.GetSelectedTheme(m_platform).panelInteractionBackground;
        }
    }
}