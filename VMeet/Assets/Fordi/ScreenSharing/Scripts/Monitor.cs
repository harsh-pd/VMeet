﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRExperience.Common;

namespace Fordi.ScreenSharing
{
    public class Monitor : Graphic, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private IMouseControl m_mouseControl = null;

        protected override void Awake()
        {
            base.Awake();
            m_mouseControl = IOC.Resolve<IMouseControl>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_mouseControl.ActivateMonitor(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_mouseControl.DeactivateMonitor(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            m_mouseControl.PointerClickOnMonitor(this, eventData);
        }
    }
}