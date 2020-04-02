using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRExperience.UI.MenuControl;
using VRExperience.Common;
using ProtoBuf;
using System.IO;
using Papae.UnitySDK.Managers;
using Random = UnityEngine.Random;

namespace VRExperience.Core
{
    [Serializable]
    public class MandalaGroup : ResourceComponent
    {
        public MandalaResource[] Resources;
        public string MusicGroupName;
    }

    [Serializable]
    public class ColorGroup : ResourceComponent
    {
        public ColorResource[] Resources;
    }

    [Serializable]
    [ProtoContract]
    public class PersistentMandalaInfo
    {
        [ProtoMember(1)]
        public Dictionary<int, int[]> MandalaPresets = new Dictionary<int, int[]>();
    }

    public class MandalaExperience : Gameplay
    {
        private MandalaGroup[] m_mandalas;
        private ColorGroup[] m_colors;

        private const string DynamicParticlesTag = "DynamicParticles";

        public const string ColorBasedAudioCommand = "ColorBasedAudio";

        public const string MainColor = "Main";
        public const string SupportColor = "Support";

        private const string MandalaInfo = "MandalaInfo.info";

        public const string PrimaryMain = "p1";
        public const string PrimarySupport = "p2";
        public const string SecondaryMain = "s1";
        public const string SecondarySupport = "s2";
        public const string TertiaryMain = "t1";
        public const string TertiarySupport = "t2";

        [SerializeField]
        private Gradient m_particlesMainGradient, m_particlesSupportGradient;

        private PersistentMandalaInfo m_mandalaInfo = null;

        private List<GameObject> m_mandalaInstances = new List<GameObject>();

        private int[] m_colorIndices = new int[3];

        private bool m_allowAnimation;
        public bool AllowAnimation {
            get
            {
                return m_allowAnimation;
            }
            set
            {
                m_allowAnimation = value;
                foreach (var item in m_mandalaInstances)
                {
                    Animator anim = item.GetComponent<Animator>();
                    if (anim != null)
                        anim.enabled = value;
                }
            }
        }

        private ParticleSystem[] m_particles = null;
        private bool m_allowParticles;
        public bool AllowParticles
        {
            get
            {
                return m_allowParticles;
            }
            set
            {
                m_allowParticles = value;
                if (m_particles == null)
                    m_particles = FindObjectsOfType<ParticleSystem>();
                foreach (var item in m_particles)
                    item.gameObject.SetActive(value);
            }
        }

        private bool m_audioInterruptionFlag = false;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_mandalas = m_commonResource.AssetDb.MandalaGroups;
            m_colors = m_commonResource.AssetDb.ColorGroups;
            m_music = m_commonResource.AssetDb.MandalaMusic;
            m_vrMenu.AudioInterruptionEvent += AudioInterruption;        
            LoadCustomPresets();
        }

        protected override void OnDestroyOverride()
        {
            m_vrMenu.AudioInterruptionEvent -= AudioInterruption;
        }

        private void AudioInterruption(object sender, EventArgs e)
        {
            m_audioInterruptionFlag = true;
            if (m_colorGuideQueue.Count > 0)
                m_colorGuideQueue.Peek().Time = m_audio.GetAudioSource(AudioType.VO).time;
            if (m_coColorMeditation != null)
            {
                StopCoroutine(m_coColorMeditation);
                m_coColorMeditation = null;
            }
        }

        public override void ExecuteMenuCommand(MenuClickArgs args)
        {
            if (args.CommandType == MenuCommandType.SAVE_PRESET)
            {
                SaveCustomPreset((ColorResource[])args.Data);
                return;
            }

            if (args.CommandType == MenuCommandType.CATEGORY_SELECTION)
            {
                ResourceComponent resourceComponent = (ResourceComponent)args.Data;
                if (resourceComponent.SpecialCommand == ColorBasedAudioCommand)
                {
                    m_menuSelection.VoiceOver = null;
                    m_vrMenu.Close();
                    AudioSelectionFlag = false;
                    return;
                }
            }

            base.ExecuteMenuCommand(args);
        }

