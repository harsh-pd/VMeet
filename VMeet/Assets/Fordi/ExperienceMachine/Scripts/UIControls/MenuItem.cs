using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fordi.Common;
using Fordi.Core;
using AudioType = Fordi.Core.AudioType;

namespace Fordi.UI.MenuControl
{
    public delegate void MenuItemEventHandler(MenuItem menuItem);

    public interface IMenuItem
    {
        MenuItemInfo Item { get; }
        Selectable Selectable { get; }
        void DataBind(IUserInterface userInterface, MenuItemInfo item);
    }

    public class MenuItem : VRButtonInteraction, IMenuItem
    {
        [SerializeField]
        protected Image m_icon = null;
        [SerializeField]
        protected bool m_allowTextScroll = false;
        [SerializeField]
        private float m_textScrollSpeed = 10.0f;

        protected TextMeshProUGUI m_clonedText;

        private bool m_textScrollInitialized = false;

        protected IUserInterface m_vrMenu;
        protected IUIEngine m_uiEngine;

        protected IMenuSelection m_menuSelection;

        protected IEnumerator m_textScrollEnumerator;

        protected Vector3 m_initialTextPosition = Vector3.zero;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_vrMenu = IOC.Resolve<IUserInterface>();
            m_menuSelection = IOC.Resolve<IMenuSelection>();
            m_uiEngine = IOC.Resolve<IUIEngine>();
        }

        protected MenuItemInfo m_item;
        public MenuItemInfo Item
        {
            get { return m_item; }
            //set
            //{
            //    if(m_item != value)
            //    {
            //        m_item = value;
            //        DataBind();
            //    }
            //}
        }

        public virtual void DataBind(IUserInterface userInterface, MenuItemInfo item)
        {
            m_item = item;
            m_vrMenu = userInterface;

            if(m_item != null)
            {
                m_icon.sprite = m_item.Icon;
                m_text.text = m_item.Text;
                if (m_item.Data != null && m_item.Data is ColorResource)
                {
                    m_icon.color = ((ColorResource)m_item.Data).Color;
                    m_text.text = ((ColorResource)m_item.Data).ShortDescription.ToUpper();
                }

                m_icon.gameObject.SetActive(m_item.Icon != null || (m_item.Data != null && m_item.Data is ColorResource));
            }
            else
            {
                m_icon.sprite = null;
                m_icon.gameObject.SetActive(false);
                m_text.text = string.Empty;
            }

            if (m_item.Validate == null)
                m_item.Validate = new MenuItemValidationEvent();

            if (m_experienceMachine == null)
                m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            if (m_appTheme == null)
                m_appTheme = IOC.Resolve<IAppTheme>();

            m_item.Validate.AddListener(m_experienceMachine.CanExecuteMenuCommand);
            m_item.Validate.AddListener((args) => args.IsValid = m_item.IsValid);

            var validationResult = IsValid();
            if(validationResult.IsVisible)
            {
                if (m_item.IsValid)
                {
                    m_text.color = overrideColor? overriddenHighlight : m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
                }
                else
                {
                    m_text.color = m_appTheme.GetSelectedTheme(m_platform).buttonDisabledTextColor;
                }

                if (m_image != null)
                {
                    if (m_item.IsValid)
                    {
                        m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
                    }
                    else
                    {
                        m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonDisabledTextColor;
                    }
                }

                if (!(m_item.Data is ColorResource))
                {
                    m_item.Action = new MenuItemEvent();
                    m_item.Action.AddListener(m_experienceMachine.ExecuteMenuCommand);
                    ((Button)selectable).onClick.AddListener(() => m_item.Action.Invoke(new MenuClickArgs(m_item.Path, m_item.Text, m_item.Command, m_item.CommandType, m_item.Data)));
                }
            }

            if (m_root != null)
                m_root.gameObject.SetActive(validationResult.IsVisible);
            selectable.interactable = validationResult.IsValid;
            if (m_allowTextScroll)
                StartCoroutine(InitializeTextScroll());
        }

        protected bool m_textOverflowing = false;

        protected IEnumerator InitializeTextScroll()
        {
            yield return null;

            if (m_text.preferredWidth > ((RectTransform)m_text.transform.parent).rect.width + 1)
            {
                m_textOverflowing = true;
                m_text.rectTransform.anchorMin = new Vector2(0, .5f);
                m_text.rectTransform.anchorMax = new Vector2(0, .5f);
                m_text.rectTransform.pivot = new Vector2(0, .5f);
                m_text.rectTransform.offsetMin = new Vector2(0, m_text.rectTransform.offsetMin.y);
                m_initialTextPosition = ((RectTransform)m_text.transform).localPosition;
                m_text.alignment = TextAlignmentOptions.Left;

                m_clonedText = Instantiate(m_text) as TextMeshProUGUI;
                RectTransform cloneRectTransform = m_clonedText.GetComponent<RectTransform>();
                cloneRectTransform.SetParent(m_text.rectTransform);
                cloneRectTransform.anchorMin = new Vector2(1, 0.5f);
                cloneRectTransform.localPosition = new Vector3(m_text.preferredWidth + 2, 0, cloneRectTransform.position.z);
                cloneRectTransform.localScale = new Vector3(1, 1, 1);
                m_clonedText.text = m_text.text;
                m_clonedText.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                m_clonedText.gameObject.SetActive(false);

                if (pointerHovering && m_allowTextScroll)
                {
                    if (m_textScrollEnumerator != null)
                        StopCoroutine(m_textScrollEnumerator);
                    m_textScrollEnumerator = TextScroll();
                    StartCoroutine(m_textScrollEnumerator);
                }
            }
            else
            {
                m_text.alignment = TextAlignmentOptions.Center;
            }

            m_textScrollInitialized = true;
        }

