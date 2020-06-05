using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Windows;
using UnityEngine.XR;
using Fordi.Core;
using Fordi.Common;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace Fordi.ScreenSharing
{
    public interface IMouseControl
    {
        void ActivateMonitor(Monitor monitor);
        void DeactivateMonitor(Monitor monitor);
        void PointerClickOnMonitor(Monitor monitor, PointerEventData eventData);
        void PointerDownOnMonitor(Monitor monitor, PointerEventData eventData);
        void PointerUpOnMonitor(Monitor monitor, PointerEventData eventData);
    }

    public class MouseControl : MonoBehaviour, IMouseControl
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
        private IExperienceMachine m_experienceMachine = null;

        private const string MonitorLayer = "Monitor";
        private int m_monitorSortingLayer;

        private Monitor m_activeMonitor = null;

        private void Awake()
        {
            m_settings = IOC.Resolve<ISettings>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_experienceMachine.OnModuleRegistration += ModuleLoaded;
            m_monitorSortingLayer = SortingLayer.NameToID(MonitorLayer);

#if UNITY_EDITOR
            m_systemWidth = 1920;
            m_systemHeight = 1080;
#else
            m_systemWidth = Display.main.systemWidth;
            m_systemHeight = Display.main.systemHeight;
#endif

        }

        private void OnDestroy()
        {
            m_experienceMachine.OnModuleRegistration -= ModuleLoaded;
        }

        void Update()
        {
            if (m_laserPointer != null && XRDevice.isPresent && XRDevice.userPresence == UserPresenceState.Present && m_activeMonitor != null && m_laserPointer.HitMonitor)
            {
                var cursorPosition = WorldToMouseCoordinates(m_laserPointer.EndPoint);
                SetCursorPos(cursorPosition.x, cursorPosition.y);
            }
        }

        private void ModuleLoaded(object sender, EventArgs e)
        {
            if (m_laserPointer == null)
                m_laserPointer = FindObjectOfType<LaserPointer>();
        }

        private void OnApplicationFocus(bool active)
        {
            m_windowActive = active;
        }

        public void ActivateMonitor(Monitor monitor)
        {
            m_activeMonitor = monitor;
        }

        public void DeactivateMonitor(Monitor monitor)
        {
            m_activeMonitor = null;
        }

        public void PointerClickOnMonitor(Monitor monitor, PointerEventData eventData)
        {
            if (m_activeMonitor != monitor)
            {
                Debug.LogWarning("PointerClickOnMonitor: Active monitor not same as event monitor");
                return;
            }
            var mouseCoordinates = WorldToMouseCoordinates(eventData.pointerCurrentRaycast.worldPosition);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)mouseCoordinates.x, (uint)mouseCoordinates.y, 0, 0);

            //Debug.LogError(xMousePosition + " " + yMousePosition + " " + worldUnitSizeDelta + " " + xMousePosition + " " + yMousePosition);
        }

        public void PointerDownOnMonitor(Monitor monitor, PointerEventData eventData)
        {
            if (m_activeMonitor != monitor)
            {
                Debug.LogWarning("PointerDownOnMonitor: Active monitor not same as event monitor");
                return;
            }
            var mouseCoordinates = WorldToMouseCoordinates(eventData.pointerCurrentRaycast.worldPosition);
            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)mouseCoordinates.x, (uint)mouseCoordinates.y, 0, 0);

            //Debug.LogError(xMousePosition + " " + yMousePosition + " " + worldUnitSizeDelta + " " + xMousePosition + " " + yMousePosition);
        }

        public void PointerUpOnMonitor(Monitor monitor, PointerEventData eventData)
        {
            if (m_activeMonitor != monitor)
            {
                Debug.LogWarning("PointerUpOnMonitor: Active monitor not same as event monitor");
                return;
            }
            var mouseCoordinates = WorldToMouseCoordinates(eventData.pointerCurrentRaycast.worldPosition);
            mouse_event(MOUSEEVENTF_LEFTUP, (uint)mouseCoordinates.x, (uint)mouseCoordinates.y, 0, 0);

            //Debug.LogError(xMousePosition + " " + yMousePosition + " " + worldUnitSizeDelta + " " + xMousePosition + " " + yMousePosition);
        }


        private Vector2Int WorldToMouseCoordinates(Vector3 worldPosition)
        {
            if (m_activeMonitor == null)
            {
                Debug.LogWarning("No active monitor");
                return new Vector2Int(0, 0);
            }

            Vector3[] worldCorners = new Vector3[4];
            m_activeMonitor.rectTransform.GetWorldCorners(worldCorners);

            var width = 2 * Vector2.Distance(new Vector2(worldCorners[2].x, worldCorners[2].z), new Vector2(m_activeMonitor.rectTransform.position.x, m_activeMonitor.rectTransform.position.z));
            var height = 2 * (worldCorners[2].y - m_activeMonitor.rectTransform.position.y);

            Vector3 leftDownCoordinates = worldCorners[0];

            var unitX = Vector3.Normalize(new Vector3(m_activeMonitor.rectTransform.position.x, leftDownCoordinates.y, m_activeMonitor.rectTransform.position.z) - leftDownCoordinates);
            //var xVector = new Vector2(worldPosition.x, worldPosition.z) - new Vector2(leftDownCoordinates.x, leftDownCoordinates.z);
            var xValue = Vector2.Distance(new Vector2(worldPosition.x, worldPosition.z), new Vector2(leftDownCoordinates.x, leftDownCoordinates.z));

            //int xMousePosition = (int)Mathf.Abs(((worldPosition.x - leftDownCoordinates.x) * m_systemWidth / width));
            int xMousePosition = (int)Mathf.Abs((xValue * m_systemWidth / width));
            int yMousePosition = (int)Mathf.Abs(((worldPosition.y - leftDownCoordinates.y) * m_systemHeight / height));
            //Debug.LogError(worldPosition + " : " + worldCorners[0] + " x: " + xMousePosition + " y: " + yMousePosition + " xValue: " + xValue + " unitX: " + unitX);

            return new Vector2Int(xMousePosition, m_systemHeight - yMousePosition);
        }
    }
}
