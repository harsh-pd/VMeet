using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VRExperience.Common;
using VRExperience.Core;
using AudioType = VRExperience.Core.AudioType;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

namespace VRExperience.UI.MenuControl
{
    public interface IVRMenu
    {
        bool IsOpen { get; }
        EventHandler AudioInterruptionEvent { get; set; }
        void OpenMenu(MenuItemInfo[] menuItemInfos, bool block = true, bool persist = true);
        void OpenGridMenu(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool backEnabled = true, bool block = false, bool persist = true);
        void OpenInventory(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true);
        void OpenColorInterface(ColorInterfaceArgs args);
        void OpenSettingsInterface(AudioClip clip);
        void OpenObjectInterface(AudioClip guide, MenuItemInfo[] menuItemInfos, string title, bool block = false, bool persist = true, bool backEnabled = true);
        void Popup(PopupInfo popupInfo);
        void OpenForm(FormArgs args, bool block = true, bool persist = true);
        void DisplayResult(Error error, bool freshScreen = false);
        void DisplayProgress(string text, bool freshScreen = false);
        void CloseLastScreen();
        void Close();
        void Open(IScreen screen);
        void GoBack();
        void ShowTooltip(string text);
        void ShowPreview(Sprite sprite);
        void DeactivateUI();
        void ShowUI();
        void SwitchToDesktopOnlyMode();
        void DisableDesktopOnlyMode();
    }

    public class Sound
    {
        public float Time { get; set; }
        public AudioClip Clip { get; private set; }
        public Sound(float time, AudioClip clip)
        {
            Time = time;
            Clip = clip;
        }
    }

    public class VRMenu : MonoBehaviour, IVRMenu
    {
        #region INSPECTOR_REFRENCES
        [SerializeField]
        private MenuScreen m_mainMenuPrefab, m_gridMenuPrefab, m_inventoryMenuPrefab, m_textBoxPrefab, m_formPrefab;
        [SerializeField]
        private MenuScreen m_dMainMenuPrefab, m_dGridMenuPrefab, m_dTextBoxPrefab, m_dSettingsInterace, m_dFromPrefab;
        [SerializeField]
        private ColorInterface m_colorInterfacePrefab;
        [SerializeField]
        private ObjectInterface m_objectInterfacePrefab;
        [SerializeField]
        private SettingsPanel m_settingsInterfacePrefab;
        [SerializeField]
        private Transform m_screensRoot, m_dScreenRoot;
        [SerializeField]
        private Popup m_popupPrefab;
        [SerializeField]
        private LaserPointer.LaserBeamBehavior m_laserBeamBehavior;
        [SerializeField]
        private GameObject m_sidePanelsRoot;
        [SerializeField]
        private TextMeshProUGUI m_thoughtOfTheDay, m_creditsText;
        [SerializeField]
        private GameObject m_creditsPanel;
        [SerializeField]
        private GameObject m_creditsButton, m_backCreditButton;
        [SerializeField]
        private Popup m_popup;
        [SerializeField]
        private BaseInputModule m_desktopInputModule, m_vrInputModule;
        [SerializeField]
        private MenuScreen m_desktopBlocker;
        [SerializeField]
        private GameObject m_laserPointerObject;
        #endregion

        private const string YOUTUBE_PAGE = "https://www.youtube.com/telecomatics";
        private const string WEBSITE = "http://telecomatics.com/";
        private const string FACEBOOK_PAGE = "http://telecomatics.com/";
        private const string INSTAGRAM_PAGE = "http://telecomatics.com/";

        private bool m_isMenuOpen = false;
        public bool IsMenuOpen { get { return m_isMenuOpen; } }

        public bool IsOpen { get { return m_screenStack.Count != 0; } }

        public EventHandler AudioInterruptionEvent { get; set; }

        private Stack<IScreen> m_screenStack = new Stack<IScreen>();

        private IPlayer m_player;
        private IAudio m_audio;
        private IMenuSelection m_menuSelection;
        private IExperienceMachine m_experienceMachine;
        private ICommonResource m_commonResource;
        private ISettings m_settings;

