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
        public void LoadRemoteDesktopView(MenuItemInfo[] items, bool block = true, bool persist = false)
        {
            throw new NotImplementedException();
            //m_screensRoot.gameObject.SetActive(true);
            //var dMenu = Instantiate(m_dRemoteMonitorPrefab, m_dScreenRoot);
            //dMenu.OpenMenu(items, block, persist);
            //m_permanentDesktopScreen = dMenu;
        }

        public override IScreen OpenCalendar(Action<string> onClick, ITimeForm timeForm)
        {
            if (m_screenStack.Count > 0 && m_screenStack.Peek() is CalendarController)
            {
                return null;
            }

            m_screensRoot.gameObject.SetActive(true);

            var menu = (CalendarController)SpawnScreen(m_calendarPrefab);
            menu.OpenCalendar(null, timeForm);
            return menu;
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
