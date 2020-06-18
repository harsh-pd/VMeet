using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Core
{
    public enum GraphicsQuality
    {
        LOW = 1,
        MEDIUM = 3,
        HIGH = 5
    }

    [CreateAssetMenu(fileName = "New Experience Settings", menuName = "Experience Settings")]
    public class Preferences : ScriptableObject
    {
        [Header("Quality")]
        public GraphicsQuality GraphicsQuality = GraphicsQuality.HIGH;

        [Header("Audio")]
        [Range(0, 1)]
        public float AudioVolume = .6f;
        [Range(0, 1)]
        public float MusicVolume = .4f;
        [Range(0, 1)]
        public float SFXVolume = .2f;
        [Range(0, 1)]
        public float AmbienceVolume = .3f;

        [Header("Performance")]
        public bool Animation = true;
        public bool Particles = true;

        [Header("Others")]
        public Color FadeColor = Color.black;
        public bool ShowVR = true;
        [HideInInspector]
        public bool ShowTooltip = false;

        [Header("Mode")]
        public bool DesktopMode;
        [HideInInspector]
        [NonSerialized]
        public bool ForcedDesktopMode = false;

        [Header("Annotation")]
        [Range(0, 1)]
        public float annotationDelay = .25f;

        [Header("Devices")]
        public string SelectedMicrophone = "";
    }
}