        private Vector3 m_playerScreenOffset;

        private Sound m_lastVo = null;

        private MenuScreen m_vrBlocker = null;

        private LaserPointer m_laserPointer;

        private bool m_recenterFlag = false;

        private void Awake()
        {
            m_player = IOC.Resolve<IPlayer>();
            m_playerScreenOffset = (m_player.PlayerCanvas.position - m_screensRoot.position) / m_player.PlayerCanvas.localScale.z;
            m_audio = IOC.Resolve<IAudio>();
            m_menuSelection = IOC.Resolve<IMenuSelection>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            m_commonResource = IOC.Resolve<ICommonResource>();
            m_settings = IOC.Resolve<ISettings>();
            if (m_experienceMachine.CurrentExperience == ExperienceType.HOME)
                InitializeSidePanels();

            OVRManager.HMDMounted += OnHMDMount;
            OVRManager.HMDUnmounted += OnHMDUnmount;
            //Debug.LogError("Awake");
        }

        private IEnumerator Start()
        {
            yield return null;
            if (m_laserPointer == null)
                m_laserPointer = m_laserPointerObject.GetComponent<LaserPointer>();
            if (m_laserPointer != null)
                m_laserPointer.laserBeamBehavior = m_laserBeamBehavior;

            if (XRDevice.userPresence == UserPresenceState.Present)
                OnHMDMount();
            else
                OnHMDUnmount();
            //yield return new WaitForSeconds(3.0f);
            //OVRManager.display.RecenterPose();
        }

        private void OnDestroy()
        {
            OVRManager.HMDMounted -= OnHMDUnmount;
            OVRManager.HMDUnmounted -= OnHMDMount;
        }

        #region CORE
        private void BringInFront(Transform menuTransform)
        {
            Vector3 offset = menuTransform.localPosition / 100.0f;
            menuTransform.transform.localPosition = Vector3.zero;
            menuTransform.transform.localRotation = Quaternion.identity;
            menuTransform.transform.localPosition = menuTransform.transform.localPosition - m_playerScreenOffset;
            menuTransform.transform.SetParent(m_screensRoot);
            menuTransform.position = menuTransform.position + menuTransform.forward * offset.z + new Vector3(0, offset.y, 0);
            m_player.RequestHaltMovement(true);
        }

        public void OpenMenu(MenuItemInfo[] items, bool block = true, bool persist = false)
        {
            if (m_experienceMachine.CurrentExperience != ExperienceType.HOME && !m_recenterFlag)
            {
                Debug.LogError("Recentering");
                UnityEngine.XR.InputTracking.Recenter();
                m_recenterFlag = true;
            }
            //Debug.LogError("OpenMenu");
            m_screensRoot.gameObject.SetActive(true);
            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                if (screen.Persist)
                    screen.Deactivate();
                else
                    m_screenStack.Pop().Close();
            }

            if (m_player == null)
                m_player = IOC.Resolve<IPlayer>();

            m_player.PrepareForSpawn();
            var menu = Instantiate(m_mainMenuPrefab, m_player.PlayerCanvas);
            BringInFront(menu.transform);

            menu.OpenMenu(items, block, persist);
            m_screenStack.Push(menu);
            m_menuOn = true;

            if (m_settings.SelectedPreferences.DesktopMode)
                menu.Hide();

            var dMenu = Instantiate(m_dMainMenuPrefab, m_dScreenRoot);
            dMenu.OpenMenu(items, block, persist);
            menu.Pair = dMenu;
        }

        public void OpenGridMenu(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            PlayGuide(guide);

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                if (screen.Persist)
                    screen.Deactivate();
                else
                    m_screenStack.Pop().Close();
            }

            m_player.PrepareForSpawn();
            var menu = Instantiate(m_gridMenuPrefab, m_player.PlayerCanvas);
            BringInFront(menu.transform);
            menu.OpenGridMenu(items, title, block, persist, backEnabled);
            m_screenStack.Push(menu);
            if (m_settings.SelectedPreferences.DesktopMode)
                menu.Hide();

