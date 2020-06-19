using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Fordi.Core;
using Fordi.Common;
using AudioType = Fordi.Core.AudioType;
using Cornea.Web;
using UniRx;

namespace Fordi.UI.MenuControl
{
    public class SettingsPanel : MenuScreen
    {
        [Header("Visual Quality")]
        [SerializeField]
        private Toggle m_highQuality;
        [SerializeField]
        private Toggle m_lowQuality;
        [SerializeField]
        private Toggle m_mediumQuality;

        [Header("Mandala")]
        [SerializeField]
        private Toggle m_mandalaAnimation;
        [SerializeField]
        private Toggle m_mandalaParticles;

        [Header("Volume")]
        [SerializeField]
        private Slider m_musicVolume;
        [SerializeField]
        private Slider m_sfxVolume;
        [SerializeField]
        private Slider m_ambienceVolume;
        [SerializeField]
        private Slider m_audioVolume;

        [Header("Devices")]
        [SerializeField]
        private TMP_Dropdown m_microphoneDropdown;

        [Header("Others")]
        [SerializeField]
        private GameObject m_saveButton;
        [SerializeField]
        private Toggle m_showTooltip;
        [SerializeField]
        private Toggle m_whiteTeleportToggle;
        [SerializeField]
        private Toggle m_blackTeleportToggle;
        [SerializeField]
        private Toggle m_desktopMode;
        
        [SerializeField]
        private Slider m_micSlider;

        [SerializeField]
        private TMP_InputField m_organization, m_name;

        private IAudio m_audio;
        private ICommonResource m_commonResource;
        private IWebInterface m_webInterace;

        [SerializeField]
        private List<Toggle> m_toggles;
        [SerializeField]
        private List<Slider> m_sliders;
        [SerializeField]
        private List<TMP_Dropdown> m_dropDowns;
        [SerializeField]
        private Toggle m_accountTab;

        private const string SampleSound = "Sample";

        private AudioType m_playingSampleType = AudioType.NONE;

        private AudioSource m_micAudioSource;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_settings = IOC.Resolve<ISettings>();
            m_audio = IOC.Resolve<IAudio>();
            m_commonResource = IOC.Resolve<ICommonResource>();
            m_webInterace = IOC.Resolve<IWebInterface>();
            m_uiEngine.InputModuleChangeEvent += OnInputModuleChange;
            m_microphoneDropdown.onValueChanged.AddListener((val) => RefreshMikeDisplay());
        }

        public override void OpenMenu(IUserInterface userInterface, MenuArgs args)
        {
            m_userInterface = userInterface;
            m_microphoneDropdown.AddOptions(new List<string>(Microphone.devices));
            ResetToPreviousSettings();
            foreach (var item in m_toggles)
                item.onValueChanged.AddListener((val) => ValueChange());
            foreach (var item in m_sliders)
                item.onValueChanged.AddListener((val) => ValueChange());
            foreach (var item in m_dropDowns)
                item.onValueChanged.AddListener((val) => ValueChange());


            m_ambienceVolume.onValueChanged.AddListener((val) => PreviewSound(val, AudioType.AMBIENCE));
            m_musicVolume.onValueChanged.AddListener((val) => PreviewSound(val, AudioType.MUSIC));
            m_sfxVolume.onValueChanged.AddListener((val) => PreviewSound(val, AudioType.SFX));
            m_audioVolume.onValueChanged.AddListener((val) => PreviewSound(val, AudioType.VO));

            m_desktopMode.interactable = !m_settings.SelectedPreferences.ForcedDesktopMode;
            m_accountTab.isOn = true;
        }

        protected override void OnDestroyOverride()
        {
            foreach (var item in m_toggles)
                item.onValueChanged.RemoveAllListeners();
            foreach (var item in m_sliders)
                item.onValueChanged.RemoveAllListeners();
            foreach (var item in m_dropDowns)
                item.onValueChanged.RemoveAllListeners();
            m_uiEngine.InputModuleChangeEvent -= OnInputModuleChange;
            if (m_micAudioSource != null)
                Destroy(m_micAudioSource.gameObject);
            m_micAudioSource = null;
            if (m_userInterface.Platform == Platform.DESKTOP && Microphone.IsRecording(m_deviceName))
                Microphone.End(m_deviceName);
        }

