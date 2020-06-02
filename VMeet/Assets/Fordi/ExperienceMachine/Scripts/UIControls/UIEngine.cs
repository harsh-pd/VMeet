using Fordi.Common;
using Fordi.Core;
using Fordi.UI.MenuControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using AudioType = Fordi.Core.AudioType;

namespace Fordi.UI
{

    public interface IUIEngine
    {
        EventHandler AudioInterruptionEvent { get; set; }
        EventHandler ScreenChangeInitiated { get; set; }
        EventHandler InputModuleChangeEvent { get; set; }
        void SwitchToDesktopOnlyMode();
        void RefreshDesktopMode();
        void DisableDesktopOnlyMode();
        InputModule ActiveModule { get;}

        void ApplyShowVRSettings(bool showVR);
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

        protected Sound m_lastVo = null;

        private void Awake()
        {
            m_settings = IOC.Resolve<ISettings>();
            m_menuSelection = IOC.Resolve<IMenuSelection>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_audio = IOC.Resolve<IAudio>();
        }

        public void SwitchToDesktopOnlyMode()
        {
            m_standaloneInterface.Hide();
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
            throw new NotImplementedException();
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
    }
}
