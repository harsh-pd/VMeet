using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;
using VRExperience.Common;
using ProtoBuf;
using VRExperience.UI.MenuControl;

namespace VRExperience.Core
{
    [Serializable]
    [ProtoContract]
    public class DynamicSettings
    {
        [ProtoMember(1)]
        public int GraphicsQuality;
        [ProtoMember(2)]
        public float AudioVolume;
        [ProtoMember(3)]
        public float MusicVolume;
        [ProtoMember(4)]
        public float SFXVolume;
        [ProtoMember(5)]
        public float AmbienceVolume;
        [ProtoMember(6)]
        public bool MandalaAnimation;
        [ProtoMember(7)]
        public bool MandalaParticles;
        [ProtoMember(8)]
        public bool Desktop;

        private ISettings m_settings;

        public void Load()
        {
            if (m_settings == null)
                m_settings = IOC.Resolve<ISettings>();

            m_settings.SelectedPreferences.GraphicsQuality = (GraphicsQuality)GraphicsQuality;
            m_settings.SelectedPreferences.AudioVolume = AudioVolume;
            m_settings.SelectedPreferences.MusicVolume = MusicVolume;
            m_settings.SelectedPreferences.SFXVolume = SFXVolume;
            m_settings.SelectedPreferences.AmbienceVolume = AmbienceVolume;
            m_settings.SelectedPreferences.Animation = MandalaAnimation;
            m_settings.SelectedPreferences.Particles = MandalaParticles;
            m_settings.SelectedPreferences.DesktopMode = Desktop;
        }

        public void Download()
        {
            if (m_settings == null)
                m_settings = IOC.Resolve<ISettings>();

            GraphicsQuality = (int)m_settings.SelectedPreferences.GraphicsQuality;
            AudioVolume = m_settings.SelectedPreferences.AudioVolume;
            MusicVolume = m_settings.SelectedPreferences.MusicVolume;
            SFXVolume = m_settings.SelectedPreferences.SFXVolume;
            AmbienceVolume = m_settings.SelectedPreferences.AmbienceVolume;
            MandalaAnimation = m_settings.SelectedPreferences.Animation;
            MandalaParticles = m_settings.SelectedPreferences.Particles;
            Desktop = m_settings.SelectedPreferences.DesktopMode;
        }
    }

    public interface ISettings
    {
        Preferences SelectedPreferences { get; }
        Preferences DefaultPreferences { get; }
        void SaveSettings();
    }

    public class Settings : MonoBehaviour, ISettings
    {
        [SerializeField]
        private Preferences selectedPreferences, defaultPreferences;

        private IExperienceMachine m_experienceMachine;
        private IPlayer m_player;
        private IVRMenu m_vrMenu;

        private const string ConfigFile = "ConfigFile.config";

        public Preferences SelectedPreferences
        {
            get
            {
                return selectedPreferences;
            }
        }

        public Preferences DefaultPreferences
        {
            get
            {
                return defaultPreferences;
            }
        }

        private void Awake()
        {
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_player = IOC.Resolve<IPlayer>();
            m_vrMenu = IOC.Resolve<IVRMenu>();

            DynamicSettings dynamicSettings = null;
            var configFilePath = Path.Combine(Application.persistentDataPath, ConfigFile);
            if (File.Exists(configFilePath))
            {
                File.ReadAllText(configFilePath);
                try
                {
                    //using (FileStream stream = new FileStream(configFilePath, FileMode.Create, FileAccess.Write))
                    //{
                    //    Serializer.Serialize(stream, dynamicSettings);
                    //}

                    using (FileStream stream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read))
                    {
                        dynamicSettings = Serializer.Deserialize<DynamicSettings>(stream);
                    }

                    dynamicSettings.Load();
                    InitSettings();
                    return;
                }
                catch(Exception)
                {
                  
                }
            }
            dynamicSettings = new DynamicSettings();
            dynamicSettings.Download();

            using (FileStream stream = new FileStream(configFilePath, FileMode.Create, FileAccess.Write))
            {
                Serializer.Serialize(stream, dynamicSettings);
            }
        }

        private IEnumerator Start()
        {
            yield return null;
            InitSettings();
        }

        private void InitSettings()
        {
            m_experienceMachine.SetQualityLevel(selectedPreferences.GraphicsQuality);
            MandalaExperience mandalaExperience = (MandalaExperience)m_experienceMachine.GetExperience(ExperienceType.MANDALA).experience;
            mandalaExperience.AllowAnimation = selectedPreferences.Animation;
            mandalaExperience.AllowParticles = selectedPreferences.Particles;
            m_experienceMachine.SetAmbienceAudioVolume(selectedPreferences.AmbienceVolume);
            if (selectedPreferences.DesktopMode)
                m_vrMenu.SwitchToDesktop();
            m_player.ApplyTooltipSettings();
        }

        public void SaveSettings()
        {
            var dynamicSettings = new DynamicSettings();
            var configFilePath = Path.Combine(Application.persistentDataPath, ConfigFile);
            dynamicSettings.Download();
            using (FileStream stream = new FileStream(configFilePath, FileMode.Create, FileAccess.Write))
            {
                Serializer.Serialize(stream, dynamicSettings);
            }

            m_experienceMachine.SetQualityLevel(selectedPreferences.GraphicsQuality);
            MandalaExperience mandalaExperience = (MandalaExperience)m_experienceMachine.GetExperience(ExperienceType.MANDALA).experience;
            mandalaExperience.AllowAnimation = selectedPreferences.Animation;
            mandalaExperience.AllowParticles = selectedPreferences.Particles;
            m_experienceMachine.SetAmbienceAudioVolume(selectedPreferences.AmbienceVolume);
            if (selectedPreferences.DesktopMode)
                m_vrMenu.SwitchToDesktop();
            m_player.ApplyTooltipSettings();
        }
    }
}