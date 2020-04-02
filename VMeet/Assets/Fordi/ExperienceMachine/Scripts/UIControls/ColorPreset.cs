using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.Core;
using VRExperience.UI;
using VRExperience.UI.MenuControl;

namespace VRExperience.UI
{
    public class ColorPreset : ColorPicker
    {
        [SerializeField]
        private Toggle m_toggle;

        public Toggle Toggle { get { return m_toggle; } }

        private Image m_image;

        public bool Selected { get { return m_toggle.isOn; } }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_image = m_colorItemRoot.GetComponent<Image>();
        }

        protected override void OnEnableOverride()
        {
            base.OnEnableOverride();
            if (Colors != null)
                Invoke("DataBind", .1f);
        }

        protected override void DataBind()
        {
            if (m_image == null)
                m_image = m_colorItemRoot.GetComponent<Image>();

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
                colorItem.SetGraphic(m_image);
                colorItem.GetComponentInChildren<Button>().gameObject.SetActive(false);
                colorItem.enabled = false;
            }

            m_toggle.group = m_colorInfo.ToggleGroup;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }

        public override void ClickColor(MenuClickArgs args)
        {
            MenuClickArgs modifiedArgs = new MenuClickArgs(args.Path, args.Name, args.Command, MenuCommandType.SELECTION, m_colorInfo.ColorGroup);
            //m_experienceMachine.ExecuteMenuCommand(modifiedArgs);
        }

        public void Select()
        {
            m_toggle.isOn = true;
        }
    }
}