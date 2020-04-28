using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VRExperience.UI.MenuControl
{
    public class ColorPalette : MenuScreen
    {
        [SerializeField]
        ToggleGroup m_group = null;

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
            }
            return menuItem;
        }
    }
}
