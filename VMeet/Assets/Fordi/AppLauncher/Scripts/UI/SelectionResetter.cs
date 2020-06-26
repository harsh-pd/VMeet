using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionResetter : MonoBehaviour, IPointerClickHandler
{

    public void OnPointerClick(PointerEventData eventData)
    {
        var selectedObjects = EventSystem.current.currentSelectedGameObject;
        if (selectedObjects == null)
        {
            print("OnPointerClick: " + name + " selection null");
        }
        else
            print("OnPointerClick: " + name + " selection: " + selectedObjects.name);

        EventSystem.current.SetSelectedGameObject(null);
    }
}
