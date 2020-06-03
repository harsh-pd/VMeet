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

        void OpenMenu(MenuItemInfo[] menuItemInfos, bool block = true, bool persist = true);
        void OpenGridMenu(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool backEnabled = true, bool block = false, bool persist = true);
        void OpenGridMenu(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool backEnabled = true, bool block = false, bool persist = true, string refreshCategory = null);
        void OpenInventory(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true);
        void OpenColorInterface(ColorInterfaceArgs args);
        void OpenSettingsInterface(AudioClip clip);
        void OpenAnnotationInterface(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool backEnabled = true, bool block = false, bool persist = true);
        void OpenCalendar(Action<string> onClick, ITimeForm timeForm);
        void OpenMeeting(MeetingInfo meetingInfo, bool block = true, bool persist = false);
        void OpenMeetingForm(MenuItemInfo[] menuItemInfos, AudioClip clip);
        void OpenObjectInterface(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool block = false, bool persist = true, bool backEnabled = true);
        void Popup(PopupInfo popupInfo);
        void OpenForm(FormArgs args, bool block = true, bool persist = true);
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
    }

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

        public bool IsOpen { get { return m_standaloneInterface.IsOpen; } }

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
        }

        public void SwitchToDesktopOnlyMode()
        {
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
            m_vrInterface.Unhide();
            m_vrInterface.Unblock();
        }

        public void ApplyShowVRSettings(bool val)
        {
            //throw new NotImplementedException();
            //if (val && !m_settings.SelectedPreferences.DesktopMode && ActiveModule == InputModule.OCULUS)
            //{
            //    m_dScreenRoot.localScale = Vector3.zero;
            //    UnblockDesktop();
            //}

            //if (!val && !m_settings.SelectedPreferences.DesktopMode && ActiveModule == InputModule.OCULUS)
            //{
            //    m_dScreenRoot.localScale = Vector3.one;
            //    BlockDesktop();
            //}
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
            m_vrInterface.Close();
            m_standaloneInterface.Close();
        }

        public void GoBack()
        {
            m_vrInterface.GoBack();
            m_standaloneInterface.GoBack();
        }

        public void OpenMenu(MenuItemInfo[] menuItemInfos, bool block = true, bool persist = true)
        {
            m_standaloneInterface.OpenMenu(menuItemInfos, block, persist);
            m_vrInterface?.OpenMenu(menuItemInfos, block, persist);
        }

        public void OpenGridMenu(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            m_standaloneInterface.OpenGridMenu(guide, menuItemInfos, title, backEnabled, block, persist);
            m_vrInterface?.OpenGridMenu(guide, menuItemInfos, title, backEnabled, block, persist);
        }

        public void OpenGridMenu(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool backEnabled = true, bool block = false, bool persist = true, string refreshCategory = null)
        {
            m_standaloneInterface.OpenGridMenu(guide, menuItemInfos, title, backEnabled, block, persist, refreshCategory);
            m_vrInterface?.OpenGridMenu(guide, menuItemInfos, title, backEnabled, block, persist, refreshCategory);
        }

        public void OpenInventory(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            m_standaloneInterface.OpenInventory(guide, items, title, backEnabled, block, persist);
            m_vrInterface?.OpenInventory(guide, items, title, backEnabled, block, persist);
        }

        public void OpenColorInterface(ColorInterfaceArgs args)
        {
            m_standaloneInterface.OpenColorInterface(args);
            m_vrInterface?.OpenColorInterface(args);
        }

        public void OpenSettingsInterface(AudioClip clip)
        {
            m_standaloneInterface.OpenSettingsInterface(clip);
            m_vrInterface?.OpenSettingsInterface(clip);
        }

        public void OpenAnnotationInterface(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            m_standaloneInterface.OpenAnnotationInterface(guide, menuItemInfos, title, backEnabled, block, persist);
            m_vrInterface?.OpenAnnotationInterface(guide, menuItemInfos, title, backEnabled, block, persist);
        }

        public void OpenCalendar(Action<string> onClick, ITimeForm timeForm)
        {
            m_standaloneInterface.OpenCalendar(onClick, timeForm);
            m_vrInterface?.OpenCalendar(onClick, timeForm);
        }

        public void OpenMeeting(MeetingInfo meetingInfo, bool block = true, bool persist = false)
        {
            m_standaloneInterface.OpenMeeting(meetingInfo, block, persist);
            m_vrInterface?.OpenMeeting(meetingInfo, block, persist);
        }

        public void OpenMeetingForm(MenuItemInfo[] menuItemInfos, AudioClip clip)
        {
            m_standaloneInterface.OpenMeetingForm(menuItemInfos, clip);
            m_vrInterface?.OpenMeetingForm(menuItemInfos, clip);
        }

        public void OpenObjectInterface(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool block = false, bool persist = true, bool backEnabled = true)
        {
            m_standaloneInterface.OpenObjectInterface(guide, menuItemInfos, title, block, persist, backEnabled);
            m_vrInterface?.OpenObjectInterface(guide, menuItemInfos, title, block, persist, backEnabled);
        }

        public void Popup(PopupInfo popupInfo)
        {
            m_standaloneInterface.Popup(popupInfo);
            m_vrInterface?.Popup(popupInfo);
        }

        public void OpenForm(FormArgs args, bool block = true, bool persist = true)
        {
            m_vrInterface?.OpenForm(args, block, persist);
            m_standaloneInterface.OpenForm(args, block, persist);
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
                m_standaloneInterface.Block("PUT ON YOUR HEADSET");
                m_vrInterface.Unblock();
                m_standaloneInterface.InputModule.enabled = false;
                m_vrInterface.InputModule.enabled = true;
                ActiveModule = InputModule.OCULUS;
            }
            else
            {
                m_vrInterface.Block("DESKTOP MODE ACTIVE");
                m_standaloneInterface.Unblock();
                m_vrInterface.InputModule.enabled = false;
                m_standaloneInterface.InputModule.enabled = true;
                ActiveModule = InputModule.STANDALONE;
            }
        }
    }
}
