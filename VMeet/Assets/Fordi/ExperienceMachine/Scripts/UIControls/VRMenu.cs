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
        private GameObject m_laserPointerObject;
        [SerializeField]
        private StandaloneMenu m_standaloneMenuPrefab;
        [SerializeField]
        private SolidBackground m_solidBackgroundPrefab = null;
        [SerializeField]
        private LaserPointer.LaserBeamBehavior m_laserBeamBehavior;
        #endregion

        private Vector3 m_playerScreenOffset;

        private LaserPointer m_laserPointer;

        private bool m_recenterFlag = false;

        protected IVRPlayer m_vrPlayer = null;

        protected override void Awake()
        {
            base.Awake();

            m_vrPlayer = (IVRPlayer)IOC.Resolve<IPlayer>();
            if (m_vrPlayer == null)
                m_vrPlayer = FindObjectOfType<Core.Player>();
            if (m_vrPlayer == null)
            {
                Destroy(gameObject);
                throw new Exception("VR player not loaded into scene.");
            }

            m_playerScreenOffset = (m_vrPlayer.PlayerCanvas.position - m_screensRoot.position) / m_vrPlayer.PlayerCanvas.localScale.z;
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
            OVRManager.HMDMounted -= OnHMDMount;
            OVRManager.HMDUnmounted -= OnHMDUnmount;
        }

        #region CORE
        protected override IScreen SpawnScreen(IScreen screenPrefab, bool enlarge = false, bool external = false)
        {
            PrepareForNewScreen();
            m_vrPlayer.PrepareForSpawn();
            var menu = Instantiate(screenPrefab.Gameobject, m_vrPlayer.PlayerCanvas).GetComponent<IScreen>();
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
            m_vrPlayer.RequestHaltMovement(true);
        }

        public override IScreen OpenMenu(MenuArgs args)
        {
            if (m_experienceMachine.CurrentExperience != ExperienceType.HOME && !m_recenterFlag && XRDevice.isPresent && XRDevice.userPresence == UserPresenceState.Present)
                StartCoroutine(CoRecenter());

            var menu = base.OpenMenu(args);
            m_menuOn = true;
            return menu;
        }

        public override IScreen OpenGridMenu(GridArgs args)
        {
            var menu = base.OpenGridMenu(args);

            if (args.Items != null && args.Items.Length > 0 && args.Items[0].Data.GetType() == typeof(ObjectGroup))
                m_inventoryOpen = true;

            return menu;
        }

        public override IScreen OpenAnnotationInterface(GridArgs args)
        {
            var menu = base.OpenAnnotationInterface(args);
            return menu;
        }

        public override IScreen OpenInventory(GridArgs args)
        {
            var menu = base.OpenInventory(args);
           

            if (args.Items != null && args.Items.Length > 0 && args.Items[0].Data.GetType() == typeof(ObjectGroup))
                m_inventoryOpen = true;

            return menu;
        }


        public override void Close()
        {
            base.Close();

            if (m_experienceMachine.CurrentExperience != ExperienceType.HOME)
                m_vrPlayer.RequestHaltMovement(false);
        }

        public override IScreen OpenMeetingForm(FormArgs args)
        {
            var menu = base.OpenMeetingForm(args);

            m_settings.SelectedPreferences.DesktopMode = true;
            m_settings.SelectedPreferences.ForcedDesktopMode = true;

            m_uiEngine.SwitchToDesktopOnlyMode();
            return menu;
        }

        //Not handled properly for VR screen
        public override IScreen OpenCalendar(CalendarArgs args)
        {
            m_screensRoot.gameObject.SetActive(true);

            m_vrPlayer.PrepareForSpawn();
            var menu = Instantiate(m_calendarPrefab, m_vrPlayer.PlayerCanvas);
            BringInFront(menu.transform);

            menu.OpenCalendar(this, args);
            m_screenStack.Push(menu);
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

        public override IScreen OpenForm(FormArgs args)
        {
            var form = base.OpenForm(args);
          
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