        public virtual void DataBind(MenuItemInfo item, object sender)
        {
            m_item = item;

            if (m_item != null)
            {
                m_icon.sprite = m_item.Icon;
                m_text.text = m_item.Text;
                if (m_item.Data != null && m_item.Data is ColorResource)
                {
                    m_icon.color = ((ColorResource)m_item.Data).Color;
                    m_text.text = ((ColorResource)m_item.Data).ShortDescription.ToUpper();
                }
                m_icon.gameObject.SetActive(m_item.Icon != null || (m_item.Data != null && m_item.Data is ColorResource));
            }
            else
            {
                m_icon.sprite = null;
                m_icon.gameObject.SetActive(false);
                m_text.text = string.Empty;
            }

            m_item.Validate = new MenuItemValidationEvent();

            if (m_experienceMachine == null)
                m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            if (m_appTheme == null)
                m_appTheme = IOC.Resolve<IAppTheme>();

            m_item.Validate.AddListener(m_experienceMachine.CanExecuteMenuCommand);
            m_item.Validate.AddListener((args) => args.IsValid = m_item.IsValid);

            var validationResult = IsValid();
            if (validationResult.IsVisible)
            {
                if (m_item.IsValid)
                {
                    m_text.color = overrideColor? overriddenHighlight : m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
                }
                else
                {
                    m_text.color = m_appTheme.GetSelectedTheme(m_platform).buttonDisabledTextColor;
                }

                if (m_image != null)
                {
                    if (m_item.IsValid)
                    {
                        m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
                    }
                    else
                    {
                        m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonDisabledTextColor;
                    }
                }

                m_item.Action = new MenuItemEvent();

                if (!(m_item.Data is ColorResource))
                    m_item.Action.AddListener(m_experienceMachine.ExecuteMenuCommand);

                ((Button)selectable).onClick.AddListener(() => m_item.Action.Invoke(new MenuClickArgs(m_item.Path, m_item.Text, m_item.Command, m_item.CommandType, m_item.Data)));
               
            }

            gameObject.SetActive(validationResult.IsVisible);
            selectable.interactable = validationResult.IsValid;
            if (m_allowTextScroll)
                StartCoroutine(InitializeTextScroll());
        }

