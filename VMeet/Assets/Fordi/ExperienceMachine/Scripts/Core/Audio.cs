using Papae.UnitySDK.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VRExperience.Common;

namespace VRExperience.Core
{
    public enum AudioType
    {
        MUSIC,
        VO,
        SFX,
        AMBIENCE,
        NONE
    }

    [Serializable]
    public class AudioArgs
    {
        public AudioClip Clip;
        public bool Fade;
        public float FadeTime = 0;
        public Action Done;
        public AudioType AudioType = AudioType.MUSIC;
        public float ResumeTime = 0;

        public AudioArgs() { }
        
        public AudioArgs(AudioClip clip, AudioType audioType)
        {
            Clip = clip;
            AudioType = audioType;
        }
    }

    public interface IAudio
    {
        void Play(AudioArgs args);
        void Pause(AudioArgs args);
        void Resume(AudioArgs args);
        void Stop(AudioArgs args);
        void PlaySFX(string clipName);
        AudioSource GetAudioSource(AudioType audioType);
        void ConfigureAudioVolumes();
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class Audio : MonoBehaviour, IAudio
    {
        public const string PointerClick = "click";
        public const string PointerHover = "hover";
        private const string UiSfx = "UI";
       
        private AudioSource m_musicSource, m_voSource, m_sfxSource, m_ambienceAudioSource;

        private ISettings m_settings;

        private ICommonResource m_commonResource;

        private AudioGroup[] m_sfxGroups;

        void Awake()
        {
            m_settings = IOC.Resolve<ISettings>();

            m_musicSource = new GameObject("MusicSource").AddComponent<AudioSource>();

            m_voSource = new GameObject("VOSource").AddComponent<AudioSource>();

            m_sfxSource = new GameObject("SFXSource").AddComponent<AudioSource>();

            m_ambienceAudioSource = new GameObject("AmbienceAudioSource").AddComponent<AudioSource>();

            m_musicSource.transform.SetParent(this.transform);
            m_voSource.transform.SetParent(this.transform);
            m_sfxSource.transform.SetParent(this.transform);
            m_ambienceAudioSource.transform.SetParent(this.transform);

           
        }

        private IEnumerator Start()
        {
            yield return null;
            m_commonResource = IOC.Resolve<ICommonResource>();
            m_sfxGroups = m_commonResource.AssetDb.SFXGroups;
        }

        public AudioSource GetAudioSource(AudioType audioType)
        {
            AudioSource audioSource = null;

            switch (audioType)
            {
                case AudioType.MUSIC:
                    audioSource = m_musicSource;
                    break;
                case AudioType.VO:
                    audioSource = m_voSource;
                    break;
                case AudioType.SFX:
                    audioSource = m_sfxSource;
                    break;
                case AudioType.AMBIENCE:
                    audioSource = m_ambienceAudioSource;
                    break;
                default:
                    break;
            }
            return audioSource;
        }

        public void Pause(AudioArgs args)
        {
            if (args.AudioType == AudioType.NONE || args.Clip == null)
                return;

            AudioSource audioSource = GetAudioSource(args.AudioType);

            StartCoroutine(CoAudioVolume(audioSource, 0, args.FadeTime, () =>
            {
                audioSource.Pause();
                args.Done?.Invoke();
            }));
        }

        private float GetVolumeSetting(AudioType audioType)
        {
            if (m_settings == null)
                m_settings = IOC.Resolve<ISettings>();

            switch (audioType)
            {
                case AudioType.MUSIC:
                    return m_settings.SelectedPreferences.MusicVolume;
                case AudioType.VO:
                    return m_settings.SelectedPreferences.AudioVolume;
                case AudioType.SFX:
                    return m_settings.SelectedPreferences.SFXVolume;
                case AudioType.AMBIENCE:
                    return m_settings.SelectedPreferences.AmbienceVolume;
            }

            return 1;
        }

        public void Play(AudioArgs args)
        {
            if (args.AudioType == AudioType.NONE || args.Clip == null)
                return;
            AudioSource audioSource = GetAudioSource(args.AudioType);
            float finalVOlume = GetVolumeSetting(args.AudioType);

            audioSource.clip = args.Clip;
            audioSource.time = 0;
            audioSource.Play();
            if (args.FadeTime == 0)
                audioSource.volume = finalVOlume;
            else
                StartCoroutine(CoAudioVolume(audioSource, finalVOlume, args.FadeTime, args.Done));
        }

        public void Resume(AudioArgs args)
        {
            if (args.AudioType == AudioType.NONE || args.Clip == null)
                return;

            AudioSource audioSource = GetAudioSource(args.AudioType);
            float finalVOlume = GetVolumeSetting(args.AudioType);

            audioSource.clip = args.Clip;
            audioSource.volume = 0;
            audioSource.time = 0;
            audioSource.Play();
            audioSource.time = args.ResumeTime;
            if (args.FadeTime == 0)
                audioSource.volume = finalVOlume;
            else
                StartCoroutine(CoAudioVolume(audioSource, finalVOlume, args.FadeTime, null));
        }

        public void Stop(AudioArgs args)
        {
            if (args.AudioType == AudioType.NONE)
                return;

            AudioSource audioSource = GetAudioSource(args.AudioType);
            StartCoroutine(CoAudioVolume(GetAudioSource(args.AudioType), 0, args.FadeTime, () =>
            {
                audioSource.Stop();
                args.Done?.Invoke();
            }));
        }

        IEnumerator CoAudioVolume(AudioSource audioSource, float final, float time, Action done)
        {
            //Debug.LogError(time);
            bool play = audioSource.volume < final;

            float initialVolume = audioSource.volume;
            float elapsedTime = 0.0f;
            while (elapsedTime < time)
            {

                elapsedTime += Time.deltaTime;
                float currentVolume = Mathf.Lerp(play ? 0 : initialVolume, final, Mathf.Clamp01(elapsedTime / time));
                audioSource.volume = currentVolume;
                //Debug.Log(audioSource.volume);
                yield return new WaitForEndOfFrame();
            }

            done?.Invoke();
        }

        IEnumerator Fade(AudioSource audioSource, float final, float fadeDuration)
        {
            bool play = audioSource.volume < final;

            float initialVolume = audioSource.volume;
            float elapsedTime = 0.0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float currentVolume = Mathf.Lerp(play ? 0 : initialVolume, play ? initialVolume : 0, Mathf.Clamp01(elapsedTime / fadeDuration));
                audioSource.volume = currentVolume;
                yield return new WaitForEndOfFrame();
            }
        }

        public void PlaySFX(string clipName)
        {
            float volume = GetVolumeSetting(AudioType.SFX);
            AudioGroup group = Array.Find(m_sfxGroups, item => item.Name == UiSfx);
            AudioClip clip = Array.Find(group.Resources, item => item.Name == clipName).Clip;
            AudioManager.Instance.PlayOneShot(clip, Vector3.zero, volume);
            //Debug.LogError("playing sfx: " + clip.name);
        }

        public void ConfigureAudioVolumes()
        {
            m_musicSource.volume = m_settings.SelectedPreferences.MusicVolume;
            m_ambienceAudioSource.volume = m_settings.SelectedPreferences.AmbienceVolume;
            m_voSource.volume = m_settings.SelectedPreferences.AudioVolume;
            m_sfxSource.volume = m_settings.SelectedPreferences.SFXVolume;
        }
    }
}
