using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using VRExperience.Core;
using VRExperience.Common;
using AudioType = VRExperience.Core.AudioType;
using Cornea.Web;

namespace VRExperience.UI.MenuControl
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
        private TMP_InputField m_organization, m_name;

        private IAudio m_audio;
        private ICommonResource m_commonResource;
        private IWebInterface m_webInterace;

        [SerializeField]
        private List<Toggle> m_toggles;
        [SerializeField]
        private List<Slider> m_sliders;
        [SerializeField]
        private Toggle m_accountTab;

        private const string SampleSound = "Sample";

        private AudioType m_playingSampleType = AudioType.NONE;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_settings = IOC.Resolve<ISettings>();
            m_audio = IOC.Resolve<IAudio>();
            m_commonResource = IOC.Resolve<ICommonResource>();
            m_webInterace = IOC.Resolve<IWebInterface>();
            Init();
        }

        private void Init()
        {
            ResetToPreviousSettings();
            foreach (var item in m_toggles)
                item.onValueChanged.AddListener((val) => ValueChange());
            foreach (var item in m_sliders)
                item.onValueChanged.AddListener((val) => ValueChange());
            if (Pair != null)
            {
                m_ambienceVolume.onValueChanged.AddListener((val) => PreviewSound(val, AudioType.AMBIENCE));
                m_musicVolume.onValueChanged.AddListener((val) => PreviewSound(val, AudioType.MUSIC));
                m_sfxVolume.onValueChanged.AddListener((val) => PreviewSound(val, AudioType.SFX));
                m_audioVolume.onValueChanged.AddListener((val) => PreviewSound(val, AudioType.VO));
            }

            m_desktopMode.interactable = !m_settings.SelectedPreferences.ForcedDesktopMode;
            m_accountTab.isOn = true;
        }

        protected override void OnDestroyOverride()
        {
            foreach (var item in m_toggles)
                item.onValueChanged.RemoveAllListeners();
            foreach (var item in m_sliders)
                item.onValueChanged.RemoveAllListeners();
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

            changes = changes || m_settings.SelectedPreferences.ShowTooltip != m_showTooltip.isOn;

            changes = changes || m_settings.SelectedPreferences.DesktopMode != m_desktopMode.isOn;

            m_saveButton.SetActive(changes);
        }

        private void ResetToPreviousSettings()
        {
            m_mandalaAnimation.isOn = m_settings.SelectedPreferences.Animation;
            m_mandalaParticles.isOn = m_settings.SelectedPreferences.Particles;

            m_highQuality.isOn = m_settings.SelectedPreferences.GraphicsQuality == GraphicsQuality.HIGH;
            m_mediumQuality.isOn = m_settings.SelectedPreferences.GraphicsQuality == GraphicsQuality.MEDIUM;
            m_lowQuality.isOn = m_settings.SelectedPreferences.GraphicsQuality == GraphicsQuality.LOW;

            m_musicVolume.value = m_settings.SelectedPreferences.MusicVolume;
            m_sfxVolume.value = m_settings.SelectedPreferences.SFXVolume;
            m_ambienceVolume.value = m_settings.SelectedPreferences.AmbienceVolume;
            m_audioVolume.value = m_settings.SelectedPreferences.AudioVolume;

            m_whiteTeleportToggle.isOn = m_settings.SelectedPreferences.FadeColor == Color.white;
            m_blackTeleportToggle.isOn = m_settings.SelectedPreferences.FadeColor == Color.black;

            m_showTooltip.isOn = m_settings.SelectedPreferences.ShowTooltip;

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

            if (m_highQuality.isOn)
                m_settings.SelectedPreferences.GraphicsQuality = GraphicsQuality.HIGH;
            if (m_mediumQuality.isOn)
                m_settings.SelectedPreferences.GraphicsQuality = GraphicsQuality.MEDIUM;
            if (m_lowQuality.isOn)
                m_settings.SelectedPreferences.GraphicsQuality = GraphicsQuality.LOW;

            m_settings.SelectedPreferences.FadeColor = m_whiteTeleportToggle.isOn ? Color.white : Color.black;

            m_settings.SelectedPreferences.ShowTooltip = m_showTooltip.isOn;

            m_settings.SelectedPreferences.DesktopMode = m_desktopMode.isOn;

            ToggleEdit(false);
            m_settings.SaveSettings();
            m_saveButton.SetActive(false);
        }

        public void ResetToDefaultSettings()
        {
            m_mandalaAnimation.isOn = m_settings.DefaultPreferences.Animation;
            m_mandalaParticles.isOn = m_settings.DefaultPreferences.Particles;

            m_highQuality.isOn = m_settings.DefaultPreferences.GraphicsQuality == GraphicsQuality.HIGH;
            m_mediumQuality.isOn = m_settings.DefaultPreferences.GraphicsQuality == GraphicsQuality.MEDIUM;
            m_lowQuality.isOn = m_settings.DefaultPreferences.GraphicsQuality == GraphicsQuality.LOW;

            m_musicVolume.SetValue(m_settings.DefaultPreferences.MusicVolume);
            m_sfxVolume.SetValue(m_settings.DefaultPreferences.SFXVolume);
            m_ambienceVolume.SetValue(m_settings.DefaultPreferences.AmbienceVolume);
            m_audioVolume.SetValue(m_settings.DefaultPreferences.AudioVolume);

            m_whiteTeleportToggle.isOn = m_settings.DefaultPreferences.FadeColor == Color.white;
            m_blackTeleportToggle.isOn = m_settings.DefaultPreferences.FadeColor == Color.black;

            m_showTooltip.isOn = m_settings.DefaultPreferences.ShowTooltip;

            m_desktopMode.isOn = m_settings.DefaultPreferences.DesktopMode;
            m_desktopMode.interactable = !m_settings.SelectedPreferences.ForcedDesktopMode;

            m_organization.text = "Organization: " + m_webInterace.UserInfo.organizationId.ToString();
            m_name.text = "Name: " + m_webInterace.UserInfo.name;

            Save();
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

        public override void Reopen()
        {
            base.Reopen();
            m_desktopMode.interactable = !m_settings.SelectedPreferences.ForcedDesktopMode;
        }
    }
}