using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.Common;
using VRExperience.Core;

namespace VRExperience.UI.MenuControl
{
    public interface IForm : IScreen
    {
        void DisplayError(Error error);
        void OpenForm(FormArgs args, bool blocked, bool persist);
    }

    public class FormArgs
    {
        public MenuItemInfo[] FormItems = new MenuItemInfo[] { };
        public Action<string[]> OnClickAction;
        public string Title;
        public string ActionName;

        public FormArgs(MenuItemInfo[] formItems, string title, string actionName, Action<string[]> onClickAction)
        {
            FormItems = formItems;
            Title = title;
            OnClickAction = onClickAction;
            ActionName = actionName;
        }
    }

    public class Form : MenuScreen, IForm
    {
        [SerializeField]
        private Button m_actionButtonPrefab;

        [SerializeField]
        private TextMeshProUGUI m_resultTextPrefab;

        public void DisplayError(Error error)
        {
            throw new System.NotImplementedException();
        }

        private Button m_actionButton;
        private TextMeshProUGUI m_resultText;

        public virtual void OpenForm(FormArgs args, bool blocked, bool persist)
        {
            Clear();
            Blocked = blocked;
            Persist = persist;
            gameObject.SetActive(true);
            foreach (var item in args.FormItems)
                SpawnMenuItem(item, m_menuItem, m_contentRoot);

            m_title.text = args.Title;

            m_actionButton = Instantiate(m_actionButtonPrefab, m_contentRoot);
            if (args.OnClickAction != null)
                m_actionButton.onClick.AddListener(() => args.OnClickAction.Invoke(new string[] { }));

            m_actionButton.GetComponentInChildren<TextMeshProUGUI>().text = args.ActionName;

            m_resultText = Instantiate(m_resultTextPrefab, m_contentRoot);
            m_resultText.text = "";

            if (m_vrMenu == null)
                m_vrMenu = IOC.Resolve<IVRMenu>();

            if (m_okButton != null)
                m_okButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());
            if (m_closeButton != null)
                m_closeButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());
        }
    }
}