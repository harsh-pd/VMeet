using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Modified version of OVRInputModule to enable second optional button for click.
    /// </summary>
    public class FordiInputModule : OVRInputModule
    {
        [SerializeField]
        private OVRInput.Button m_additionalClickButton;

        /// <summary>
        /// Get state of button corresponding to gaze pointer
        /// </summary>
        /// <returns></returns>
        override protected PointerEventData.FramePressState GetGazeButtonState()
        {
            var pressed = Input.GetKeyDown(gazeClickKey) || OVRInput.GetDown(joyPadClickButton) || OVRInput.GetDown(m_additionalClickButton, OVRInput.Controller.RTouch);
            var released = Input.GetKeyUp(gazeClickKey) || OVRInput.GetUp(joyPadClickButton) || OVRInput.GetUp(m_additionalClickButton, OVRInput.Controller.RTouch);

#if UNITY_ANDROID && !UNITY_EDITOR
            // On Gear VR the mouse button events correspond to touch pad events. We only use these as gaze pointer clicks
            // on Gear VR because on PC the mouse clicks are used for actual mouse pointer interactions.
            pressed |= Input.GetMouseButtonDown(0);
            released |= Input.GetMouseButtonUp(0);
#endif

            if (pressed && released)
                return PointerEventData.FramePressState.PressedAndReleased;
            if (pressed)
                return PointerEventData.FramePressState.Pressed;
            if (released)
                return PointerEventData.FramePressState.Released;
            return PointerEventData.FramePressState.NotChanged;
        }
    }
}
