using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System;
using Fordi.Common;

namespace Fordi.UI
{
    public class FordiDropdown : TMP_Dropdown
    {
        private DropdownBlocker m_customBlockerPrefab;

        private IUIEngine m_uiEngine;

        protected override void Start()
        {
            base.Start();
            m_uiEngine = IOC.Resolve<IUIEngine>();
        }

        private bool m_customBlockerFlag = false;

        protected override GameObject CreateBlocker(Canvas rootCanvas)
        {
            if (!m_customBlockerFlag && m_customBlockerPrefab == null)
            {
                m_customBlockerFlag = true;
                m_customBlockerPrefab = GetComponentInChildren<DropdownBlocker>(true);
            }

            if (m_customBlockerPrefab == null)
                return base.CreateBlocker(rootCanvas);
            
            var blocker = Instantiate(m_customBlockerPrefab, transform);
            blocker.transform.SetParent(rootCanvas.transform);
            blocker.gameObject.SetActive(true);
            return blocker.gameObject;
        }
    }
}
