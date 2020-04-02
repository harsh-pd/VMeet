using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRExperience.Common;
using VRExperience.UI.MenuControl;

namespace VRExperience.Core
{
    public interface ICommonResource
    {
        ExperienceResource[] GetResource(ResourceType type, string category);
        ResourceComponent[] GetCategories(ResourceType type);
        AudioClip GetGuideClip(MenuCommandType commandType);
        AssetDB AssetDb { get; }
    }

    public abstract class ResourceComponent
    {
        public string Name;
        public string Description;
        public ResourceType ResourceType;
        public Sprite Preview;
        public Sprite LargePreview;
        public string SpecialCommand;
    }

    [Serializable]
    public class ExperienceGroup : ResourceComponent
    {
        public ExperienceResource[] Resources;
    }

    [Serializable]
    public class AudioGroup : ResourceComponent
    {
        public AudioResource[] Resources;
    }

    [Serializable]
    public class VOGroup: AudioGroup
    {
        public string MusicGroupName;
    }

    [Serializable]
    public class ObjectGroup : ResourceComponent
    {
        public ObjectResource[] Resources;
    }

    [Serializable]
    public class GuideAudioGroup : ResourceComponent
    {
        public GuideAudioResource[] Resources;
    }

    public class CommonResource : MonoBehaviour, ICommonResource
    {
        private VOGroup[] m_voiceOvers;
        private GuideAudioGroup[] m_controlGuideVO;
        private ObjectGroup[] m_objects;

        [SerializeField]
        private AssetDB m_assetDb;
        public AssetDB AssetDb { get { return m_assetDb; } }


        public const string MainCategory = "Main Guide VO";
        public const string TrainingCategory = "Training Guide VO";

        public const string SampleResource = "Sample";

        private IExperienceMachine m_experienceMachine;

        private string GuideCategory
        {
            get
            {
                if (m_experienceMachine == null)
                    m_experienceMachine = IOC.Resolve<IExperienceMachine>();
                if (ExperienceMachine.AppMode == AppMode.APPLICATION)
                    return MainCategory;
                else
                    return TrainingCategory;
            }
        }

        private void Awake()
        {
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_voiceOvers = m_assetDb.AudioGroups;
            m_controlGuideVO = m_assetDb.GuideAudioGroups;
            m_objects = m_assetDb.ObjectGroups;
        }

        public ResourceComponent[] GetCategories(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.AUDIO:
                    return Array.FindAll(m_voiceOvers, item => item.Name != null && !item.Name.Equals(SampleResource));
                case ResourceType.OBJECT:
                    return m_objects;
                default:
                    return null;
            }
        }

        public ExperienceResource[] GetResource(ResourceType resourceType, string category)
        {
            try
            {
                switch (resourceType)
                {
                    case ResourceType.AUDIO:
                        return Array.Find(m_voiceOvers, item => item.Name.Equals(category)).Resources;
                    case ResourceType.SFX:
                        return Array.Find(m_assetDb.SFXGroups, item => item.Name.Equals(category)).Resources;
                    case ResourceType.AMBIENCE_MUSIC:
                        return Array.Find(m_assetDb.AmbienceAudioGroups, item => item.Name.Equals(category)).Resources;
                    case ResourceType.COLOR_AUDIO:
                        return Array.Find(m_assetDb.ColorAudioGroups, item => item.Name.Equals(category)).Resources;
                    case ResourceType.OBJECT:
                        return Array.Find(m_assetDb.ObjectGroups, item => item.Name.Equals(category)).Resources;
                    default:
                        return null;
                }
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public AudioClip GetGuideClip(MenuCommandType commandType)
        {
            //GuideAudioGroup audioGroup = Array.Find(m_controlGuideVO, item => item.Name == category);

            GuideAudioGroup audioGroup = null;
           
            audioGroup = Array.Find(m_controlGuideVO, item => item.Name == GuideCategory);

            if (audioGroup == null)
                return null;

            GuideAudioResource resource = Array.Find(audioGroup.Resources, item => item.CommandType == commandType);
            if (resource != null)
                return resource.Clip;
            else
                return null;
        }
    }
}