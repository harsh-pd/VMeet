using Fordi.Annotation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.Common;

namespace VRExperience.UI.MenuControl
{
    public class ColorPalette : MenuScreen
    {
        [SerializeField]
        ToggleGroup m_group = null;

        private IAnnotation m_annotaiton;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_annotaiton = IOC.Resolve<IAnnotation>();
        }

        public override void Close()
        {
            foreach (var item in m_menuItems)
                ((Toggle)item.Selectable).onValueChanged.RemoveAllListeners();
            
            base.Close();
        }

        public override IMenuItem SpawnMenuItem(MenuItemInfo menuItemInfo, GameObject prefab, Transform parent)
        {
            var menuItem = base.SpawnMenuItem(menuItemInfo, prefab, parent);
            if (menuItem is PaletteColorItem colorItem)
            {
                ((Toggle)colorItem.Selectable).group = m_group;
                ((Toggle)colorItem.Selectable).onValueChanged.AddListener((val) =>
                {
                    if (val)
                        m_preview.color = m_annotaiton.SelectedColor;
                });
            }
            return menuItem;
        }

        public override void OpenGridMenu(MenuItemInfo[] items, string title, bool blocked, bool persist, bool backEnabled = true, bool requireRefreshOnReopen = false)
        {
            if (m_annotaiton == null)
                m_annotaiton = IOC.Resolve<IAnnotation>();
            base.OpenGridMenu(items, title, blocked, persist, backEnabled, requireRefreshOnReopen);
            m_preview.color = m_annotaiton.SelectedColor;
            m_group.allowSwitchOff = false;
        }
    }
}