        protected MenuItemValidationArgs IsValid()
        {
            if(m_item == null)
            {
                return new MenuItemValidationArgs(m_item.Command) { IsValid = false, IsVisible = false };
            }

            if(m_item.Validate == null)
            {
                return new MenuItemValidationArgs(m_item.Command) { IsValid = true, IsVisible = true };
            }

            MenuItemValidationArgs args = new MenuItemValidationArgs(m_item.Command);
            m_item.Validate.Invoke(args);
            return args;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (Item.Data is ResourceComponent experienceResource)
            {
                m_vrMenu.ShowTooltip(experienceResource.Description);
                if (!(experienceResource is ColorResource))
                    m_vrMenu.ShowPreview(experienceResource.LargePreview);

                if (Item.Data is AudioResource)
                    PreviewSound();
            }

            if (m_allowTextScroll && m_textOverflowing && !string.IsNullOrEmpty(m_text.text))
            {
                if (m_textScrollEnumerator != null)
                    StopCoroutine(m_textScrollEnumerator);
                m_textScrollEnumerator = TextScroll();
                StartCoroutine(m_textScrollEnumerator);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (Item.Data is ResourceComponent experienceResource)
            {
                m_vrMenu.ShowTooltip("");
                m_vrMenu.ShowPreview(null);
            }

            if (Item.Data is AudioResource)
                StopSoundPreview();

            if (m_textScrollEnumerator != null)
            {
                StopCoroutine(m_textScrollEnumerator);
                ((RectTransform)m_text.transform).localPosition = m_initialTextPosition;
                if (m_clonedText != null)
                    m_clonedText.gameObject.SetActive(false);
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (Item.Data is AudioResource audioResource && m_experienceMachine.CurrentExperience == ExperienceType.HOME && audioResource.ResourceType == ResourceType.AUDIO)
                StopSoundPreview();
        }

        public void SetGraphic(Image image)
        {
            ((Button)selectable).targetGraphic = image;
        }

        protected override void OnEnableOverride()
        {
            base.OnEnableOverride();
            m_uiEngine.ScreenChangeInitiated += ScreenChangeInitiated;
        }

        protected override void OnDisableOverride()
        {
            base.OnDisableOverride();

            m_uiEngine.ScreenChangeInitiated -= ScreenChangeInitiated;

            if (m_textScrollEnumerator != null)
            {
                StopCoroutine(m_textScrollEnumerator);
                ((RectTransform)m_text.transform).localPosition = m_initialTextPosition;
                if (m_clonedText != null)
                    m_clonedText.gameObject.SetActive(false);
            }
        }

        private void ScreenChangeInitiated(object sender, EventArgs e)
        {
            if (Item != null && Item.Data != null && Item.Data is ResourceComponent experienceResource)
            {
                m_vrMenu.ShowPreview(null);
                m_vrMenu.ShowTooltip("");
            }
        }

        private Sound m_lastVo;

        protected void PreviewSound()
        {
            if (m_menuSelection == null)
                m_menuSelection = IOC.Resolve<IMenuSelection>();

            AudioResource audioResource = (AudioResource)Item.Data;
            
            AudioType audioType;
            if (audioResource.ResourceType == ResourceType.AUDIO)
                audioType = AudioType.VO;
            else
                audioType = AudioType.MUSIC;

            var audioSource = m_audio.GetAudioSource(audioType);

            if (audioType == AudioType.VO && audioSource.clip == m_menuSelection.VoiceOver && audioSource.isPlaying)
            {
                if (m_lastVo == null || m_lastVo.Clip != m_menuSelection.VoiceOver)
                    m_lastVo = new Sound(audioSource.time, m_menuSelection.VoiceOver);
            }
            else if (audioType == AudioType.VO && audioSource.clip == m_menuSelection.VoiceOver)
            {
                m_lastVo = null;
            }
            if (audioType == AudioType.MUSIC && audioSource.clip == m_menuSelection.Music && audioSource.isPlaying)
            {
                m_lastVo = new Sound(audioSource.time, m_menuSelection.Music);
            }
            else if (audioType == AudioType.MUSIC && audioSource.clip == m_menuSelection.Music)
            {
                m_lastVo = null;
            }

            if (audioSource.isPlaying)
            {
                AudioArgs audioArgs = new AudioArgs
                {
                    FadeTime = .5f,
                    AudioType = audioType,
                    Done = () =>
                    {
                        audioSource.time = 0;
                        AudioClip clip = audioResource.Clip;
                        AudioArgs args = new AudioArgs(clip, audioType)
                        {
                            FadeTime = .5f
                        };
                        m_audio.Play(args);
                    }
                };

                m_audio.Stop(audioArgs);
            }
            else
            {
                audioSource.time = 0;
                AudioClip clip = audioResource.Clip;
                AudioArgs args = new AudioArgs(clip, audioType)
                {
                    FadeTime = 1
                };
                m_audio.Play(args);
            }
        }

        private void StopSoundPreview()
        {
            //Debug.LogError("Stop Preview");
            AudioResource audioResource = (AudioResource)Item.Data;

            AudioType audioType;
            if (audioResource.ResourceType == ResourceType.AUDIO)
                audioType = AudioType.VO;
            else
                audioType = AudioType.MUSIC;

            var audioSource = m_audio.GetAudioSource(audioType);
            if (audioSource.isPlaying)
            {
                AudioArgs audioArgs = new AudioArgs
                {
                    FadeTime = .5f,
                    AudioType = audioType,
                    Done = () =>
                    {
                        if ((audioType == AudioType.VO && !m_vrMenu.IsOpen && m_lastVo != null) || (audioType == AudioType.MUSIC && m_lastVo != null))
                        {
                            //Debug.LogError("LastVO: " + m_lastVo.Clip.name);
                            AudioArgs args = new AudioArgs(m_lastVo.Clip, audioType)
                            {
                                FadeTime = .5f,
                                ResumeTime = m_lastVo.Time
                            };
                            m_audio.Resume(args);
                        }

                    }
                };

                m_audio.Stop(audioArgs);
            }
            else if (m_lastVo != null)
            {
                AudioArgs args = new AudioArgs(m_lastVo.Clip, audioType)
                {
                    FadeTime = 1f,
                    ResumeTime = m_lastVo.Time,
                };
                m_audio.Resume(args);
            }
        }

        #region TEXT_SCROLL
        protected IEnumerator TextScroll()
        {
            yield return new WaitUntil(() => m_textScrollInitialized);

            if (m_clonedText == null)
                yield break;

            //if (m_clonedText != null)
            //    m_clonedText.gameObject.SetActive(true);

            float width = m_text.preferredWidth;
            Vector3 startPosition = m_text.rectTransform.localPosition;

            float scrollPosition = 0;

            while (true)
            {
                m_text.rectTransform.localPosition = new Vector3(-scrollPosition % width, startPosition.y, startPosition.z);
                scrollPosition += m_textScrollSpeed * Time.deltaTime;
                yield return null;
            }
        }
        #endregion
    }
}

