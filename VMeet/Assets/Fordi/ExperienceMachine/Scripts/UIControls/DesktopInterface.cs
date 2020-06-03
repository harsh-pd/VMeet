using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Fordi.Common;
using Fordi.Core;
using AudioType = Fordi.Core.AudioType;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fordi.Meeting;
using Fordi.Meetings.UI;
using Fordi.ScreenSharing;

namespace Fordi.UI.MenuControl
{
    public class DesktopInterface : UserInterface
    {
        #region INSPECTOR_REFRENCES
        [SerializeField]
        private StandaloneMenu m_standaloneMenuPrefab;
        #endregion


        private StandaloneMenu m_standAloneMenu = null;
        private MenuScreen m_permanentDesktopScreen = null;

        #region CORE
        public void LoadRemoteDesktopView(MenuArgs args)
        {
            throw new NotImplementedException();
            //m_screensRoot.gameObject.SetActive(true);
            //var dMenu = Instantiate(m_dRemoteMonitorPrefab, m_dScreenRoot);
            //dMenu.OpenMenu(items, block, persist);
            //m_permanentDesktopScreen = dMenu;
        }

        public override IScreen OpenCalendar(CalendarArgs args)
        {
            m_screensRoot.gameObject.SetActive(true);
            var menu = Instantiate(m_calendarPrefab, m_screensRoot);
            m_screenStack.Push(menu);
            menu.OpenCalendar(this, args);
            return menu;
        }

        public override IScreen OpenAnnotationInterface(GridArgs args)
        {
            return DisplayMessage("Annotation interface activated in VR.", true);
        }
        #endregion

        public override IScreen DisplayProgress(string text, bool freshScreen = false)
        {
            return base.DisplayProgress(text.Style(ExperienceMachine.ProgressTextColorStyle), freshScreen);
        }

        //public void DisplayMessage(string text)
        //{

        //}

        public void SwitchStandaloneMenu()
        {
            if (m_standAloneMenu != null)
                m_standAloneMenu.gameObject.SetActive(true);
            else
                m_standAloneMenu = Instantiate(m_standaloneMenuPrefab, m_screensRoot);
        }
    }
}
