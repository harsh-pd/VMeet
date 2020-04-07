using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.Common;
using VRExperience.Core;

namespace VRExperience.UI.MenuControl
{
    public class FormItem : MenuItem
    {
        public override void ToggleOutlineHighlight(bool val) { }

        public override void OnReset() { }

        protected override void DataBind()
        {
            if (m_item != null)
            {
                m_icon.sprite = m_item.Icon;
                if (m_item.Data != null && m_item.Data is ColorResource)
                {
                    m_icon.color = ((ColorResource)m_item.Data).Color;
                    m_text.text = ((ColorResource)m_item.Data).ShortDescription.ToUpper();
                    if (m_icon.color.grayscale > .5f)
                        m_text.color = Color.white;
                    else
                        m_text.color = Color.black;
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
                if (m_image != null)
                {
                    if (m_item.IsValid)
                    {
                        m_image.color = m_appTheme.SelectedTheme.buttonNormalTextColor;
                    }
                    else
                    {
                        m_image.color = m_appTheme.SelectedTheme.buttonDisabledTextColor;
                    }
                }

                if (m_item.Data is TMP_InputField.ContentType)
                {
                    TMP_InputField inputField = (TMP_InputField)selectable;
                    if (inputField != null)
                    {
                        inputField.contentType = (TMP_InputField.ContentType)m_item.Data;
                        var placeHolderText = inputField.placeholder.GetComponent<TextMeshProUGUI>();
                        if (placeHolderText)
                            placeHolderText.text = m_item.Text;
                    }

                }

                if (!(m_item.Data is TMPro.TMP_InputField.ContentType))
                {
                    m_item.Action = new MenuItemEvent();
                    m_item.Action.AddListener(m_experienceMachine.ExecuteMenuCommand);
                    ((Button)selectable).onClick.AddListener(() => m_item.Action.Invoke(new MenuClickArgs(m_item.Path, m_item.Text, m_item.Command, m_item.CommandType, m_item.Data)));
                }
            }

            gameObject.SetActive(validationResult.IsVisible);
            selectable.interactable = validationResult.IsValid;
        }

        public override void DataBind(MenuItemInfo item, object sender)
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
                    if (m_icon.color.grayscale < .5f)
                        m_text.color = Color.white;
                    else
                        m_text.color = Color.black;
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
                if (m_image != null)
                {
                    if (m_item.IsValid)
                    {
                        m_image.color = m_appTheme.SelectedTheme.buttonNormalTextColor;
                    }
                    else
                    {
                        m_image.color = m_appTheme.SelectedTheme.buttonDisabledTextColor;
                    }
                }

                m_item.Action = new MenuItemEvent();

                if (!(m_item.Data is ColorResource))
                    m_item.Action.AddListener(m_experienceMachine.ExecuteMenuCommand);
                else if (sender is ColorPicker colorPicker)
                    m_item.Action.AddListener(colorPicker.ClickColor);

                ((Button)selectable).onClick.AddListener(() => m_item.Action.Invoke(new MenuClickArgs(m_item.Path, m_item.Text, m_item.Command, m_item.CommandType, m_item.Data)));

            }

            gameObject.SetActive(validationResult.IsVisible);
            selectable.interactable = validationResult.IsValid;
        }
    }
}