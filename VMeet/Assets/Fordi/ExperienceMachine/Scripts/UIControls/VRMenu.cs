using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Fordi.Common;
using Fordi.Core;
using AudioType = Fordi.Core.AudioType;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fordi.Meeting;
using Fordi.Meetings.UI;
using Fordi.ScreenSharing;

namespace Fordi.UI.MenuControl
{
    public enum InputModule
    {
        STANDALONE,
        OCULUS
    }

    public enum Platform
    {
        STANDALONE,
        OCULUS
    }

    public class Sound
    {
        public float Time { get; set; }
        public AudioClip Clip { get; private set; }
        public Sound(float time, AudioClip clip)
        {
            Time = time;
            Clip = clip;
        }
    }

    public class VRMenu : UserInterface
    {
        #region INSPECTOR_REFRENCES
        [SerializeField]
        private LaserPointer.LaserBeamBehavior m_laserBeamBehavior;
        [SerializeField]
        private GameObject m_laserPointerObject;
        [SerializeField]
        private StandaloneMenu m_standaloneMenuPrefab;
        [SerializeField]
        private SolidBackground m_solidBackgroundPrefab = null;
        #endregion

        private Vector3 m_playerScreenOffset;

        private MenuScreen m_vrBlocker = null;

        private LaserPointer m_laserPointer;

        private bool m_recenterFlag = false;

        private StandaloneMenu m_standAloneMenu = null;
        private MenuScreen m_permanentDesktopScreen = null;

        protected override void Awake()
        {
            base.Awake();
            m_playerScreenOffset = (m_player.PlayerCanvas.position - m_screensRoot.position) / m_player.PlayerCanvas.localScale.z;
            OVRManager.HMDMounted += OnHMDMount;
            OVRManager.HMDUnmounted += OnHMDUnmount;
            //Debug.LogError("Awake");
        }

        protected override void StartOverride()
        {
            if (m_laserPointer == null)
                m_laserPointer = m_laserPointerObject.GetComponent<LaserPointer>();
            if (m_laserPointer != null)
                m_laserPointer.laserBeamBehavior = m_laserBeamBehavior;

            if (XRDevice.userPresence == UserPresenceState.Present)
                OnHMDMount();
            else
                OnHMDUnmount();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //OVRManager.HMDMounted -= OnHMDUnmount;
            //OVRManager.HMDUnmounted -= OnHMDMount;
        }

        #region CORE
        protected override IScreen SpawnScreen(IScreen screenPrefab, bool external = false)
        {
            PrepareForNewScreen();
            m_player.PrepareForSpawn();
            var menu = Instantiate(screenPrefab.Gameobject, m_player.PlayerCanvas).GetComponent<IScreen>();
            BringInFront(menu.Gameobject.transform);
            if (!external)
            {
                m_screenStack.Push(menu);
                if (m_settings.SelectedPreferences.DesktopMode)
                    menu.Hide();
            }
            return menu;
        }

        private void BringInFront(Transform menuTransform, bool solidInGameplay = true, bool enlarge = false)
        {
            m_screensRoot.gameObject.SetActive(true);
            Vector3 offset = menuTransform.localPosition / 100.0f;
            menuTransform.transform.localPosition = Vector3.zero;
            menuTransform.transform.localRotation = Quaternion.identity;
            menuTransform.transform.localPosition = menuTransform.transform.localPosition - m_playerScreenOffset;
            menuTransform.transform.SetParent(m_screensRoot);
            menuTransform.position = menuTransform.position + menuTransform.forward * offset.z + new Vector3(0, offset.y, 0);

            if (m_experienceMachine.GetExperience(m_experienceMachine.CurrentExperience) is Gameplay && solidInGameplay)
            {
                var solidBackground = Instantiate(m_solidBackgroundPrefab, menuTransform);
                if (enlarge)
                    solidBackground.Enlarge();
                Vector3 localRotation = menuTransform.localRotation.eulerAngles;
                menuTransform.localRotation = Quaternion.Euler(new Vector3(30, localRotation.y, localRotation.z));
            }
            m_player.RequestHaltMovement(true);
        }

