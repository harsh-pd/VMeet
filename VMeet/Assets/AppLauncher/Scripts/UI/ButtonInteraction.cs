﻿using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

namespace AL.UI
{
    public class ButtonInteraction: UIInteractionBase
    {
        public override void ToggleBackgroundHighlight(bool val)
        {
            if (val && selectable.interactable)
                image.color = m_appTheme.SelectedTheme.colorMix2;
            else
                image.color = m_appTheme.SelectedTheme.panelInteractionBackground;
        }

        public override void OnReset()
        {
            if (mouseHovering)
                shadow.effectColor = m_appTheme.SelectedTheme.colorMix2;
            else
                shadow.effectColor = m_appTheme.SelectedTheme.panelInteractionOutline;
            image.color = m_appTheme.SelectedTheme.panelInteractionBackground;
        }
    }
}