        private void ToggleEdit(bool val)
        {
            bool changes = false;

            changes = changes || m_settings.SelectedPreferences.AmbienceVolume != m_ambienceVolume.value;
            changes = changes || m_settings.SelectedPreferences.MusicVolume != m_musicVolume.value;
            changes = changes || m_settings.SelectedPreferences.SFXVolume != m_sfxVolume.value;
            changes = changes || m_settings.SelectedPreferences.AudioVolume != m_audioVolume.value;

            changes = changes || m_settings.SelectedPreferences.Animation != m_mandalaAnimation.isOn;
            changes = changes || m_settings.SelectedPreferences.Particles != m_mandalaParticles.isOn;

            changes = changes || m_settings.SelectedPreferences.SelectedMicrophone != m_microphoneDropdown.options[m_microphoneDropdown.value].text;

            if (m_highQuality.isOn)
                changes = changes || m_settings.SelectedPreferences.GraphicsQuality != GraphicsQuality.HIGH;
            if (m_mediumQuality.isOn)
                changes = changes || m_settings.SelectedPreferences.GraphicsQuality != GraphicsQuality.MEDIUM;
            if (m_lowQuality.isOn)
                changes = changes || m_settings.SelectedPreferences.GraphicsQuality != GraphicsQuality.LOW;

            if (m_whiteTeleportToggle.isOn && m_settings.SelectedPreferences.FadeColor != Color.white)
                changes = true;
            if (m_blackTeleportToggle.isOn && m_settings.SelectedPreferences.FadeColor != Color.black)
                changes = true;

            changes = changes || m_settings.SelectedPreferences.ShowVR != m_showTooltip.isOn;

            changes = changes || m_settings.SelectedPreferences.DesktopMode != m_desktopMode.isOn;

            m_saveButton.SetActive(changes);
        }

        private void ResetToPreviousSettings()
        {
            m_mandalaAnimation.isOn = m_settings.SelectedPreferences.Animation;
            m_mandalaParticles.isOn = m_settings.SelectedPreferences.Particles;

            m_microphoneDropdown.value = m_microphoneDropdown.options.FindIndex(item => item.text == m_settings.SelectedPreferences.SelectedMicrophone);

            m_highQuality.isOn = m_settings.SelectedPreferences.GraphicsQuality == GraphicsQuality.HIGH;
            m_mediumQuality.isOn = m_settings.SelectedPreferences.GraphicsQuality == GraphicsQuality.MEDIUM;
            m_lowQuality.isOn = m_settings.SelectedPreferences.GraphicsQuality == GraphicsQuality.LOW;

            m_musicVolume.value = m_settings.SelectedPreferences.MusicVolume;
            m_sfxVolume.value = m_settings.SelectedPreferences.SFXVolume;
            m_ambienceVolume.value = m_settings.SelectedPreferences.AmbienceVolume;
            m_audioVolume.value = m_settings.SelectedPreferences.AudioVolume;

            m_whiteTeleportToggle.isOn = m_settings.SelectedPreferences.FadeColor == Color.white;
            m_blackTeleportToggle.isOn = m_settings.SelectedPreferences.FadeColor == Color.black;

            m_showTooltip.isOn = m_settings.SelectedPreferences.ShowVR;

            m_desktopMode.isOn = m_settings.SelectedPreferences.DesktopMode;
            m_desktopMode.interactable = !m_settings.SelectedPreferences.ForcedDesktopMode;

            m_organization.text = "Organization: " + m_webInterace.UserInfo.organizationId.ToString();
            m_name.text = "Name: " + m_webInterace.UserInfo.name;
        }

        public void CancelEdit()
        {
            if (m_saveButton.gameObject.activeSelf)
            {
                ResetToPreviousSettings();
                ToggleEdit(false);
            }
        }

