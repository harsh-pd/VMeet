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

        [SerializeField]
        private GameObject m_loaderPrefab;

        private Button m_actionButton;
        private TextMeshProUGUI m_resultText;

        private List<TMP_InputField> m_inputs = new List<TMP_InputField>();

        private int m_inputIndex = 0;

        protected override void Update()
        {
            if (m_inputs.Count == 0)
                return;

            if (m_vrMenu.ActiveModule == InputModule.STANDALONE && !m_blocker.gameObject.activeSelf && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab))
            {
                m_inputIndex--;
                if (m_inputIndex < 0)
                    m_inputIndex = m_inputs.Count - 1;
                m_inputs[m_inputIndex].Select();
                return;
            }
            else if (m_vrMenu.ActiveModule == InputModule.STANDALONE && !m_blocker.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Tab))
            {
                m_inputIndex++;
                if (m_inputIndex > m_inputs.Count - 1)
                    m_inputIndex = 0;
                m_inputs[m_inputIndex].Select();
            }
        }

        public override void DisplayResult(Error error)
        {
            if (m_loader != null)
                m_loader.gameObject.SetActive(false);

            if (m_blocker != null)
                m_blocker.gameObject.SetActive(false);
            
            if (error.HasError)
                m_resultText.text = error.ErrorText.Style(ExperienceMachine.ErrorTextColorStyle);
            else
                m_resultText.text = error.ErrorText.Style(ExperienceMachine.CorrectTextColorStyle);

            if (Pair != null && !(Pair is IForm))
                throw new InvalidCastException();

            if (Pair != null)
                ((IForm)Pair).DisplayResult(error);
        }

        public override void DisplayProgress(string text)
        {
            if (m_loader == null && m_loaderPrefab != null)
                m_loader = Instantiate(m_loaderPrefab, m_actionButton.transform);
            else if (m_loader != null)
                m_loader.SetActive(true);
            if (m_blocker != null)
                m_blocker.gameObject.SetActive(true);

            m_resultText.text = text.Style(ExperienceMachine.ProgressTextColorStyle);
            if (Pair != null && !(Pair is IForm))
                throw new InvalidCastException();

            if (Pair != null)
                ((IForm)Pair).DisplayProgress(text);
        }

        public override IMenuItem SpawnMenuItem(MenuItemInfo menuItemInfo, GameObject prefab, Transform parent)
        {
            FormItem menuItem = Instantiate(prefab, parent, false).GetComponentInChildren<FormItem>();
            //menuItem.name = "MenuItem";
            menuItem.Item = menuItemInfo;
            m_inputs.Add(((FormItem)menuItem).InputField);
            ((FormItem)menuItem).InputField.onEndEdit.AddListener(OnInputEnter);
            return menuItem;
        }

        public void OnInputEnter(string value)
        {
            if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
            {
                if (m_actionButton.onClick != null)
                    m_actionButton.onClick.Invoke();
            }
        }

        public virtual void OpenForm(FormArgs args, bool blocked, bool persist)
        {
            //Clear();
            Blocked = blocked;
            Persist = persist;
            gameObject.SetActive(true);
            foreach (var item in args.FormItems)
                SpawnMenuItem(item, m_menuItem, m_contentRoot);

            m_title.text = args.Title;

            m_actionButton = Instantiate(m_actionButtonPrefab, m_contentRoot);
            if (args.OnClickAction != null)
            {
                m_actionButton.onClick.AddListener(() =>
                {
                    string[] inputs = new string[m_inputs.Count];
                    for (int i = 0; i < m_inputs.Count; i++)
                        inputs[i] = m_inputs[i].text;
                    args.OnClickAction.Invoke(inputs);
                });
            }

            m_actionButton.GetComponentInChildren<TextMeshProUGUI>().text = args.ActionName;

            m_resultText = Instantiate(m_resultTextPrefab, m_contentRoot);
            m_resultText.text = "";

            if (m_vrMenu == null)
                m_vrMenu = IOC.Resolve<IVRMenu>();

            if (m_okButton != null)
                m_okButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());
            if (m_closeButton != null)
                m_closeButton.onClick.AddListener(() => m_vrMenu.CloseLastScreen());

            if (m_inputs.Count > 0)
            {
                m_inputs[0].Select();
                m_inputIndex = 0;
            }

            for (int i = 0; i < m_inputs.Count; i++)
            {
                int index = i;
                m_inputs[i].onSelect.AddListener((val) => m_inputIndex = index);
            }
        }
    }
}