using Fordi.Common;
using Fordi.Core;
using Fordi.Meeting;
using Fordi.UI.MenuControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using AudioType = Fordi.Core.AudioType;

namespace Fordi.UI
{

    public interface IUIEngine
    {
        EventHandler AudioInterruptionEvent { get; set; }
        EventHandler ScreenChangeInitiated { get; set; }
        EventHandler InputModuleChangeEvent { get; set; }

        void OpenMenu(MenuArgs args);
        void OpenStandaloneMenu(MenuArgs menuArgs);
        void OpenGridMenu(GridArgs args);
        void OpenInventory(GridArgs args);
        void OpenSettingsInterface(AudioClip clip);
        void OpenAnnotationInterface(GridArgs args);
        void OpenCalendar(CalendarArgs args);
        void OpenMeeting(MeetingArgs args);
        void OpenMeetingForm(FormArgs args);
        void OpenObjectInterface(GridArgs args);
        void Popup(PopupInfo popupInfo);
        void OpenForm(FormArgs args);
        void LoadRemoteDesktopView(MenuArgs args);
        void AddVideo(MenuItemInfo videoItem);
        void OpenVideoConference(MenuArgs args);
        void DeactivateUI();
        void ShowUI();

        Canvas GetRootCanvas(Platform platform);

        void DisplayResult(Error error, bool freshScreen = false);
        void DisplayProgress(string text, bool freshScreen = false);

        void SwitchToDesktopOnlyMode();
        void RefreshDesktopMode();
        void DisableDesktopOnlyMode();
        InputModule ActiveModule { get;}
        bool IsOpen { get; }
        void ApplyShowVRSettings(bool showVR);
        void CloseLastScreen();
        void Close();
        void GoBack();

        void ActivateInterface(Platform platform);
        void RegisterInterface(IUserInterface userInterface);
        void PresentVideo(MenuItemInfo item);
        void RemoveVideo(uint uid);
    }

    [DefaultExecutionOrder(-50)]
    public class UIEngine : MonoBehaviour, IUIEngine
    {
        private bool m_isMenuOpen = false;
        public bool IsMenuOpen { get { return m_isMenuOpen; } }

        public InputModule ActiveModule { get; private set; }

        public static bool s_InputSelectedFlag = false;

        private ISettings m_settings = null;
        private IMenuSelection m_menuSelection = null;
        private IExperienceMachine m_experienceMachine = null;
        private IAudio m_audio = null;

        private IUserInterface m_standaloneInterface, m_vrInterface;

        public EventHandler AudioInterruptionEvent { get; set; }
        public EventHandler ScreenChangeInitiated { get; set; }
        public EventHandler InputModuleChangeEvent { get; set; }

        public bool IsOpen {
            get
            {
                if (m_vrInterface != null)
                    return m_vrInterface.IsOpen;
                return m_standaloneInterface.IsOpen;
            }
        }

        protected Sound m_lastVo = null;

        private HashSet<BaseInputModule> m_inputModules = new HashSet<BaseInputModule>();

        private void Awake()
        {
            m_settings = IOC.Resolve<ISettings>();
            m_menuSelection = IOC.Resolve<IMenuSelection>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_audio = IOC.Resolve<IAudio>();
            m_vrInterface = FindObjectOfType<VRMenu>();
            m_standaloneInterface = FindObjectOfType<DesktopInterface>();
            s_InputSelectedFlag = false;
        }

        public void SwitchToDesktopOnlyMode()
        {
            m_settings.SelectedPreferences.DesktopMode = true;
            m_vrInterface.Block("Desktop only mode is active.");
            m_standaloneInterface.Unblock();
        }

        public void RefreshDesktopMode()
        {
            //Debug.LogError("RefreshDesktopMode");
            m_settings.SelectedPreferences.ForcedDesktopMode = false;
            m_settings.SyncSettingsWithDisk(() =>
            {
                if (m_settings.SelectedPreferences.DesktopMode)
                    SwitchToDesktopOnlyMode();
                else
                    DisableDesktopOnlyMode();
            });
        }

        public void DisableDesktopOnlyMode()
        {
            m_settings.SelectedPreferences.ForcedDesktopMode = false;
            m_settings.SelectedPreferences.DesktopMode = false;
            if (m_vrInterface != null)
            {
                m_vrInterface.Unhide();
                m_vrInterface.Unblock();
            }
        }

