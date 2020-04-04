using AL;
using UnityEngine.EventSystems;

namespace AL.UI
{
    public class InputFieldInteraction : UIInteractionBase, IDeselectHandler, ISelectHandler
    {

        public override void OnReset()
        {
            if (pointerHovering)
                shadow.effectColor = Coordinator.instance.appTheme.SelectedTheme.colorMix2;
            else
                shadow.effectColor = Coordinator.instance.appTheme.SelectedTheme.panelInteractionOutline;
            image.color = Coordinator.instance.appTheme.SelectedTheme.InputFieldNormalColor;
        }

        public override void ToggleBackgroundHighlight(bool val)
        {
            if (val && selectable.interactable)
                image.color = Coordinator.instance.appTheme.SelectedTheme.InputFieldSelection;
            else if (!((TMPro.TMP_InputField)selectable).isFocused)
                image.color = Coordinator.instance.appTheme.SelectedTheme.InputFieldNormalColor;
        }

        // Disabled for now
        public virtual void OnSelect(BaseEventData eventData)
        {
            HardSelect();
        }

        public override void ToggleOutlineHighlight(bool val)
        {
            //UnityEngine. Debug.Log("ToggleOUtline");
            if (val && selectable.interactable)
                shadow.effectColor = Coordinator.instance.appTheme.SelectedTheme.colorMix2;
            else if (!((TMPro.TMP_InputField)selectable).isFocused)
                shadow.effectColor = Coordinator.instance.appTheme.SelectedTheme.panelInteractionOutline;
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