        private void LoadCustomPresets()
        {
            string mandalaInfoPath = Path.Combine(Application.persistentDataPath, MandalaInfo);

            if (File.Exists(mandalaInfoPath))
            {
                try
                {
                    using (FileStream stream = new FileStream(mandalaInfoPath, FileMode.Open, FileAccess.Read))
                    {
                        m_mandalaInfo = Serializer.Deserialize<PersistentMandalaInfo>(new FileStream(mandalaInfoPath, FileMode.Open, FileAccess.Read));
                    }

                    if (m_mandalaInfo != null)
                    {
                        foreach (KeyValuePair<int, int[]> item in m_mandalaInfo.MandalaPresets)
                        {
                            foreach (var mandalaGroup in m_mandalas)
                            {
                                foreach (var mandalaResource in mandalaGroup.Resources)
                                {
                                    if (mandalaResource.Mandala != null && mandalaResource.Mandala.GetInstanceID() == item.Key)
                                    {

                                        if (m_colors.Length == 0)
                                        {
                                            Debug.LogError("Can't load custom preset, mandala color resources empty");
                                            return;
                                        }

                                        ColorResource[] colorResources = m_colors[0].Resources;

                                        ColorResource[] customPreset = new ColorResource[3];

                                        for (int i = 0; i < 3; i++)
                                        {
                                            if (item.Value[i] < 0)
                                            {
                                                Debug.LogError("Failed to load custom preset. Custom preset invalid");
                                                return;
                                            }
                                            if (item.Value[i] > colorResources.Length - 1)
                                            {
                                                Debug.LogError("Failed to load custom preset, mandala color resources corrupt");
                                                return;
                                            }

                                            customPreset[i] = colorResources[item.Value[i]];
                                        }

                                        mandalaResource.CustomPreset = customPreset;
                                    }
                                }
                            }
                        }
                    }

                    return;
                }
                catch (Exception)
                {

                }
            }
        }

        private void SaveCustomPreset(ColorResource[] presetColorResources)
        {
            if (m_mandalaInfo == null)
                m_mandalaInfo = new PersistentMandalaInfo();

            string mandalaInfoPath = Path.Combine(Application.persistentDataPath, MandalaInfo);

            if (presetColorResources != null && presetColorResources.Length == 3)
            {
                ColorResource[] customPreset = presetColorResources;

                if (m_colors.Length == 0)
                {
                    Debug.LogError("Can't save color preset, mandala color resources corrupt.");
                    return;
                }
                
                int[] colorIndices = new int[3];
                ColorResource[] colors = m_colors[0].Resources;

                for (int i = 0; i < 3; i++)
                {
                    int index = Array.FindIndex(colors, item => item.Name == customPreset[i].Name && item.Color == customPreset[i].Color);
                    if (index == -1)
                    {
                        Debug.LogError("Can't save preset, color resource: " + customPreset[i].Name + " not available in mandala color resources for reference");
                        return;
                    }
                    colorIndices[i] = index;
                }

                m_mandalaInfo.MandalaPresets[m_menuSelection.MandalaResource.Mandala.GetInstanceID()] = colorIndices;

                using (FileStream stream = new FileStream(mandalaInfoPath, FileMode.Create, FileAccess.Write))
                {
                    Serializer.Serialize(stream, m_mandalaInfo);
                }

            }

        }

        private MenuItemInfo[] GetColorItems()
        {
            MenuItemInfo[] menuItems = new MenuItemInfo[m_colors.Length];
            for (int i = 0; i < m_colors.Length; i++)
            {
                menuItems[i] = new MenuItemInfo
                {
                    Path = "",
                    Text = "",
                    Command = "",
                    Icon = null,
                    Data = m_colors[i],
                    CommandType = MenuCommandType.SELECTION
                };
            }
            return menuItems;
        }

