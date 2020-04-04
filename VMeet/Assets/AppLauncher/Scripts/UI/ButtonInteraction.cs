using UnityEngine;
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
                image.color = Coordinator.instance.appTheme.SelectedTheme.colorMix2;
            else
                image.color = Coordinator.instance.appTheme.SelectedTheme.panelInteractionBackground;
        }

        public override void OnReset()
        {
            if (pointerHovering)
                shadow.effectColor = Coordinator.instance.appTheme.SelectedTheme.colorMix2;
            else
                shadow.effectColor = Coordinator.instance.appTheme.SelectedTheme.panelInteractionOutline;
            image.color = Coordinator.instance.appTheme.SelectedTheme.panelInteractionBackground;
        }
    }
}