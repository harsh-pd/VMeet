using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRExperience.Common;
using VRExperience.Core;

namespace VRExperience.UI.MenuControl
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

        private IPlayer m_player;

        private ObjectGrid m_objectGrid = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_player = IOC.Resolve<IPlayer>();
        }

        public override void SpawnMenuItem(MenuItemInfo menuItemInfo, GameObject prefab, Transform parent)
        {
            ObjectItem objectItem = Instantiate(prefab, parent, false).GetComponentInChildren<ObjectItem>();
            objectItem.Item = menuItemInfo;
            m_objectItems.Add(objectItem);
            //m_scrollRect.enabled = m_objectItems.Count > 12;
        }

        public override void OpenGridMenu(MenuItemInfo[] items, string title, bool blocked, bool persist, bool backEnabled = true)
        {
            m_objectItems.Clear();
            base.OpenGridMenu(items, title, blocked, persist, backEnabled);

            int emptyItemCount = m_dimentions.x - m_objectItems.Count % m_dimentions.x;

            if (m_objectItems.Count + emptyItemCount < m_dimentions.x * m_dimentions.y)
                emptyItemCount += m_dimentions.x * m_dimentions.y - (m_objectItems.Count + emptyItemCount);

            if (m_player == null)
                m_player = IOC.Resolve<IPlayer>();

            for (int i = 0; i < emptyItemCount; i++)
                Instantiate(m_emptyItemPrefab, m_contentRoot);
        }
    }
}
