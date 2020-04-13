using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRExperience.Core
{
    [CreateAssetMenu(fileName = "AssetDatabase", menuName = "Asset Database")]
    public class AssetDB : ScriptableObject
    {
        #region UI
        [Header("Mandala")]
        public MandalaGroup[] MandalaGroups;
        public ColorGroup[] ColorGroups;
        public VOGroup MandalaColorCategory;
        [Header("Audio")]
        public AudioGroup[] MusicGroups;
        public AudioGroup[] NatureMusic;
        public AudioGroup[] MeetingMusic;
        public AudioGroup[] LobbyMusic;
        public AudioGroup[] HomeMusic;
        public AudioGroup[] MandalaMusic;
        public AudioGroup[] AbstractMusic;
        public VOGroup[] AudioGroups;
        public AudioGroup[] SFXGroups;
        public AudioGroup[] AmbienceAudioGroups;
        public AudioGroup[] ColorAudioGroups;
        public GuideAudioGroup[] GuideAudioGroups;
        [Header("Locations")]
        public ExperienceGroup[] NatureLocationsGroups;
        public ExperienceGroup[] AbstractLocationsGroups;
        [Header("Objects")]
        public ObjectGroup[] ObjectGroups;
        [Header("Others")]
        public string[] Thoughts;
        public string Credits;
        [Header("Vectors")]
        public ExperienceGroup Vectors;
        #endregion
    }
}