            if (items != null && items.Length > 0 && items[0].Data.GetType() == typeof(ObjectGroup))
                m_inventoryOpen = true;

            var dMenu = Instantiate(m_dGridMenuPrefab, m_dScreenRoot);
            dMenu.OpenGridMenu(items, title, block, persist, backEnabled);
            menu.Pair = dMenu;
        }

        public void OpenInventory(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            PlayGuide(guide);

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                if (screen.Persist)
                    screen.Deactivate();
                else
                    m_screenStack.Pop().Close();
            }

            m_player.PrepareForSpawn();
            var menu = Instantiate(m_inventoryMenuPrefab, m_player.PlayerCanvas);
            BringInFront(menu.transform);
            menu.OpenGridMenu(items, title, block, persist, backEnabled);
            m_screenStack.Push(menu);
            if (m_settings.SelectedPreferences.DesktopMode)
                menu.Hide();

            if (items != null && items.Length > 0 && items[0].Data.GetType() == typeof(ObjectGroup))
                m_inventoryOpen = true;
        }


        public void OpenObjectInterface(AudioClip guide, MenuItemInfo[] items, string title, bool backEnabled = true, bool block = false, bool persist = true)
        {
            PlayGuide(guide);

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                if (screen.Persist)
                    screen.Deactivate();
                else
                    m_screenStack.Pop().Close();
            }

            m_player.PrepareForSpawn();
            var menu = Instantiate(m_objectInterfacePrefab, m_player.PlayerCanvas);
            BringInFront(menu.transform);
            menu.OpenGridMenu(items, title, block, persist, backEnabled);
            m_screenStack.Push(menu);
            if (m_settings.SelectedPreferences.DesktopMode)
                menu.Hide();
        }

        public void Popup(PopupInfo popupInfo)
        {
            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                if (screen.Persist)
                    screen.Deactivate();
                else
                    m_screenStack.Pop().Close();
            }

            var popup = Instantiate(m_popupPrefab, m_screensRoot);
            popup.Show(popupInfo, null);
            m_screenStack.Push(popup);
            if (m_settings.SelectedPreferences.DesktopMode)
                popup.Hide();
        }

        public void CloseLastScreen()
        {
            //Debug.LogError("Close last screen");

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Pop();
                screen.Close();
            }

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                screen.Reopen();
                if (!(screen is IForm))
                    RefreshDesktopMode();
                //Debug.LogError("opening: " + screen.Gameobject.name);
            }
            else
                m_screensRoot.gameObject.SetActive(false);
        }

        public void Open(IScreen screen)
        {
            if (m_screenStack.Count > 0)
            {
                var lstScreen = m_screenStack.Peek();
                if (screen.Persist)
                    screen.Deactivate();
                else
                    m_screenStack.Pop().Close();
            }

            screen.Gameobject.transform.SetParent(m_screensRoot);
            m_screenStack.Push(screen);
        }

        public void Close()
        {
            foreach (var item in m_screenStack)
                item.Close();
            if (m_menuSelection == null)
                m_menuSelection = IOC.Resolve<IMenuSelection>();
            
            m_screenStack.Clear();
            m_menuOff = true;
            m_player.RequestHaltMovement(false);

            if (m_menuSelection.ExperienceType == ExperienceType.MANDALA && m_experienceMachine.CurrentExperience == ExperienceType.MANDALA && m_menuSelection.VoiceOver == null)
            {
                m_experienceMachine.GetExperience(ExperienceType.MANDALA).ResumeGuide();
                return;
            }

            if (!Experience.AudioSelectionFlag && m_lastVo != null)
            {
                AudioSource audioSource = m_audio.GetAudioSource(AudioType.VO);
                
                if (audioSource.isPlaying)
                {
                    float lastTime = m_lastVo.Time;
                    AudioArgs args = new AudioArgs(null, AudioType.VO)
                    {
                        FadeTime = .5f,
                        Done = () =>
                        {
                            AudioArgs voArgs = new AudioArgs(m_menuSelection.VoiceOver, AudioType.VO);
                            voArgs.FadeTime = 2;
                            voArgs.ResumeTime = lastTime;
                            m_audio.Resume(voArgs);
                        }
                    };
                    m_audio.Stop(args);
                    m_lastVo = null;
                }
                else
                {
                    AudioArgs voArgs = new AudioArgs(m_menuSelection.VoiceOver, AudioType.VO);
                    voArgs.FadeTime = 2;
                    voArgs.ResumeTime = m_lastVo.Time;
                    m_audio.Resume(voArgs);
                    m_lastVo = null;
                }
            }
            if (m_experienceMachine.CurrentExperience == ExperienceType.HOME)
                m_screensRoot.gameObject.SetActive(false);

            RefreshDesktopMode();
        }

        public void GoBack()
        {
            if (m_screenStack.Count > 1)
            {
                m_screenStack.Pop().Close();
                m_screenStack.Peek().Reopen();
            }
        }

        public void ShowTooltip(string text)
        {
            if (m_screenStack.Count > 0)
            {
                m_screenStack.Peek().ShowTooltip(text);
            }
        }

        public void ShowPreview(Sprite sprite)
        {
            if (m_screenStack.Count > 0)
            {
                m_screenStack.Peek().ShowPreview(sprite);
            }
        }

        public void OpenColorInterface(ColorInterfaceArgs args)
        {
            PlayGuide(args.GuideClip);

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                if (screen.Persist)
                    screen.Deactivate();
                else
                    m_screenStack.Pop().Close();
            }

            m_player.PrepareForSpawn();
            var menu = Instantiate(m_colorInterfacePrefab, m_player.PlayerCanvas);
            BringInFront(menu.transform);
            menu.OpenColorInterface(args);
            m_screenStack.Push(menu);
        }
        
        public void OpenSettingsInterface(AudioClip clip)
        {
            OpenInterface(m_settingsInterfacePrefab);
            PlayGuide(clip);
        }

        private void PlayGuide(AudioClip clip)
        {
            if (m_menuSelection == null)
                m_menuSelection = IOC.Resolve<IMenuSelection>();

            var voSource = m_audio.GetAudioSource(AudioType.VO);
            if (voSource.clip == m_menuSelection.VoiceOver && voSource.isPlaying && clip != null)
                m_lastVo = new Sound(voSource.time, m_menuSelection.VoiceOver);
            else if (voSource.clip == m_menuSelection.VoiceOver)
                m_lastVo = null;

            if (m_player.GuideOn)
                return;

            if (clip != null)
            {
                AudioInterruptionEvent?.Invoke(this, EventArgs.Empty);
                AudioArgs audioArgs = new AudioArgs(clip, AudioType.VO)
                {
                    FadeTime = 0,
                    Done = null
                };
                m_audio.Play(audioArgs);
            }
        }

        private void OpenInterface(MenuScreen screenPrefab, bool block = true, bool persist = false)
        {
            m_screensRoot.gameObject.SetActive(true);
            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                if (screen.Persist)
                    screen.Deactivate();
                else
                    m_screenStack.Pop().Close();
            }

            m_player.PrepareForSpawn();
            var menu = Instantiate(screenPrefab, m_player.PlayerCanvas);
            BringInFront(menu.transform);

            menu.Init(block, persist);
            m_screenStack.Push(menu);
            if (m_settings.SelectedPreferences.DesktopMode)
                menu.Hide();

            var dMenu = Instantiate(m_dSettingsInterace, m_dScreenRoot);
            dMenu.Init(block, persist);
            menu.Pair = dMenu;
        }

        public void DisplayMessage(string message, bool block = true, bool persist = false)
        {
            if (m_vrBlocker != null)
                m_vrBlocker.Close();

            m_screensRoot.gameObject.SetActive(true);
            //if (m_screenStack.Count > 0)
            //{
            //    var screen = m_screenStack.Peek();
            //    if (screen.Persist)
            //        screen.Deactivate();
            //    else
            //        m_screenStack.Pop().Close();
            //}

            if (m_player == null)
                m_player = IOC.Resolve<IPlayer>();

            m_player.PrepareForSpawn();
            var menu = Instantiate(m_textBoxPrefab, m_player.PlayerCanvas);
            BringInFront(menu.transform);

            menu.OpenMenu(message, block, persist);
            m_vrBlocker = menu;
            //m_screenStack.Push(menu);
            m_menuOn = true;
        }

        public void CloseVRBlocker()
        {
            if (m_vrBlocker != null)
                m_vrBlocker.Close();
            if (m_screenStack.Count == 0)
                m_menuOff = true;
        }

        public void BlockDesktop()
        {
            if (m_desktopBlocker != null)
                m_desktopBlocker.Reopen();
        }

        public void UnblockDesktop()
        {
            if (m_desktopBlocker != null)
                m_desktopBlocker.Deactivate();
        }
        #endregion

        #region GUIDE_CONDITIONS
        private bool m_menuOn = false, m_menuOff = false, m_inventoryOpen = false;

        public bool MenuOn()
        {
            var val = m_menuOn;
            if (m_menuOn)
                m_menuOn = false;
            return val;
        }

        public bool MenuOff()
        {
            var val = m_menuOff;
            if (m_menuOff)
                m_menuOff = false;
            return val;
        }

        public bool InventoryOpen()
        {
            var val = m_inventoryOpen;
            if (m_inventoryOpen)
                m_inventoryOpen = false;
            return val;
        }
        #endregion

        public void DeactivateUI()
        {
            if (m_screenStack.Count == 0)
                return;
            m_screenStack.Peek().Deactivate();
        }

        public void ShowUI()
        {
            if (m_screenStack.Count == 0)
                return;
            m_screenStack.Peek().Reopen();
        }

        public void ToggleSidePanels()
        {
            m_sidePanelsRoot.SetActive(!m_sidePanelsRoot.activeSelf);
        }

        public void ToggleCredits()
        {
            if (!m_creditsPanel.activeSelf)
            {
                if (m_screenStack.Count > 0)
                    m_screenStack.Peek().Deactivate();
            }
            else
            {
                if (m_screenStack.Count > 0)
                    m_screenStack.Peek().Reopen();
            }
            m_creditsPanel.SetActive(!m_creditsPanel.activeSelf);
            m_backCreditButton.SetActive(m_creditsPanel.activeSelf);
            m_creditsButton.SetActive(!m_creditsPanel.activeSelf);
        }

        private void InitializeSidePanels()
        {
            var thoughts = m_commonResource.AssetDb.Thoughts;
            if (thoughts.Length > 0)
                m_thoughtOfTheDay.text = thoughts[UnityEngine.Random.Range(0, thoughts.Length)];
            //m_creditsText.text = m_commonResource.AssetDb.Credits;
        }

        #region SOCIAL_MEDIA
        public void OpenYoutube()
        {
            OpenLinkPopup(YOUTUBE_PAGE);
        }


        public void OpenWebsite()
        {
            OpenLinkPopup(WEBSITE);
        }

        public void OpenFacebookPage()
        {
            OpenLinkPopup(FACEBOOK_PAGE);
        }

        public void OpenInstagramPage()
        {
            OpenLinkPopup(INSTAGRAM_PAGE);
        }

        private void OpenLinkPopup(string link)
        {
            var popup = Instantiate(m_popup, m_screensRoot);
            popup.transform.SetParent(m_screensRoot.parent);
            var popupInfo = new PopupInfo
            {
                Content = "Open <#FF004D>" + link + "</color> in browser?"
            };
            popup.Show(popupInfo, () => System.Diagnostics.Process.Start(link));
        }
        #endregion

        #region DESKTOP_VR_COORDINATION
        private void EnsureModuleIntegrity()
        {
            if (gameObject == null)
            {
                Debug.LogError("Gameobject destroyed but still event called");
            }
            if (m_desktopInputModule == null)
                m_desktopInputModule = FindObjectOfType<StandaloneInputModule>();
            if (m_vrInputModule == null)
                m_vrInputModule = FindObjectOfType<FordiInputModule>();
            if (m_laserPointer == null)
                m_laserPointer = m_laserPointerObject.GetComponent<LaserPointer>();
        }

        public void SwitchToDesktopOnlyMode()
        {
            Debug.LogError("Switch to donly");
            foreach (var item in m_screenStack)
                item.Hide();

            DisplayMessage("Desktop only mode is active.");
            UnblockDesktop();
            m_sidePanelsRoot.SetActive(false);
        }

        public void RefreshDesktopMode()
        {
            m_settings.SelectedPreferences.ForcedDesktopMode = false;
            m_settings.SyncSettingsWithDisk(() =>
            {
                if (m_settings.SelectedPreferences.DesktopMode)
                    SwitchToDesktopOnlyMode();
                else
                    DisableDesktopOnlyMode();
            });
        }

        public void DisableDesktopOnlyMode()
        {
            m_settings.SelectedPreferences.ForcedDesktopMode = false;
            m_settings.SelectedPreferences.DesktopMode = false;
            foreach (var item in m_screenStack)
                item.UnHide();

            CloseVRBlocker();
            if (m_screenStack.Count > 0)
                m_screenStack.Peek().Reopen();
        }

        void OnHMDUnmount()
        {
            if (!m_settings.SelectedPreferences.DesktopMode)
                UnblockDesktop();
            EnableDesktopModule();
        }

        void OnHMDMount()
        {
            if (!m_settings.SelectedPreferences.DesktopMode)
                BlockDesktop();
            EnableVRModule();
        }

        private void EnableDesktopModule()
        {
            if (IOC.Resolve<IVRMenu>() != this)
            {
                OVRManager.HMDMounted -= this.OnHMDMount;
                OVRManager.HMDUnmounted -= this.OnHMDUnmount;
                return;
            }
            EnsureModuleIntegrity();
            m_vrInputModule.enabled = false;
            m_desktopInputModule.enabled = true;
            m_laserPointer.gameObject.SetActive(false);
        }

        private void EnableVRModule()
        {
            if (IOC.Resolve<IVRMenu>() != this)
            {
                OVRManager.HMDMounted -= this.OnHMDMount;
                OVRManager.HMDUnmounted -= this.OnHMDUnmount;
                return;
            }
            EnsureModuleIntegrity();
            m_desktopInputModule.enabled = false;
            m_vrInputModule.enabled = true;
            m_laserPointer.gameObject.SetActive(true);
        }

        public void OpenForm(FormArgs args, bool block = true, bool persist = true)
        {
            m_screensRoot.gameObject.SetActive(true);
            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                if (screen.Persist)
                    screen.Deactivate();
                else
                    m_screenStack.Pop().Close();
            }

            if (m_player == null)
                m_player = IOC.Resolve<IPlayer>();

            m_player.PrepareForSpawn();
            var menu = Instantiate(m_formPrefab, m_player.PlayerCanvas);
            if (!(menu is Form))
                throw new InvalidOperationException();
            
            BringInFront(menu.transform);

            ((Form)menu).OpenForm(args, block, persist);
            m_screenStack.Push(menu);
            m_menuOn = true;

            if (m_settings.SelectedPreferences.DesktopMode)
                menu.Hide();

            var dMenu = Instantiate(m_dFromPrefab, m_dScreenRoot);
            if (!(dMenu is Form))
                throw new InvalidOperationException();
            ((Form)dMenu).OpenForm(args, block, persist);
            menu.Pair = dMenu;

            m_settings.SelectedPreferences.DesktopMode = true;
            m_settings.SelectedPreferences.ForcedDesktopMode = true;
            SwitchToDesktopOnlyMode();
        }

        public void DisplayResult(Error error, bool freshScreen = false)
        {
            if (!freshScreen && m_screenStack.Count > 0 && m_screenStack.Peek() is IForm form)
            {
                form.DisplayResult(error);
            }
        }

        public void DisplayProgress(string text, bool freshScreen = false)
        {
            if (!freshScreen && m_screenStack.Count > 0 && m_screenStack.Peek() is IForm form)
            {
                form.DisplayProgress(text);
            }
        }
        #endregion
    }
}