        public void Save()
        {
            m_settings.SelectedPreferences.AmbienceVolume = m_ambienceVolume.value;
            m_settings.SelectedPreferences.MusicVolume = m_musicVolume.value;
            m_settings.SelectedPreferences.SFXVolume = m_sfxVolume.value;
            m_settings.SelectedPreferences.AudioVolume = m_audioVolume.value;

            m_settings.SelectedPreferences.Animation = m_mandalaAnimation.isOn;
            m_settings.SelectedPreferences.Particles = m_mandalaParticles.isOn;

            m_settings.SelectedPreferences.SelectedMicrophone = m_microphoneDropdown.options[m_microphoneDropdown.value].text;

            if (m_highQuality.isOn)
                m_settings.SelectedPreferences.GraphicsQuality = GraphicsQuality.HIGH;
            if (m_mediumQuality.isOn)
                m_settings.SelectedPreferences.GraphicsQuality = GraphicsQuality.MEDIUM;
            if (m_lowQuality.isOn)
                m_settings.SelectedPreferences.GraphicsQuality = GraphicsQuality.LOW;

            m_settings.SelectedPreferences.FadeColor = m_whiteTeleportToggle.isOn ? Color.white : Color.black;

            m_settings.SelectedPreferences.ShowVR = m_showTooltip.isOn;

            m_settings.SelectedPreferences.DesktopMode = m_desktopMode.isOn;

            ToggleEdit(false);
            m_settings.SaveSettings();
            m_saveButton.SetActive(false);
        }

        public void ResetToDefaultSettings()
        {
            ExternalChangesDone?.Invoke(this, EventArgs.Empty);
            Save();
        }

        protected override void OnExternalChanges(object sender, EventArgs e)
        {
            m_mandalaAnimation.isOn = m_settings.DefaultPreferences.Animation;
            m_mandalaParticles.isOn = m_settings.DefaultPreferences.Particles;
            m_microphoneDropdown.value = m_microphoneDropdown.options.FindIndex(item => item.text == m_settings.DefaultPreferences.SelectedMicrophone);


            m_highQuality.isOn = m_settings.DefaultPreferences.GraphicsQuality == GraphicsQuality.HIGH;
            m_mediumQuality.isOn = m_settings.DefaultPreferences.GraphicsQuality == GraphicsQuality.MEDIUM;
            m_lowQuality.isOn = m_settings.DefaultPreferences.GraphicsQuality == GraphicsQuality.LOW;

            m_musicVolume.SetValue(m_settings.DefaultPreferences.MusicVolume);
            m_sfxVolume.SetValue(m_settings.DefaultPreferences.SFXVolume);
            m_ambienceVolume.SetValue(m_settings.DefaultPreferences.AmbienceVolume);
            m_audioVolume.SetValue(m_settings.DefaultPreferences.AudioVolume);

            m_whiteTeleportToggle.isOn = m_settings.DefaultPreferences.FadeColor == Color.white;
            m_blackTeleportToggle.isOn = m_settings.DefaultPreferences.FadeColor == Color.black;

            m_showTooltip.isOn = m_settings.DefaultPreferences.ShowVR;

            m_desktopMode.isOn = m_settings.DefaultPreferences.DesktopMode;
            m_desktopMode.interactable = !m_settings.SelectedPreferences.ForcedDesktopMode;

            m_organization.text = "Organization: " + m_webInterace.UserInfo.organizationId.ToString();
            m_name.text = "Name: " + m_webInterace.UserInfo.name;
        }

        public override void BackClick()
        {
            base.BackClick();
            m_audio.ConfigureAudioVolumes();
        }

        public void InitialiseEdit()
        {
            ToggleEdit(true);
        }

        private void ValueChange()
        {
            ToggleEdit(true);
        }

        private ResourceType GetResourceType(AudioType audioType)
        {
            switch (audioType)
            {
                case AudioType.MUSIC:
                    return ResourceType.MUSIC;
                case AudioType.VO:
                    return ResourceType.AUDIO;
                case AudioType.SFX:
                    return ResourceType.SFX;
                case AudioType.AMBIENCE:
                    return ResourceType.AMBIENCE_MUSIC;
            }

            return ResourceType.MUSIC;
        }