        public void ApplyShowVRSettings(bool val)
        {
            //Debug.LogError("ApplyShowVRSettings: " + val + " " + ActiveModule.ToString());
            if (!m_settings.SelectedPreferences.DesktopMode && ActiveModule == InputModule.OCULUS)
                m_standaloneInterface.Block("PUT ON YOUR HEADSET", val);

            if (!m_settings.SelectedPreferences.DesktopMode && ActiveModule == InputModule.OCULUS)
                m_standaloneInterface.Block("PUT ON YOUR HEADSET", val);
        }


        public void OnMenuClose()
        {
            if (m_menuSelection.ExperienceType == ExperienceType.MANDALA && m_experienceMachine.CurrentExperience == ExperienceType.MANDALA && m_menuSelection.VoiceOver == null)
            {
                m_experienceMachine.GetExperience(ExperienceType.MANDALA).ResumeGuide();
                return;
            }

            if (!Experience.AudioSelectionFlag && m_lastVo != null)
            {
                AudioSource audioSource = m_audio.GetAudioSource(AudioType.VO);

                if (audioSource.isPlaying)
                {
                    float lastTime = m_lastVo.Time;
                    AudioArgs args = new AudioArgs(null, AudioType.VO)
                    {
                        FadeTime = .5f,
                        Done = () =>
                        {
                            AudioArgs voArgs = new AudioArgs(m_menuSelection.VoiceOver, AudioType.VO);
                            voArgs.FadeTime = 2;
                            voArgs.ResumeTime = lastTime;
                            m_audio.Resume(voArgs);
                        }
                    };
                    m_audio.Stop(args);
                    m_lastVo = null;
                }
                else
                {
                    AudioArgs voArgs = new AudioArgs(m_menuSelection.VoiceOver, AudioType.VO);
                    voArgs.FadeTime = 2;
                    voArgs.ResumeTime = m_lastVo.Time;
                    m_audio.Resume(voArgs);
                    m_lastVo = null;
                }
            }
        }

        public void Close()
        {
            m_vrInterface?.Close();
            m_standaloneInterface.Close();
            if (m_vrInterface != null)
                m_menuOff = true;

        }

        public void GoBack()
        {
            m_vrInterface?.GoBack();
            m_standaloneInterface.GoBack();
        }

        public void OpenMenu(MenuArgs args)
        {
            m_standaloneInterface.OpenMenu(args);
            if (m_vrInterface != null)
            {
                m_vrInterface.OpenMenu(args);
                m_menuOn = true;
            }
        }

        public void OpenStandaloneMenu(MenuArgs args)
        {
            m_standaloneInterface.OpenMenu(args);
        }

        public void OpenGridMenu(GridArgs args)
        {
            m_standaloneInterface.OpenGridMenu(args);
            m_vrInterface?.OpenGridMenu(args);
            if (m_vrInterface != null)
            {
                if (args.Items != null && args.Items.Length > 0 && args.Items[0].Data.GetType() == typeof(ObjectGroup))
                    m_inventoryOpen = true;
            }
        }

        public void OpenInventory(GridArgs args)
        {
            m_standaloneInterface.OpenInventory(args);
            m_vrInterface?.OpenInventory(args);

            if (m_vrInterface != null)
            {
                if (args.Items != null && args.Items.Length > 0 && args.Items[0].Data.GetType() == typeof(ObjectGroup))
                    m_inventoryOpen = true;
            }
        }

        public void OpenSettingsInterface(AudioClip clip)
        {
            m_standaloneInterface.OpenSettingsInterface(clip);
            m_vrInterface?.OpenSettingsInterface(clip);
        }

        public void OpenAnnotationInterface(GridArgs args)
        {
            m_standaloneInterface.OpenAnnotationInterface(args);
            m_vrInterface?.OpenAnnotationInterface(args);
        }

        public void OpenCalendar(CalendarArgs args)
        {
            m_standaloneInterface.OpenCalendar(args);
            m_vrInterface?.OpenCalendar(args);
        }

        public void OpenMeeting(MeetingArgs args)
        {
            m_standaloneInterface.OpenMeeting(args);
            m_vrInterface?.OpenMeeting(args);
        }

        public void OpenMeetingForm(FormArgs args)
        {
            m_standaloneInterface.OpenMeetingForm(args);
            m_vrInterface?.OpenMeetingForm(args);
        }

