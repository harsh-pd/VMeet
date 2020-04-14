using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Windows;
using UnityEngine.XR;
using VRExperience.Core;
using VRExperience.Common;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fordi.ScreenSharing
{
    public class MouseControl : MonoBehaviour
    {
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private static int m_systemWidth, m_systemHeight;
        public static int SystemWidth { get { return m_systemWidth; } }
        public static int SystemHeight { get { return m_systemHeight; } }

        [SerializeField]
        private LaserPointer m_laserPointer;

        private bool m_windowActive = true;

        private ISettings m_settings;

        private const string MonitorLayer = "Monitor";
        private int m_monitorSortingLayer;

        private void Awake()
        {
            m_settings = IOC.Resolve<ISettings>();
            m_monitorSortingLayer = SortingLayer.NameToID(MonitorLayer);

#if UNITY_EDITOR
            m_systemWidth = 1920;
            m_systemHeight = 1080;
#else
            m_systemWidth = Display.main.systemWidth;
            m_systemHeight = Display.main.systemHeight;
#endif

        }

        void Update()
        {
            if (XRDevice.isPresent && XRDevice.userPresence == UserPresenceState.Present && m_laserPointer.HitMonitor)
            {
                if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
                {
                    Debug.LogError(m_laserPointer.EndPoint + " " +  Input.mousePosition);
                }

                //mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 50, 50, 0, 0);
            }


        }

        private void OnApplicationFocus(bool active)
        {
            m_windowActive = active;
        }

    }
}
