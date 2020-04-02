using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEngine.EventSystems
{
    [Serializable]
    public struct ButtonController
    {
        [SerializeField]
        private OVRInput.Button m_button;
        [SerializeField]
        private OVRInput.Controller m_controller;

        public OVRInput.Button Button { get { return m_button; } }
        public OVRInput.Controller Controller { get { return m_controller; } }

        public ButtonController(OVRInput.Button button, OVRInput.Controller controller)
        {
            m_button = button;
            m_controller = controller;
        }

        public static bool operator ==(ButtonController first, ButtonController second)
        {
            return (first.Button == second.Button) && (first.Controller == second.Controller);
        }

        public static bool operator !=(ButtonController first, ButtonController second)
        {
            return (first.Button != second.Button) || (first.Controller != second.Controller);
        }
    }

    public class FordiInput : MonoBehaviour
    {
        public static bool Blocked { get; set; }

        public static OVRInput.Button LastPressedButton { get; private set; }
        public static OVRInput.Controller LastPressedController { get; private set; }

        private static List<ButtonController> m_blockedButtons = new List<ButtonController>();

        private List<OVRInput.Button> m_buttons = new List<OVRInput.Button>();
        private List<OVRInput.Controller> m_controllers = new List<OVRInput.Controller>();

        private void Awake()
        {
            m_blockedButtons.Clear();
            m_buttons = Enum.GetValues(typeof(OVRInput.Button)).Cast<OVRInput.Button>().ToList();
            m_buttons.Remove(OVRInput.Button.Any);
            m_controllers.Add(OVRInput.Controller.LTouch);
            m_controllers.Add(OVRInput.Controller.RTouch);
        }

        private void Update()
        {
            UpdateLastPress();
        }

        private void UpdateLastPress()
        {
            foreach (var button in m_buttons)
            {
                foreach (var controller in m_controllers)
                {
                    if (OVRInput.GetDown(button, controller))
                    {
                        LastPressedButton = button;
                        LastPressedController = controller;
                    }
                }
            }
        }

        public static bool GetDown(OVRInput.Button button, OVRInput.Controller controller)
        {
            if (Blocked || m_blockedButtons.Contains(new ButtonController(button, controller)))
                return false;
            
            return OVRInput.GetDown(button, controller);
        }

        public static bool GetUp(OVRInput.Button button, OVRInput.Controller controller)
        {
            if (Blocked || m_blockedButtons.Contains(new ButtonController(button, controller)))
                return false;

            return OVRInput.GetUp(button, controller);
        }

        public static bool Get(OVRInput.Button button, OVRInput.Controller controller)
        {
            if (Blocked || m_blockedButtons.Contains(new ButtonController(button, controller)))
                return false;

            return OVRInput.Get(button, controller);
        }

        public static void Block(OVRInput.Button button, OVRInput.Controller controller)
        {
            m_blockedButtons.Add(new ButtonController(button, controller));
        }

        public static void Unblock(OVRInput.Button button, OVRInput.Controller controller)
        {
            var buttonController = new ButtonController(button, controller);
            if (m_blockedButtons.Contains(buttonController))
                m_blockedButtons.Remove(buttonController);
        }
    }
}
