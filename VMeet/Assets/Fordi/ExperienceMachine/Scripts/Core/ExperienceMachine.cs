using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Fordi.Common;
using Fordi.UI;
using Fordi.UI.MenuControl;

namespace Fordi.Core
{
    public enum AppMode
    {
        TRAINING,
        APPLICATION
    }

    public enum ExperienceType
    {
        HOME,
        NATURE,
        MANDALA,
        ABSTRACT,
        GLOBAL,
        MEETING,
        LOBBY
    }

    public enum GameplayMode
    { 
        NONE,
        PAUSED,
        RUNNING,
    }

    public interface IExperience
    {
        List<ResourceType> MenuSequence { get; }
        List<ResourceType> LearnMenuSequence { get; }
        ExperienceResource[] GetResource(ResourceType resourceType, string category);
        bool CanExecuteMenuCommand(string cmd);
        void ExecuteMenuCommand(MenuClickArgs args);
        void Play();
        void ResumeGuide();
        void Pause();
        void Resume();
        void Stop();
        void ToggleMenu();
        void GoBack();
        void OpenMenu();
        void OpenGridMenu(MenuCommandType commandType);
        void UpdateResourceSelection(MenuClickArgs args);
        void OnLoad();
        void ToggleInventory();
        Transform GetNextTeleportAnchor();
        ResourceComponent[] GetCategories(ResourceType resourceType);
        Experience experience { get; }
    }

    public class Error
    {
        public const int OK = 0;
        public const int E_Exception = 1;
        public const int E_NotFound = 2;
        public const int E_AlreadyExist = 3;
        public const int E_InvalidOperation = 4;
        public const int E_NetworkIssue = 5;

        public int ErrorCode;

        public string ErrorText;

        public bool HasError
        {
            get { return ErrorCode != 0; }
        }

        public Error()
        {
            ErrorCode = OK;
        }

        public Error(int errorCode)
        {
            ErrorCode = errorCode;
        }

        public override string ToString()
        {
            return string.Format("Error({0}): {1}", ErrorCode, ErrorText);
        }
    }

    public interface IExperienceMachine
    {
        void ExecuteMenuCommand(MenuClickArgs args);
        void CanExecuteMenuCommand(MenuItemValidationArgs args);
        IExperience GetExperience(ExperienceType experience);
        Transform GetNextTeleportAnchor();
        IExperience GetExperience(string name);
        void SetExperience(IExperience experience);
        void UpdateResourceSelection(MenuClickArgs args);
        void LoadExperience();
        void SetQualityLevel(GraphicsQuality quality);
        void SetAmbienceAudioVolume(float volume);
        void OpenSceneMenu();
        ExperienceType CurrentExperience { get; }
        bool IsRunning { get; }
    }

