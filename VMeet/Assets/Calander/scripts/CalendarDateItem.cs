using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using VRExperience.UI.MenuControl;
using UnityEngine.EventSystems;
using VRExperience.UI;

public class CalendarDateItem : MenuItem {

    protected override void OnDisableOverride()
    {
        ToggleOutlineHighlight(false);
        ToggleBackgroundHighlight(false);
        Pop(false);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

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