        public override IScreen OpenMenu(MenuItemInfo[] items, bool block = true, bool persist = false)
        {
            if (m_experienceMachine.CurrentExperience != ExperienceType.HOME && !m_recenterFlag && XRDevice.isPresent && XRDevice.userPresence == UserPresenceState.Present)
                StartCoroutine(CoRecenter());

            var menu = base.OpenMenu(items, block, persist);
            m_menuOn = true;
            return menu;
        }

        public override IScreen OpenGridMenu(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            var menu = base.OpenGridMenu(guide, items, title, backEnabled, block, persist);

            if (items != null && items.Length > 0 && items[0].Data.GetType() == typeof(ObjectGroup))
                m_inventoryOpen = true;

            return menu;
        }

        public override IScreen OpenGridMenu(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true, string refreshCategory = null)
        {
            var menu = base.OpenGridMenu(guide, items, title, backEnabled, block, persist, refreshCategory);

            if (items != null && items.Length > 0 && items[0].Data.GetType() == typeof(ObjectGroup))
                m_inventoryOpen = true;
            return menu;
        }

        public override IScreen OpenAnnotationInterface(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            var menu = base.OpenAnnotationInterface(guide, items, title, backEnabled, block, persist);
            return menu;
        }

        public override IScreen OpenInventory(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            var menu = base.OpenInventory(guide, items, title, backEnabled, block, persist);
           

            if (items != null && items.Length > 0 && items[0].Data.GetType() == typeof(ObjectGroup))
                m_inventoryOpen = true;

            return menu;
        }


        public override void Close()
        {
            base.Close();

            if (m_experienceMachine.CurrentExperience != ExperienceType.HOME)
                m_player.RequestHaltMovement(false);
        }

        public override IScreen OpenMeetingForm(MenuItemInfo[] items, AudioClip clip)
        {
            var menu = base.OpenMeetingForm(items, clip);

            m_settings.SelectedPreferences.DesktopMode = true;
            m_settings.SelectedPreferences.ForcedDesktopMode = true;

            m_uiEngine.SwitchToDesktopOnlyMode();
            return menu;
        }

        //Not handled properly for VR screen
        public override IScreen OpenCalendar(Action<string> onClick, ITimeForm timeForm)
        {
            if (m_screenStack.Count > 0 && m_screenStack.Peek() is CalendarController)
            {
                return null;
            }

            m_screensRoot.gameObject.SetActive(true);

            var menu = (CalendarController)SpawnScreen(m_calendarPrefab);
            menu.OpenCalendar(null, timeForm);

            if (m_settings.SelectedPreferences.DesktopMode)
                menu.Hide();
            return menu;
        }

        //public void BlockDesktop()
        //{
        //    if (m_desktopBlocker != null)
        //    {
        //        m_desktopBlocker.Reopen();
        //        m_desktopBlocker.transform.localScale = m_settings.SelectedPreferences.ShowVR ? Vector3.zero : Vector3.one;
        //    }

        //}

        //public void UnblockDesktop()
        //{
        //    if (m_desktopBlocker != null)
        //        m_desktopBlocker.Deactivate();
        //}
        #endregion

        #region DESKTOP_VR_COORDINATION
        void OnHMDUnmount()
        {
            m_uiEngine.ActivateInterface(Fordi.Platform.DESKTOP);
        }

        void OnHMDMount()
        {

            m_uiEngine.ActivateInterface(Fordi.Platform.VR);

            if (!m_recenterFlag && XRDevice.isPresent && XRDevice.userPresence == UserPresenceState.Present)
                StartCoroutine(CoRecenter());
        }

        private IEnumerator CoRecenter()
        {
            yield return new WaitForSeconds(1);
            InputTracking.Recenter();
            m_recenterFlag = true;
        }

        public override IScreen OpenForm(FormArgs args, bool block = true, bool persist = true)
        {
            var form = base.OpenForm(args, block, persist);
          
            m_settings.SelectedPreferences.DesktopMode = true;
            m_settings.SelectedPreferences.ForcedDesktopMode = true;
            m_uiEngine.SwitchToDesktopOnlyMode();
            return form;
        }

        //public void DisplayMessage(string text)
        //{
            
        //}

        public void SwitchToDesktopOnlyMode()
        {
            throw new NotImplementedException();
        }

        public void DisableDesktopOnlyMode()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