    /// <summary>
    /// Uses state design pattern.
    /// All Experience Classes are its states.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class ExperienceMachine : MonoBehaviour, IExperienceMachine
    {
        [SerializeField]
        private OvrAvatar m_avatarPrefab;

        private IExperience m_home, m_nature, m_mandala, m_abstract, m_lobby, m_meeting, m_currentExperience;
        private IMenuSelection m_menuSelection;
        private IAudio m_audio;
        private IUserInterface m_vrMenu;
        private ISettings m_settings;
        private IPlayer m_player;

        public const string DynamicAmbienceTag = "DynamicAmbience";
        public const string CorrectTextColorStyle = "Correct";
        public const string ErrorTextColorStyle = "Error";
        public const string ProgressTextColorStyle = "Progress";

        #region GUIDE_CONDITIONS
        private bool m_clicked = false;
        #endregion

        public static AppMode AppMode { get; set; } = AppMode.APPLICATION;

        private ExperienceType m_currentExperienceType;
        public ExperienceType CurrentExperience { get { return m_currentExperienceType; } }

        private bool m_isRunning = false;
        public bool IsRunning { get { return m_isRunning; } }

        private const string MeetingScene = "Meeting";

        private void Awake()
        {
            m_isRunning = true;
            m_home = GetComponentInChildren<Home>();
            m_nature = GetComponentInChildren<NatureExperience>();
            m_mandala = GetComponentInChildren<MandalaExperience>();
            m_abstract = GetComponentInChildren<AbstractExperience>();
            m_lobby = GetComponentInChildren<Lobby>();
            m_meeting = GetComponentInChildren<MeetingExperience>();
           
            m_menuSelection = IOC.Resolve<IMenuSelection>();
            m_audio = IOC.Resolve<IAudio>();
            m_vrMenu = IOC.Resolve<IUserInterface>();
            m_settings = IOC.Resolve<ISettings>();
            m_player = IOC.Resolve<IPlayer>();

            SetExperience(GetExperience(m_menuSelection.ExperienceType));
            UIInteractionBase.OnClick += Click;
            ResetGuideConditions();
        }

        private void ResetGuideConditions()
        {
            m_clicked = false;
        }

        private void OnDestroy()
        {
            UIInteractionBase.OnClick -= Click;
        }

        private IEnumerator Start()
        {
            yield return null;
            m_currentExperience.OnLoad();
            MinimumWindowSize.Set(1472, 828);
        }

        private void Update()
        {
            if (m_settings.SelectedPreferences.DesktopMode)
                return;

            if (FordiInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.LTouch))
                ToggleMenu();
            if (FordiInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
            {
                if (m_vrMenu.IsOpen)
                    ToggleMenu();
                else
                {
                    ToggleMenu();
                    m_currentExperience.ToggleInventory();
                    //ExecuteMenuCommand(new MenuClickArgs("", "", "", MenuCommandType.INVENTORY, null));
                }
            }
        }

        public IExperience GetExperience(ExperienceType experience)
        {
            switch (experience)
            {
                case ExperienceType.HOME:
                    return m_home;
                case ExperienceType.NATURE:
                    return m_nature;
                case ExperienceType.MANDALA:
                    return m_mandala;
                case ExperienceType.ABSTRACT:
                    return m_abstract;
                case ExperienceType.MEETING:
                    return m_meeting;
                case ExperienceType.LOBBY:
                    return m_lobby;
                default:
                    return null;
            }
        }

        public void SetExperience(IExperience experience)
        {
            m_currentExperience = experience;
            if (m_currentExperience == m_home)
                m_currentExperienceType = ExperienceType.HOME;
            if (m_currentExperience == m_mandala)
                m_currentExperienceType = ExperienceType.MANDALA;
            if (m_currentExperience == m_nature)
                m_currentExperienceType = ExperienceType.NATURE;
            if (m_currentExperience == m_abstract)
                m_currentExperienceType = ExperienceType.ABSTRACT;
            if (m_currentExperience == m_lobby)
                m_currentExperienceType = ExperienceType.LOBBY;
            if (m_currentExperience == m_meeting)
                m_currentExperienceType = ExperienceType.MEETING;
        }

        #region EXPERIENCE_INTERFACE
        public void CanExecuteMenuCommand(MenuItemValidationArgs args)
        {
            args.IsValid = m_currentExperience.CanExecuteMenuCommand(args.Command);
        }

        public void ExecuteMenuCommand(MenuClickArgs args)
        {
            m_currentExperience.ExecuteMenuCommand(args);
        }
        
        public void Play()
        {
            m_currentExperience.Play();
        }

        public void Pause()
        {
            m_currentExperience.Pause();
        }

        public void Resume()
        {
            m_currentExperience.Resume();
        }

        public void Stop()
        {
            m_currentExperience.Stop();
        }
        
        public void ToggleMenu()
        {
            m_currentExperience.ToggleMenu();
        }

        public void GoBack()
        {
            m_currentExperience.GoBack();
        }

        public IExperience GetExperience(string name)
        {
            switch (name.ToLower())
            {
                case "home":
                    return m_home;
                case "nature":
                    return m_nature;
                case "mandala":
                    return m_mandala;
                case "abstract":
                    return m_abstract;
                case "meeting":
                    return m_meeting;
                case "lobby":
                    return m_lobby;
                default:
                    return null;
            }
        }
        
        public void UpdateResourceSelection(MenuClickArgs args)
        {
            //Debug.LogError("UpdateResourceSelection");
            if (args.Data != null && args.Data is ColorGroup)
            {
                var experience = GetExperience(m_menuSelection.ExperienceType);
                experience.UpdateResourceSelection(args);
            }
            else
            {
                if (args.Data != null && ((ExperienceResource)args.Data).ResourceType == ResourceType.EXPERIENCE)
                {
                    m_home.UpdateResourceSelection(args);
                }
                else
                {
                    var experience = GetExperience(m_menuSelection.ExperienceType);
                    experience.UpdateResourceSelection(args);
                }
            }
        }

        public void LoadExperience()
        {
            //Debug.LogError("LoadExperience: " + m_menuSelection.Location + " " + m_menuSelection.ExperienceType.ToString());
            AudioArgs args = new AudioArgs(null, AudioType.MUSIC)
            {
                FadeTime = 2,
                Done = () =>
                {
                    if (PhotonNetwork.InRoom)
                        PhotonNetwork.LoadLevel(m_menuSelection.Location);
                    else
                        SceneManager.LoadSceneAsync(m_menuSelection.Location);
                }
            };
            m_audio.Stop(args);

            m_player.FadeOut();

            AudioArgs voArgs = new AudioArgs(null, AudioType.VO)
            {
                FadeTime = 2,
            };
            m_audio.Stop(voArgs);

            m_vrMenu.DisplayProgress("Loading: " + m_menuSelection.Location, true);
            //SceneManager.LoadScene(m_menuSelection.Location);
        }
        #endregion

        private void OnApplicationQuit()
        {
#if !UNITY_EDITOR
            System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
            MinimumWindowSize.Reset();
        }

        public Transform GetNextTeleportAnchor()
        {
            return m_currentExperience.GetNextTeleportAnchor();
        }

        public void SetQualityLevel(GraphicsQuality quality)
        {
            QualitySettings.SetQualityLevel((int)quality, false);
        }

        public void Click(object sender, PointerEventData eventData)
        {
            m_clicked = true;

            //if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            //{
            //    Debug.LogError("Action tru");
            //}
            //else
            //    Debug.LogError("Action false");

            StartCoroutine(ResetClick());
        }

        private IEnumerator ResetClick()
        {
            yield return null;

            //if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            //{
            //    Debug.LogError("Action tru");
            //}
            //else
            //    Debug.LogError("Action false");

            m_clicked = false;
            //Debug.LogError(m_clicked);
        }

        public bool ClickCondition()
        {
            return m_clicked;
        }

        public void SetAmbienceAudioVolume(float volume)
        {
            GameObject[] dynamicParticles = GameObject.FindGameObjectsWithTag(DynamicAmbienceTag);
            foreach (var item in dynamicParticles)
            {
                var audiSources = item.GetComponentsInChildren<AudioSource>();
                foreach (var audioSource in audiSources)
                    audioSource.volume = volume;
            }
        }

        public void SetupPersonalisedAvatar(string id)
        {
            m_avatarPrefab.oculusUserID = id;
        }

        public void OpenSceneMenu()
        {
            m_vrMenu.Close();
            ToggleMenu();
        }
    }
}
