using Cornea.Web;
using Fordi.Networking;
using Fordi.ScreenSharing;
using Fordi.Sync;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRExperience.Common;
using VRExperience.UI.MenuControl;
using Network = Fordi.Networking.Network;

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

        private IFordiNetwork m_fordiNetwork;

        protected virtual IFordiNetwork FordiNetwork
        {
            get
            {
                FordiNetwork fordiNetwork = FindObjectOfType<FordiNetwork>();
                if (fordiNetwork == null)
                {
                    var obj = new GameObject("FordiNetwork");
                    fordiNetwork = obj.AddComponent<FordiNetwork>();
                    fordiNetwork.transform.parent = transform;
                    fordiNetwork.transform.localPosition = Vector3.zero;
                }
                return fordiNetwork;
            }
        }

        private IWebInterface m_webInterface;

        protected virtual IWebInterface WebInterface
        {
            get
            {
                WebInterface webInterface = FindObjectOfType<WebInterface>();
                if (webInterface == null)
                {
                    var obj = new GameObject("WebInterface");
                    webInterface = obj.AddComponent<WebInterface>();
                    webInterface.transform.parent = transform;
                    webInterface.transform.localPosition = Vector3.zero;
                }
                return webInterface;
            }
        }

        private INetwork m_network;

        protected virtual INetwork Network
        {
            get
            {
                Network network = FindObjectOfType<Network>();
                if (network == null)
                {
                    var obj = new GameObject("Network");
                    network = obj.AddComponent<Network>();
                    network.transform.parent = transform;
                    network.transform.localPosition = Vector3.zero;
                }
                return network;
            }
        }

        private IMouseControl m_mouseControl;

        protected virtual IMouseControl MouseControl
        {
            get
            {
                MouseControl mouseControl = FindObjectOfType<MouseControl>();
                if (mouseControl == null)
                {
                    var obj = new GameObject("MouseControl");
                    mouseControl = obj.AddComponent<MouseControl>();
                    mouseControl.transform.parent = transform;
                    mouseControl.transform.localPosition = Vector3.zero;
                }
                return mouseControl;
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
            m_fordiNetwork = FordiNetwork;
            m_webInterface = WebInterface;
            m_network = Network;
            m_mouseControl = MouseControl;
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
            IOC.RegisterFallback(() => Instance.m_fordiNetwork);
            IOC.RegisterFallback(() => Instance.m_webInterface);
            IOC.RegisterFallback(() => Instance.m_network);
            IOC.RegisterFallback(() => Instance.m_mouseControl);

            if (IOC.Resolve<IMenuSelection>() == null)
                IOC.Register<IMenuSelection>(new MenuSelection());
        }

        private static void OnSceneUnloaded(Scene arg0)
        {
            m_instance = null;
        }
    }
}

