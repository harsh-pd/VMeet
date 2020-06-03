using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using Fordi.UI.MenuControl;
using UnityEngine.EventSystems;
using Fordi.UI;
using Fordi.Common;
using Fordi.Core;

public class CalendarDateItem : MenuItem {

    protected override void OnDisableOverride()
    {
        ToggleOutlineHighlight(false);
        ToggleBackgroundHighlight(false);
        Pop(false);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

    }

    public override void DataBind(IUserInterface userInterface, MenuItemInfo item)
    {
        m_item = item;
        m_vrMenu = userInterface;

        if (m_appTheme == null)
            m_appTheme = IOC.Resolve<IAppTheme>();
        if (m_item != null)
        {
            m_icon.sprite = m_item.Icon;
            m_text.text = m_item.Text;

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
            }

            if (m_root != null)
                m_root.gameObject.SetActive(validationResult.IsVisible);
            selectable.interactable = validationResult.IsValid;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        pointerHovering = true;
        ToggleOutlineHighlight(true);
        Pop(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        pointerHovering = false;
        ToggleOutlineHighlight(false);
        ToggleBackgroundHighlight(false);
        Pop(false);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(null);
        return;
    }

    public void OnDateItemClick()
    {
        CalendarController._calendarInstance.OnDateItemClick(gameObject.GetComponentInChildren<TextMeshProUGUI>().text);
    }
}
