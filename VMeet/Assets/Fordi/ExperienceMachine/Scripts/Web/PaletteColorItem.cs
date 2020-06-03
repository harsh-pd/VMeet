using Fordi;
using Fordi.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fordi.Common;
using Fordi.Core;
using Fordi.Meeting;

namespace Fordi.UI.MenuControl
{
    public class PaletteColorItem : VRToggleInteraction, IMenuItem, IResettable
    {
        [SerializeField]
        private Image m_icon;

        protected MenuItemInfo m_item;
        public MenuItemInfo Item
        {
            get { return m_item; }
            //set
            //{
            //    if (m_item != value)
            //    {
            //        m_item = value;
            //        DataBind();
            //    }
            //}
        }

        public GameObject Gameobject { get { return gameObject; } }

        protected IExperienceMachine m_experienceMachine = null;
        protected IAnnotation m_annotation = null;
        protected Toggle m_selectionToggle = null;
        protected IUserInterface m_userInterface = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_selectionToggle = (Toggle)selectable;
        }

        //private void OnEnable()
        //{
        //    selectionToggle.onValueChanged.AddListener(Coordinator.instance.meetingInterface.OnMemberSelectionChange);
        //}

        //protected override void OnDisableOverride()
        //{
        //    m_selectionToggle.onValueChanged.RemoveAllListeners();
        //}

        public bool Selected
        {
            get
            {
                return m_selectionToggle.isOn;
            }
        }

        public void OnMaximumSelectionSet()
        {
            if (!m_selectionToggle.isOn)
                m_selectionToggle.interactable = false;
        }

        public void OnMaximumSelectionReset()
        {
            m_selectionToggle.interactable = true;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            ((Toggle)selectable).onValueChanged.RemoveAllListeners();
        }

        public void DataBind(IUserInterface userInterface, MenuItemInfo item)
        {
            m_userInterface = userInterface;
            m_item = item;

            if (m_item != null)
            {
                m_icon.sprite = m_item.Icon;
                m_text.text = m_item.Text;
                if (m_item.Data != null && m_item.Data is ColorResource)
                {
                    m_icon.color = ((ColorResource)m_item.Data).Color;
                    m_text.text = "";
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
            if (m_annotation == null)
                m_annotation = IOC.Resolve<IAnnotation>();

            m_item.Validate.AddListener(m_experienceMachine.CanExecuteMenuCommand);
            m_item.Validate.AddListener((args) => args.IsValid = m_item.IsValid);

            var validationResult = IsValid();
            if (validationResult.IsVisible)
            {
                if (m_item.IsValid)
                {
                    m_text.color = overrideColor ? overriddenHighlight : m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
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

                if ((m_item.Data is ColorResource))
                {
                    ((Toggle)selectable).onValueChanged.AddListener((val) =>
                    {
                        if (val)
                            m_annotation.ColorSelection(((ColorResource)m_item.Data).Color);
                    });
                }
            }

            gameObject.SetActive(validationResult.IsVisible);
            selectable.interactable = validationResult.IsValid;

            m_selectionToggle.isOn = m_annotation.SelectedColor == ((ColorResource)m_item.Data).Color;
            //if (m_allowTextScroll)
            //    StartCoroutine(InitializeTextScroll());

        }

        protected MenuItemValidationArgs IsValid()
        {
            if (m_item == null)
            {
                return new MenuItemValidationArgs(m_item.Command) { IsValid = false, IsVisible = false };
            }

            if (m_item.Validate == null)
            {
                return new MenuItemValidationArgs(m_item.Command) { IsValid = true, IsVisible = true };
            }

            MenuItemValidationArgs args = new MenuItemValidationArgs(m_item.Command);
            m_item.Validate.Invoke(args);
            return args;
        }
    }
}
