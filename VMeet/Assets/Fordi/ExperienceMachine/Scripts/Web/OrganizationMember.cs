using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fordi;
using VRExperience.UI.MenuControl;
using VRExperience.UI;
using VRExperience.Core;
using VRExperience.Common;
using AL.UI;

namespace VRExperience.Meeting
{
    public class OrganizationMember : ToggleInteraction, IMenuItem, IResettable
    {
        [SerializeField]
        TextMeshProUGUI nameField;
        [SerializeField]
        private Image m_icon;
        [SerializeField]
        private TextMeshProUGUI m_text;
        [SerializeField]
        private Image m_image;
        [SerializeField]
        private bool overrideColor;
        [SerializeField]
        private Color overriddenHighlight;

        private int userId;

        public string Name
        {
            get
            {
                return m_userInfo.name;
            }
        }

        public string Email
        {
            get
            {
                return m_userInfo.emailAddress;
            }
        }

        public int UserId
        {
            get
            {
                return userId;
            }
        }

        private UserInfo m_userInfo;

        protected MenuItemInfo m_item;
        public MenuItemInfo Item
        {
            get { return m_item; }
            set
            {
                if (m_item != value)
                {
                    m_item = value;
                    DataBind();
                }
            }
        }

        public GameObject Gameobject { get { return gameObject; } }

        protected IExperienceMachine m_experienceMachine = null;
        protected Toggle m_selectionToggle = null;

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

        public void Init(string _name, string _email, int _userId)
        {
            //nameField.text = _name;
            //emailField.text = _email;
            userId = _userId;
            m_selectionToggle.isOn = false;
            m_selectionToggle.interactable = true;
        }

        protected void DataBind()
        {
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

                if (!(m_item.Data is ColorResource))
                {
                    //m_item.Action.AddListener(m_experienceMachine.ExecuteMenuCommand);
                    ((Toggle)selectable).onValueChanged.AddListener((val) =>
                    {
                        MenuItemEvent<bool> action = new MenuItemEvent<bool>();
                        action.Invoke(new MenuClickArgs(m_item.Path, m_item.Text, m_item.Command, m_item.CommandType, m_item.Data), val);
                    });
                }
            }

            gameObject.SetActive(validationResult.IsVisible);
            selectable.interactable = validationResult.IsValid;

            m_userInfo = ((UserResource)m_item.Data).UserInfo;

            userId = m_userInfo.id;
            m_selectionToggle.isOn = false;
            m_selectionToggle.interactable = true;
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