        private void PreviewSound(float volume, AudioType audioType)
        {
            if (Pair == null && m_uiEngine.ActiveModule != InputModule.STANDALONE)
                return;

            if (Pair != null && m_uiEngine.ActiveModule != InputModule.OCULUS)
                return;

            var audioSource = m_audio.GetAudioSource(audioType);
            if (audioSource.isPlaying)
            {
                audioSource.volume = volume;
                return;
            }

            var sampleSounds = m_commonResource.GetResource(GetResourceType(audioType), SampleSound);
            if (sampleSounds == null || sampleSounds.Length == 0)
                return;

            AudioClip sampleClip = ((AudioResource)sampleSounds[0]).Clip;
            AudioArgs args = new AudioArgs(sampleClip, audioType)
            {
                FadeTime = 0
            };
            m_audio.Play(args);
            if (audioType == AudioType.SFX)
                audioSource.loop = true;

            m_playingSampleType = audioType;
        }

        public void OnVolumeSliderUp()
        {
            m_audio.GetAudioSource(AudioType.SFX).loop = false;

            AudioArgs args = new AudioArgs
            {
                FadeTime = 1,
                AudioType = m_playingSampleType
            };

            m_audio.Stop(args);
        }

        private void OnInputModuleChange(object sender, EventArgs e)
        {
            OnVolumeSliderUp();
        }

        public override void Reopen()
        {
            base.Reopen();
            m_desktopMode.interactable = !m_settings.SelectedPreferences.ForcedDesktopMode;
        }

        public void Logout()
        {
            m_experienceMachine.ExecuteMenuCommand(new MenuClickArgs("Logout", "Logout", "Logout", MenuCommandType.LOGOUT, null));
        }

        #region MICROPHONE
        private int m_lastPos, m_pos;
        private string m_deviceName = "";

        private static float s_microphoneDb = 0;
        private bool m_recorgingMic = false;
        private AudioClip m_micClip;

        private void RefreshMikeDisplay()
        {
            if (m_userInterface.Platform == Platform.DESKTOP && Microphone.IsRecording(m_deviceName))
                Microphone.End(m_deviceName);

            m_deviceName = m_microphoneDropdown.options[m_microphoneDropdown.value].text;
            s_microphoneDb = 0;
            m_lastPos = 0;
            m_pos = 0;
            m_micSlider.value = 0;
            m_micClip = null;

            if (m_userInterface.Platform != Platform.DESKTOP)
                return;


            if (m_micAudioSource == null)
            {
                var obj = new GameObject("MicTestAudioSource");
                m_micAudioSource = obj.AddComponent<AudioSource>();
                m_micAudioSource.playOnAwake = false;
            }

           
            m_micAudioSource.clip = Microphone.Start(m_deviceName, true, 10, 44100);
            m_micClip = m_micAudioSource.clip;
            //Debug.LogError("Setting up m_micClip: " + name);
        }

        protected override void Update()
        {
            base.Update();

            if (m_userInterface == null)
                return;

            if (m_userInterface.Platform != Platform.DESKTOP)
            {
                m_micSlider.value = s_microphoneDb;
                return;
            }

            if (m_micAudioSource == null)
                return;

            if ((m_pos = Microphone.GetPosition(m_deviceName)) > 0)
            {
                if (m_lastPos > m_pos) m_lastPos = 0;

                if (m_pos - m_lastPos > 0)
                {
                    // Allocate the space for the new sample.
                    int len = (m_pos - m_lastPos) * m_micClip.channels;
                    float[] samples = new float[len];
                    m_micClip.GetData(samples, m_lastPos);
                    DisplayAudioLevel(samples);
                    m_lastPos = m_pos;
                }
            }
        }

        private void DisplayAudioLevel(float[] samples)
        {
            s_microphoneDb = Average(samples);
            m_micSlider.value = s_microphoneDb;
        }

        private float Average(float[] samples)
        {
            float sum = 0;
            foreach (var item in samples)
                sum += Mathf.Abs(item);
            return sum / (.3f * samples.Length);
        }
        #endregion
    }
}