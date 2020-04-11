using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRExperience.Core
{
    public interface IMenuSelection
    {
        ColorGroup ColorGroup { get; set; }
        string Location { get; set; }
        MandalaResource MandalaResource { get; set; }
        AudioClip Music { get; set; }
        AudioClip VoiceOver { get; set; }
        ExperienceType  ExperienceType { get; set; }
        string MusicGroup { get; set; }
    }

    /// <summary>
    /// This is preserved between scenes.
    /// </summary>
    public class MenuSelection : IMenuSelection
    {
        public ColorGroup ColorGroup { get; set; }
        public string Location { get; set; }
        public MandalaResource MandalaResource { get; set; }
        public AudioClip Music { get; set; }
        public AudioClip VoiceOver { get; set; }
        public ExperienceType ExperienceType { get; set; } = ExperienceType.HOME;
        public string MusicGroup { get; set; }
    }
}
