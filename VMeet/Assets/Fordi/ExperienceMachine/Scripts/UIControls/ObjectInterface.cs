using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fordi.Common;
using Fordi.Core;

namespace Fordi.UI.MenuControl
{
    public class ObjectInterface : MenuScreen
    {
        [SerializeField]
        private ObjectGrid m_objectGridPrefab;
        [SerializeField]
        private ScrollRect m_scrollRect;
        [SerializeField]
        private GameObject m_emptyItemPrefab;

        [SerializeField]
        private Vector2Int m_dimentions = new Vector2Int(3, 3);

        private List<ObjectItem> m_objectItems = new List<ObjectItem>();


        private ObjectGrid m_objectGrid = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
        }

        public override IMenuItem SpawnMenuItem(MenuItemInfo menuItemInfo, GameObject prefab, Transform parent)
        {
            ObjectItem objectItem = Instantiate(prefab, parent, false).GetComponentInChildren<ObjectItem>();
            objectItem.DataBind(m_userInterface, menuItemInfo);
            m_objectItems.Add(objectItem);
            return objectItem;
            //m_scrollRect.enabled = m_objectItems.Count > 12;
        }

        public override void OpenGridMenu(IUserInterface userInterface, GridArgs args)
        {
            m_objectItems.Clear();
            base.OpenGridMenu(userInterface, args);

            int emptyItemCount = m_dimentions.x - m_objectItems.Count % m_dimentions.x;

            if (m_objectItems.Count + emptyItemCount < m_dimentions.x * m_dimentions.y)
                emptyItemCount += m_dimentions.x * m_dimentions.y - (m_objectItems.Count + emptyItemCount);

            for (int i = 0; i < emptyItemCount; i++)
                Instantiate(m_emptyItemPrefab, m_contentRoot);
        }
    }
}
