using Cornea.Web;
using Fordi.Annotations;
using Fordi.Networking;
using Fordi.ScreenSharing;
using Fordi.Sync;
using Fordi.Voice;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fordi.Common;
using Fordi.UI.MenuControl;
using Network = Fordi.Networking.Network;
using Fordi.UI;
using Fordi.AssetManagement;
using Fordi.Plugins;
using Fordi.VideoCall;

namespace Fordi.Core
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


        private IVideoCallEngine m_videoCallEngine;

        protected virtual IVideoCallEngine VideoCallEngine
        {
            get
            {
                VideoCallEngine dep = FindObjectOfType<VideoCallEngine>();
                if (dep == null)
                {
                    var obj = new GameObject("VideoCallEngine");
                    dep = obj.AddComponent<VideoCallEngine>();
                    dep.transform.parent = m_modulesRoot;
                    dep.transform.localPosition = Vector3.zero;
                }
                return dep;
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
                    audio.transform.parent = m_modulesRoot;
                    audio.transform.localPosition = Vector3.zero;
                }
                return audio;
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

        private IUIEngine m_uiEngine;

        protected virtual IUIEngine UIEngine
        {
            get
            {
                UIEngine uiEngine = FindObjectOfType<UIEngine>();
                if (uiEngine == null)
                {
                    var obj = new GameObject("UIEngine");
                    uiEngine = obj.AddComponent<UIEngine>();
                    uiEngine.transform.parent = m_modulesRoot;
                    uiEngine.transform.localPosition = Vector3.zero;
                }
                return uiEngine;
            }
        }

        private IAssetLoader m_assetLoader;

        protected virtual IAssetLoader AssetLoader
        {
            get
            {
                AssetLoader assetLoader = FindObjectOfType<AssetLoader>();
                if (assetLoader == null)
                {
                    var obj = new GameObject("AssetLoader");
                    assetLoader = obj.AddComponent<AssetLoader>();
                    assetLoader.transform.parent = m_modulesRoot;
                    assetLoader.transform.localPosition = Vector3.zero;
                }
                return assetLoader;
            }
        }


        private IPluginHook m_pluginHook;

        protected virtual IPluginHook PluginHook
        {
            get
            {
                PluginHook dep = FindObjectOfType<PluginHook>();
                if (dep == null)
                {
                    var obj = new GameObject("PluginHook");
                    dep = obj.AddComponent<PluginHook>();
                    dep.transform.parent = m_modulesRoot;
                    dep.transform.localPosition = Vector3.zero;
                }
                return dep;
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
                    commonResource.transform.parent = m_modulesRoot;
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
                    fordiNetwork.transform.parent = m_modulesRoot;
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
                    webInterface.transform.parent = m_modulesRoot;
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
                    network.transform.parent = m_modulesRoot;
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
                    mouseControl.transform.parent = m_modulesRoot;
                    mouseControl.transform.localPosition = Vector3.zero;
                }
                return mouseControl;
            }
        }

        private IScreenShare m_screenShare;

        protected virtual IScreenShare ScreenShare
        {
            get
            {
                ScreenShare screenShare = FindObjectOfType<ScreenShare>();
                if (screenShare == null)
                {
                    var obj = new GameObject("ScreenShare");
                    screenShare = obj.AddComponent<ScreenShare>();
                    screenShare.transform.parent = m_modulesRoot;
                    screenShare.transform.localPosition = Vector3.zero;
                }
                return screenShare;
            }
        }

        private IVoiceChat m_voiceChat;

        protected virtual IVoiceChat VoiceChat
        {
            get
            {
                VoiceChat voiceChat = FindObjectOfType<VoiceChat>();
                if (voiceChat == null)
                {
                    throw new System.InvalidOperationException("No VoiceChat object found in the scene");
                }
                return voiceChat;
            }
        }

        //private IAnnotation m_annotation;

        //protected virtual IAnnotation Annotation
        //{
        //    get
        //    {
        //        Annotation annotation = FindObjectOfType<Annotation>();
        //        if (annotation == null)
        //        {
        //            var obj = new GameObject("Annotation");
        //            annotation = obj.AddComponent<Annotation>();
        //            annotation.transform.parent = m_modulesRoot;
        //            annotation.transform.localPosition = Vector3.zero;
        //        }
        //        return annotation;
        //    }
        //}

        private Transform m_modulesRoot = null;

        private void Awake()
        {
            if(m_instance != null)
            {
                Debug.LogWarning("AnotherInstance of ExperienceDeps exists");
            }
            m_instance = this;

            var modules = GameObject.Find("Modules");
            if (modules != null)
                m_modulesRoot = modules.transform;

            AwakeOverride();
        }

        protected virtual void AwakeOverride()
        {
            m_experienceMachine = ExperienceMachine;
            m_appTheme = AppTheme;
            m_audio = Audio;
            m_commonResource = CommonResource;
            m_settings = Settings;
            m_fordiNetwork = FordiNetwork;
            m_webInterface = WebInterface;
            m_network = Network;
            m_mouseControl = MouseControl;
            m_screenShare = ScreenShare;
            m_voiceChat = VoiceChat;
            m_videoCallEngine = VideoCallEngine;
            //m_annotation = Annotation;
            m_settings = Settings;
            m_uiEngine = UIEngine;
            m_assetLoader = AssetLoader;
            m_pluginHook = PluginHook;
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
            IOC.RegisterFallback(() => Instance.m_audio);
            IOC.RegisterFallback(() => Instance.m_commonResource);
            IOC.RegisterFallback(() => Instance.m_settings);
            IOC.RegisterFallback(() => Instance.m_fordiNetwork);
            IOC.RegisterFallback(() => Instance.m_webInterface);
            IOC.RegisterFallback(() => Instance.m_network);
            IOC.RegisterFallback(() => Instance.m_mouseControl);
            IOC.RegisterFallback(() => Instance.m_screenShare);
            IOC.RegisterFallback(() => Instance.m_voiceChat);
            IOC.RegisterFallback(() => Instance.m_assetLoader);
            IOC.RegisterFallback(() => Instance.m_pluginHook);
            //IOC.RegisterFallback(() => Instance.m_annotation);
            IOC.RegisterFallback(() => Instance.m_uiEngine);
            IOC.RegisterFallback(() => Instance.m_videoCallEngine);

            if (IOC.Resolve<IMenuSelection>() == null)
                IOC.Register<IMenuSelection>(new MenuSelection());
        }

        private static void OnSceneUnloaded(Scene arg0)
        {
            m_instance = null;
        }
    }
}

