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

        public override BaseInputModule InputModule
        {
            get
            {
                if (m_inputModule == null)
                {
                    m_inputModule = FindObjectOfType<FordiInputModule>();
                    if (m_inputModule == null)
                    {
                        Destroy(gameObject);
                        throw new Exception("VR input module not found.");
                    }
                }
                return m_inputModule;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            //Debug.LogError("Awake");
        }

        protected override void StartOverride()
        {
            OVRManager.HMDMounted += OnHMDMount;
            OVRManager.HMDUnmounted += OnHMDUnmount;

            m_inputModule = FindObjectOfType<FordiInputModule>();
            if (m_inputModule == null)
            {
                Destroy(gameObject);
                throw new Exception("VR input module not found.");
            }

            m_playerScreenOffset = (((IVRPlayer)m_experienceMachine.Player).PlayerCanvas.position - m_screensRoot.position) / ((IVRPlayer)m_experienceMachine.Player).PlayerCanvas.localScale.z;

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
            m_screensRoot.gameObject.SetActive(true);
            PrepareForNewScreen();
            ((IVRPlayer)m_experienceMachine.Player).PrepareForSpawn();
            var menu = Instantiate(screenPrefab.Gameobject, ((IVRPlayer)m_experienceMachine.Player).PlayerCanvas).GetComponent<IScreen>();
            BringInFront(menu.Gameobject.transform, enlarge, !(screenPrefab is ObjectInterface));
            if (!external)
            {
                m_screenStack.Push(menu);
                if (m_settings.SelectedPreferences.DesktopMode)
                    menu.Hide();
            }
            return menu;
        }

        private void BringInFront(Transform menuTransform, bool enlarge = false, bool solidInGameplay = true)
        {
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
            }

            if (m_experienceMachine.GetExperience(m_experienceMachine.CurrentExperience) is Gameplay)
            {
                Vector3 localRotation = menuTransform.localRotation.eulerAngles;
                menuTransform.localRotation = Quaternion.Euler(new Vector3(30, localRotation.y, localRotation.z));
            }

            ((IVRPlayer)m_experienceMachine.Player).RequestHaltMovement(true);
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
                ((IVRPlayer)m_experienceMachine.Player).RequestHaltMovement(false);
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
            ((IVRPlayer)m_experienceMachine.Player).PrepareForSpawn();
            var menu = Instantiate(m_calendarPrefab, ((IVRPlayer)m_experienceMachine.Player).PlayerCanvas);
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
            var message = string.Empty;
            switch (args.FormType)
            {
                case FormType.LICENSE:
                    message = "Please activate license on desktop to continue.";
                    break;
                case FormType.LOGIN:
                    message = "Please login on desktop to continue.";
                    break;
                default:
                    break;
            }

            var screen = DisplayMessage(new MessageArgs()
            {
                Persist = true,
                Block = true,
                Text = message,
                BackEnabled = false
            });

            m_settings.SelectedPreferences.ForcedDesktopMode = true;
            m_uiEngine.SwitchToDesktopOnlyMode();
            return screen;
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