        public override void UpdateResourceSelection(MenuClickArgs args)
        {
            base.UpdateResourceSelection(args);

            if (args.Data != null && args.Data is ResourceComponent)
            {
                ResourceComponent resource = (ResourceComponent)args.Data;

                if (resource.ResourceType == ResourceType.MANDALA)
                {
                    var mandalaResource = ((MandalaResource)resource);
                    m_menuSelection.MandalaResource = mandalaResource;
                    m_menuSelection.Location = mandalaResource.SceneName;
                }
                else if (resource.ResourceType == ResourceType.COLOR)
                {
                    m_menuSelection.ColorGroup = (ColorGroup)args.Data;
                }
            }
        }

        public override ExperienceResource[] GetResource(ResourceType resourceType, string category)
        {
            ExperienceResource[] resources = base.GetResource(resourceType, category);
            if (resources != null)
                return resources;
            
            switch (resourceType)
            {
                case ResourceType.MANDALA:
                    return Array.Find(m_mandalas, item => item.Name.Equals(category)).Resources;
                case ResourceType.COLOR:
                    return Array.Find(m_colors, item => item.Name.Equals(category)).Resources;
                case ResourceType.MUSIC:
                    return Array.Find(m_music, item => item.Name.Equals(category)).Resources;
            }

            return null;
        }

        private void LoadMandalas()
        {
            GameObject mandala1, mandala2, mandala3, mandala4, mandala5;
            if (m_menuSelection.MandalaResource != null)
            {
                mandala1 = Instantiate(m_menuSelection.MandalaResource.Mandala);

                Animator animator = mandala1.GetComponent<Animator>();
                if (animator != null)
                    animator.enabled = m_settings.SelectedPreferences.MandalaAnimation;

                for (int i = 0; i < 3; i++)
                {
                    m_colorIndices[i] = Array.FindIndex(m_colors[0].Resources, item =>  item.Name == m_menuSelection.ColorGroup.Resources[i].Name);
                    if (m_colorIndices[i] == -1)
                        return;
                }

                List<Material> materials = new List<Material>();

                if (mandala1 != null)
                {
                    Renderer[] renderers = mandala1.GetComponentsInChildren<Renderer>();
                    foreach (var item in renderers)
                        materials.AddRange(item.materials);
                }

                foreach (var item in materials)
                {
                    if (item.name.Length > 1)
                    {
                        string prefix = item.name.Substring(0, 2);
                        switch (prefix)
                        {
                            case PrimaryMain:
                                item.color = m_colors[0].Resources[m_colorIndices[0]].Color;
                                item.SetColor("_EmissionColor", item.color);
                                break;
                            case SecondaryMain:
                                item.color = m_colors[0].Resources[m_colorIndices[1]].Color;
                                item.SetColor("_EmissionColor", m_colors[0].Resources[m_colorIndices[1]].Color);
                                item.SetColor("_EmissionColor", item.color);
                                break;
                            case TertiaryMain:
                                item.color = m_colors[0].Resources[m_colorIndices[2]].Color;
                                item.SetColor("_EmissionColor", m_colors[0].Resources[m_colorIndices[2]].Color);
                                item.SetColor("_EmissionColor", item.color);
                                break;
                            case PrimarySupport:
                                item.color = m_colors[1].Resources[m_colorIndices[0]].Color;
                                item.SetColor("_EmissionColor", m_colors[1].Resources[m_colorIndices[0]].Color);
                                item.SetColor("_EmissionColor", item.color);
                                break;
                            case SecondarySupport:
                                item.color = m_colors[1].Resources[m_colorIndices[1]].Color;
                                item.SetColor("_EmissionColor", m_colors[1].Resources[m_colorIndices[1]].Color);
                                item.SetColor("_EmissionColor", item.color);
                                break;
                            case TertiarySupport:
                                item.color = m_colors[1].Resources[m_colorIndices[2]].Color;
                                item.SetColor("_EmissionColor", m_colors[1].Resources[m_colorIndices[2]].Color);
                                item.SetColor("_EmissionColor", item.color);
                                break;
                        }
                    }
                }

                mandala2 = Instantiate(mandala1);
                mandala3 = Instantiate(mandala1);
                mandala4 = Instantiate(mandala1);
                mandala5 = Instantiate(mandala1);

                mandala2.transform.position = new Vector3(-mandala1.transform.position.z - .5f, mandala1.transform.position.y, 0);
                mandala2.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));

