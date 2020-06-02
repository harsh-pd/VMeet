using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fordi.UI.MenuControl
{
    public class Blocker : MonoBehaviour, IPointerClickHandler
    {
        public EventHandler<PointerEventData> ClickEvent { get; set; }

        public void OnPointerClick(PointerEventData eventData)
        {
            ClickEvent?.Invoke(this, eventData);
        }
    }
}
