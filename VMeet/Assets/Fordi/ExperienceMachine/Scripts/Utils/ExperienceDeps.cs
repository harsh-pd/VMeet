using UnityEngine;
using UnityEngine.SceneManagement;
using VRExperience.Common;
using VRExperience.UI.MenuControl;

namespace VRExperience.Core
{
    [DefaultExecutionOrder(-100)]
    public class ExperienceDeps : MonoBehaviour 
    {
        private IExperienceMachine m_experienceMachine;

        protected virtual IExperienceMachine ExperienceMachine
        {
            get
            {
                ExperienceMachine experienceMachine = FindObjectOfType<ExperienceMachine>();
                if(experienceMachine == null)
                {
                    experienceMachine = gameObject.AddComponent<ExperienceMachine>();
                }
                return experienceMachine;
            }
        }

        private IAppTheme m_appTheme;

        protected virtual IAppTheme AppTheme
        {
            get
            {
                AppTheme appTheme = FindObjectOfType<AppTheme>();
                if (appTheme == null)
                {
                    appTheme = gameObject.AddComponent<AppTheme>();
                }
                return appTheme;
            }
        }

        private IAudio m_audio;

        protected virtual IAudio Audio
        {
            get
            {
                Audio audio = FindObjectOfType<Audio>();
                if (audio == null)
                {
                    var obj = new GameObject("Audio");
                    audio = obj.AddComponent<Audio>();
                    audio.transform.parent = transform;
                    audio.transform.localPosition = Vector3.zero;
                }
                return audio;
            }
        }

        private IVRMenu m_vRMenu;

        protected virtual IVRMenu VRMenu
        {
            get
            {
                VRMenu vrMenu = FindObjectOfType<VRMenu>();
                return vrMenu;
            }
        }

        private ISettings m_settings;

        protected virtual ISettings Settings
        {
            get
            {
                Settings settings = FindObjectOfType<Settings>();
                if (settings == null)
                    settings = gameObject.AddComponent<Settings>();
                return settings;
            }
        }

        private IPlayer m_player;

        protected virtual IPlayer Player
        {
            get
            {
                Player player = FindObjectOfType<Player>();
                return player;
            }
        }

        private ICommonResource m_commonResource;

        protected virtual ICommonResource CommonResource
        {
            get
            {
                CommonResource commonResource = FindObjectOfType<CommonResource>();
                if (commonResource == null)
                {
                    var obj = new GameObject("CommonResource");
                    commonResource = obj.AddComponent<CommonResource>();
                    commonResource.transform.parent = transform;
                    commonResource.transform.localPosition = Vector3.zero;
                }
                return commonResource;
            }
        }

        private void Awake()
        {
            if(m_instance != null)
            {
                Debug.LogWarning("AnotherInstance of ExperienceDeps exists");
            }
            m_instance = this;

            AwakeOverride();
        }

        protected virtual void AwakeOverride()
        {
            m_experienceMachine = ExperienceMachine;
            m_appTheme = AppTheme;
            m_audio = Audio;
            m_vRMenu = VRMenu;
            m_commonResource = CommonResource;
            m_player = Player;
            m_settings = Settings;
        }

        private void OnDestroy()
        {
            if(m_instance == this)
            {
                m_instance = null;
            }

            OnDestroyOverride();

            m_experienceMachine = null;
        }

        protected virtual void OnDestroyOverride()
        {

        }


        private static ExperienceDeps m_instance;
        private static ExperienceDeps Instance
        {
            get
            {
                if(m_instance == null)
                {
                    ExperienceDeps deps = FindObjectOfType<ExperienceDeps>();
                    if (deps == null)
                    {
                        GameObject go = new GameObject("ExperienceDeps");
                        go.AddComponent<ExperienceDeps>();
                        go.transform.SetSiblingIndex(0);
                    }   
                    else
                    {
                        m_instance = deps;
                    }
                }
                return m_instance;
            }    
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if(!Application.isPlaying)
            {
                return;
            }

            RegisterExpDeps();
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private static void RegisterExpDeps()
        {
            IOC.RegisterFallback(() => Instance.m_experienceMachine);
            IOC.RegisterFallback(() => Instance.m_appTheme);
            IOC.RegisterFallback(() => Instance.m_vRMenu);
            IOC.RegisterFallback(() => Instance.m_audio);
            IOC.RegisterFallback(() => Instance.m_commonResource);
            IOC.RegisterFallback(() => Instance.m_player);
            IOC.RegisterFallback(() => Instance.m_settings);
            if (IOC.Resolve<IMenuSelection>() == null)
                IOC.Register<IMenuSelection>(new MenuSelection());
        }

        private static void OnSceneUnloaded(Scene arg0)
        {
            m_instance = null;
        }
    }
}

