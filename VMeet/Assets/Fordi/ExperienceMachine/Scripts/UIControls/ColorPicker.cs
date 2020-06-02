using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fordi.Common;
using Fordi.Core;
using Fordi.UI.MenuControl;

namespace Fordi.UI
{
    public class ColorPicker : MonoBehaviour
    {
        [SerializeField]
        protected GameObject m_colorItemPrefab;
        [SerializeField]
        protected Transform m_colorItemRoot;
        [SerializeField]
        protected TextMeshProUGUI m_title;

        protected IExperienceMachine m_experienceMachine;

        protected ColorInfo m_colorInfo;
        public ColorInfo Colors
        {
            get { return m_colorInfo; }
            set
            {
                if (m_colorInfo != value)
                {
                    m_colorInfo = value;
                    DataBind();
                }
            }
        }

        private void Awake()
        {
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            AwakeOverride();
        }

        private void OnEnable()
        {
            OnEnableOverride();
        }

        private void OnDestroy()
        {
            OnDestroyOverride();
        }

        protected virtual void OnEnableOverride()
        {

        }

        protected virtual void AwakeOverride()
        {

        }

        protected virtual void OnDestroyOverride()
        {

        }

        protected virtual void DataBind()
        {
            foreach (Transform item in m_colorItemRoot)
                Destroy(item.gameObject);
            m_colorItemRoot.DetachChildren();

            m_title.text = m_colorInfo.Title;
            foreach (var item in m_colorInfo.ColorGroup.Resources)
            {
                MenuItem colorItem = Instantiate(m_colorItemPrefab, m_colorItemRoot).GetComponentInChildren<MenuItem>();
                colorItem.DataBind(new MenuItemInfo
                {
                    Path = "",
                    Text = "",
                    Command = "",
                    Icon = null,
                    Data = item,
                    CommandType = MenuCommandType.SELECTION
                }, this);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }

        public virtual void ClickColor(MenuClickArgs args)
        {
            if (m_colorInfo.OnItemClick != null)
                m_colorInfo.OnItemClick.Invoke(args);
        }
    }
}