        public void OpenObjectInterface(GridArgs args)
        {
            m_standaloneInterface.OpenObjectInterface(args);
            m_vrInterface?.OpenObjectInterface(args);
        }

        public void Popup(PopupInfo popupInfo)
        {
            m_standaloneInterface.Popup(popupInfo);
            m_vrInterface?.Popup(popupInfo);
        }

        public void OpenForm(FormArgs args)
        {
            m_standaloneInterface.OpenForm(args);
            m_vrInterface?.OpenForm(args);
        }

        public void OpenVideoConference(MenuArgs args)
        {
            m_standaloneInterface.OpenVideoConference(args);
            m_vrInterface?.OpenVideoConference(args);
        }

        public void DeactivateUI()
        {
            m_standaloneInterface.DeactivateUI();
            m_vrInterface?.DeactivateUI();
        }


        public void ShowUI()
        {
            m_standaloneInterface.ShowUI();
            m_vrInterface?.ShowUI();
        }

        public void AddVideo(MenuItemInfo videoItem)
        {
            m_standaloneInterface.AddVideo(videoItem);
            m_vrInterface?.AddVideo(videoItem);
        }


        public void RemoveVideo(uint uid)
        {
            m_standaloneInterface.RemoveVideo(uid);
            m_vrInterface?.RemoveVideo(uid);
        }

        public void PresentVideo(MenuItemInfo item)
        {
            m_standaloneInterface.PresentVideo(item);
            m_vrInterface?.PresentVideo(item);
        }

        public void DisplayResult(Error error, bool freshScreen = false)
        {
            m_standaloneInterface.DisplayResult(error, freshScreen);
            m_vrInterface?.DisplayResult(error, freshScreen);
        }

        public void DisplayProgress(string text, bool freshScreen = false)
        {
            m_standaloneInterface.DisplayProgress(text, freshScreen);
            m_vrInterface?.DisplayProgress(text, freshScreen);
        }

        public void CloseLastScreen()
        {
            m_standaloneInterface.CloseLastScreen();
            m_vrInterface?.CloseLastScreen();
        }

        public void ActivateInterface(Platform platform)
        {
            if (platform == Platform.VR)
            {
                if (m_vrInterface == null)
                {
                    return;
                }

                m_standaloneInterface.Block("PUT ON YOUR HEADSET", m_settings.SelectedPreferences.ShowVR);
                m_vrInterface.Unblock();
                m_standaloneInterface.InputModule.enabled = false;
                m_vrInterface.InputModule.enabled = true;
                ActiveModule = InputModule.OCULUS;
            }
            else
            {
                m_standaloneInterface.Unblock();
                if (m_vrInterface != null)
                    m_vrInterface.InputModule.enabled = false;
                m_standaloneInterface.InputModule.enabled = true;
                ActiveModule = InputModule.STANDALONE;
            }
        }

        public void LoadRemoteDesktopView(MenuArgs args)
        {
            ((IStandaloneInterface)m_standaloneInterface).LoadRemoteDesktopView(args);
        }

        public void RegisterInterface(IUserInterface userInterface)
        {
            if (userInterface.Platform == Platform.VR)
                m_vrInterface = userInterface;
            if (userInterface.Platform == Platform.DESKTOP)
                m_standaloneInterface = userInterface;
        }

        public Canvas GetRootCanvas(Platform platform)
        {
            if (platform == Platform.DESKTOP)
                return m_standaloneInterface.RootCanvas;
            else if (m_vrInterface != null)
                return m_vrInterface.RootCanvas;
            else
                throw new InvalidOperationException("VR Module not available");
        }

        #region GUIDE_CONDITIONS
        private bool m_menuOn = false, m_menuOff = false, m_inventoryOpen = false;

        public bool MenuOn()
        {
            //Debug.LogError("MenuOn: " + m_menuOn);
            var val = m_menuOn;
            if (m_menuOn)
                m_menuOn = false;
            return val;
        }

        public bool MenuOff()
        {
            var val = m_menuOff;
            if (m_menuOff)
                m_menuOff = false;
            return val;
        }

        public bool InventoryOpen()
        {
            var val = m_inventoryOpen;
            if (m_inventoryOpen)
                m_inventoryOpen = false;
            return val;
        }
        #endregion
    }
}