                mandala3.transform.position = new Vector3(mandala1.transform.position.z + .5f, mandala1.transform.position.y, 0);
                mandala3.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));

                mandala4.transform.position = new Vector3(mandala1.transform.position.x, mandala1.transform.position.y, -mandala1.transform.position.z);
                mandala4.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

                mandala5.transform.position = new Vector3(0, mandala1.transform.position.z * 2, 0);
                mandala5.transform.rotation = Quaternion.Euler(new Vector3(90, 180, 0));

                m_mandalaInstances.Add(mandala1);
                m_mandalaInstances.Add(mandala2);
                m_mandalaInstances.Add(mandala3);
                m_mandalaInstances.Add(mandala4);
                m_mandalaInstances.Add(mandala5);

            }
        }

        private void ApplyColorOnParticles()
        {
            GameObject[] dynamicParticles = GameObject.FindGameObjectsWithTag(DynamicParticlesTag);
            foreach (var item in dynamicParticles)
            {
                var particles = item.GetComponentsInChildren<ParticleSystem>();

                GradientColorKey[] colorKeys = new GradientColorKey[3];

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0].alpha = 1.0f;
                alphaKeys[0].time = 0.0f;
                alphaKeys[1].alpha = 1.0f;
                alphaKeys[1].time = 1.0f;

                m_particlesMainGradient = new Gradient();
                m_particlesMainGradient.mode = GradientMode.Fixed;
                m_particlesSupportGradient = new Gradient();
                m_particlesSupportGradient.mode = GradientMode.Fixed;

                for (int i = 0; i < 3; i++)
                {
                    colorKeys[i].color = m_colors[0].Resources[m_colorIndices[i]].Color;
                    colorKeys[i].time = 1 * (i + 1) / 3.0f;
                }
                m_particlesMainGradient.SetKeys(colorKeys, alphaKeys);

                for (int i = 0; i < 3; i++)
                {
                    colorKeys[i].color = m_colors[1].Resources[m_colorIndices[i]].Color;
                }
                m_particlesSupportGradient.SetKeys(colorKeys, alphaKeys);

                var startColor =  new ParticleSystem.MinMaxGradient(m_particlesMainGradient, m_particlesSupportGradient);
                foreach (var particle in particles)
                {
                    var main = particle.main;
                    main.startColor = startColor;
                }
            }
        }

        public override void OnLoad()
        {
            base.OnLoad();
            
            LoadMandalas();

            //Don't change the order
            AllowParticles = m_settings.SelectedPreferences.MandalaParticles;
            ApplyColorOnParticles();
            PopulateColorGuideStack();
            if (m_menuSelection.VoiceOver == null && ExperienceMachine.AppMode == AppMode.APPLICATION)
                PlayColorMeditation();
        }

        private AudioClip GetRandomGuideClip(string colorName)
        {
            var audioClips = (AudioResource[])m_commonResource.GetResource(ResourceType.COLOR_AUDIO, colorName);

            if (audioClips == null)
                return null;

            audioClips = audioClips.RemoveAll(item => item.Clip != null);

            if (audioClips.Length > 0)
                return audioClips[Random.Range(0, audioClips.Length)].Clip;

            return null;
        }

        private Queue<Sound> m_colorGuideQueue = new Queue<Sound>();

        private IEnumerator m_coColorMeditation = null;

        public override void ResumeGuide()
        {
            base.ResumeGuide();
            if (m_audioInterruptionFlag)
                PlayColorMeditation();
            m_audioInterruptionFlag = false;
        }

        private void PlayColorMeditation()
        {
            if (ExperienceMachine.AppMode == AppMode.TRAINING)
                return;

            if (m_coColorMeditation != null)
                StopCoroutine(m_coColorMeditation);

            m_coColorMeditation = CoGuide();
            StartCoroutine(m_coColorMeditation);
        }

        private IEnumerator CoGuide()
        {
            AudioSource voSource = m_audio.GetAudioSource(AudioType.VO);
            yield return null;
            
            AudioArgs args = new AudioArgs(m_colorGuideQueue.Peek().Clip, AudioType.VO)
            {
                FadeTime = .5f,
                ResumeTime = m_colorGuideQueue.Peek().Time
            };
            m_audio.Resume(args);
            

            yield return new WaitUntil(() => voSource.clip == m_colorGuideQueue.Peek().Clip && !voSource.isPlaying);
            m_colorGuideQueue.Dequeue();

            if (m_colorGuideQueue.Count == 0)
                yield break;

            yield return new WaitForSeconds(3.0f);

            args = new AudioArgs(m_colorGuideQueue.Peek().Clip, AudioType.VO)
            {
                FadeTime = .5f,
                ResumeTime = m_colorGuideQueue.Peek().Time
            };
            m_audio.Resume(args);

            yield return new WaitUntil(() => voSource.clip == m_colorGuideQueue.Peek().Clip && !voSource.isPlaying);
            m_colorGuideQueue.Dequeue();

            if (m_colorGuideQueue.Count == 0)
                yield break;

            yield return new WaitForSeconds(3.0f);

            args = new AudioArgs(m_colorGuideQueue.Peek().Clip, AudioType.VO)
            {
                FadeTime = .5f,
                ResumeTime = m_colorGuideQueue.Peek().Time
            };
            m_audio.Resume(args);
        }

        private void PopulateColorGuideStack()
        {
            m_colorGuideQueue.Clear();

            AudioClip clip = GetRandomGuideClip(m_menuSelection.ColorGroup.Resources[0].Name);
            if (clip != null)
                m_colorGuideQueue.Enqueue(new Sound(0, clip));

            if (m_menuSelection.ColorGroup.Resources[1].Name != m_menuSelection.ColorGroup.Resources[0].Name)
            {
                clip = GetRandomGuideClip(m_menuSelection.ColorGroup.Resources[1].Name);
                if (clip != null)
                    m_colorGuideQueue.Enqueue(new Sound(0, clip));
            }

            if (m_menuSelection.ColorGroup.Resources[2].Name != m_menuSelection.ColorGroup.Resources[0].Name && m_menuSelection.ColorGroup.Resources[2].Name != m_menuSelection.ColorGroup.Resources[1].Name)
            {
                clip = GetRandomGuideClip(m_menuSelection.ColorGroup.Resources[2].Name);
                if (clip != null)
                    m_colorGuideQueue.Enqueue(new Sound(0, clip));
            }
        }

        public override ResourceComponent[] GetCategories(ResourceType resourceType)
        {
            ResourceComponent[] categories = base.GetCategories(resourceType);

            if (resourceType == ResourceType.AUDIO)
                return new ResourceComponent[] { m_commonResource.AssetDb.MandalaColorCategory }.Concatenate(categories);

            if (categories != null)
                return categories;

            if (resourceType == ResourceType.MANDALA)
                return m_mandalas;

            if (resourceType == ResourceType.COLOR)
                return m_colors;

            if (resourceType == ResourceType.MUSIC)
                return m_music;

            return null;
        }
    }
}
