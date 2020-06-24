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

    public interface IStandaloneInterface
    {
        IScreen LoadRemoteDesktopView(MenuArgs args);
    }

    public class DesktopInterface : UserInterface, IStandaloneInterface
    {
        #region INSPECTOR_REFRENCES
        [SerializeField]
        private StandaloneMenu m_standaloneMenuPrefab;
        [SerializeField]
        private RemoteMonitorScreen m_remoteMonitorScreen;
        #endregion


        private StandaloneMenu m_standAloneMenu = null;
        private IScreen m_permanentDesktopScreen = null;

        public override bool IsOpen { get { return m_screenStack.Count > 0 || m_permanentDesktopScreen != null; } }

        public override BaseInputModule InputModule
        {
            get
            {
                if (m_inputModule == null)
                {
                    m_inputModule = FindObjectOfType<StandaloneInputModule>();
                    if (m_inputModule == null)
                    {
                        Destroy(gameObject);
                        throw new Exception("VR input module not found.");
                    }
                }
                return m_inputModule;
            }
        }

        #region CORE
        public override IScreen OpenMenu(MenuArgs args)
        {
            if (m_permanentDesktopScreen != null)
                return m_permanentDesktopScreen;

            var menu = Instantiate(m_mainMenuPrefab, m_screensRoot);
            menu.OpenMenu(this, args);
            m_menuOn = true;

            m_permanentDesktopScreen = menu;
            return menu;
        }

        public IScreen LoadRemoteDesktopView(MenuArgs args)
        {
            var menu = Instantiate(m_remoteMonitorScreen, m_screensRoot);
            menu.OpenMenu(this, args);
            m_permanentDesktopScreen = menu;
            return menu;
        }

        public override IScreen OpenCalendar(CalendarArgs args)
        {
            var menu = Instantiate(m_calendarPrefab, m_screensRoot);
            m_screenStack.Push(menu);
            menu.OpenCalendar(this, args);
            return menu;
        }

        public override IScreen OpenAnnotationInterface(GridArgs args)
        {
            return DisplayMessage(new MessageArgs()
            {
                Persist = true,
                Block = true,
                Text = "Annotation available in VR.",
                BackEnabled = true
            });
        }
        #endregion

        public override IScreen DisplayProgress(string text, bool freshScreen = false)
        {
            return base.DisplayProgress(text.Style(ExperienceMachine.ProgressTextColorStyle), freshScreen);
        }

        public override IScreen Block(string message, bool includeRoot = false)
        {
            if (includeRoot)
                m_screensRoot.localScale = Vector3.zero;
            else
                m_screensRoot.localScale = m_screenRootScale;

            if (m_blocker != null)
            {
                m_blocker.Reopen();
                m_blocker.Gameobject.transform.localScale = includeRoot ? Vector3.zero : Vector3.one;
                return m_blocker;
            }
            else
            {
                var menu = Instantiate(m_genericLoader, m_screensRoot.parent);
                menu.Init(this, new MessageArgs()
                {
                    Persist = false,
                    Block = true,
                    Text = message,
                    BackEnabled = false
                });

                m_blocker = menu;
                m_menuOn = true;
                menu.transform.localScale = includeRoot ? Vector3.zero : Vector3.one;
                return menu;
            }
        }

        public override void CloseLastScreen()
        {
            base.CloseLastScreen();
            if (!IsOpen)
                SwitchStandaloneMenu();
        }

        public override void Close(IScreen screenToBeClosed)
        {
            base.Close(screenToBeClosed);
            if (!IsOpen)
                SwitchStandaloneMenu();
        }

        public override void Close()
        {
            base.Close();
            if (!IsOpen)
                SwitchStandaloneMenu();
        }

        public void SwitchStandaloneMenu()
        {
            if (m_standAloneMenu != null)
                m_standAloneMenu.gameObject.SetActive(true);
            else
                m_standAloneMenu = Instantiate(m_standaloneMenuPrefab, m_screensRoot);
        }
    }
}
