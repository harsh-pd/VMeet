using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fordi.Common;
using Fordi.Core;
using System;

namespace Fordi.UI
{
    public class VRDropdownInteraction : VRUIInteractionBase, IPointerClickHandler
    {
        [SerializeField]
        protected TextMeshProUGUI m_text;
        [SerializeField]
        protected Image m_image;

        IUIEngine m_uiEngine;

        public Color overriddenHighlight = Color.white;

        public bool overrideColor = false;

        TMP_Dropdown m_dropdown;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_uiEngine = IOC.Resolve<IUIEngine>();
            m_dropdown = (TMP_Dropdown)selectable;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_blocker != null)
                Destroy(m_blocker);
            m_dropdown.onValueChanged.RemoveAllListeners();
        }

        //private void OnOptionsShow(object sender, EventArgs e)
        //{
        //    var rootCanvas = m_uiEngine.GetRootCanvas(m_platform);
        //    if (rootCanvas != null && m_blocker == null)
        //    {
        //        m_blocker = Instantiate(m_customBlockerPrefab, transform);
        //        m_blocker.transform.SetParent(rootCanvas.transform);
        //        m_blocker.gameObject.SetActive(true);
        //    }
        //    if (m_blocker != null)
        //        m_blocker.gameObject.SetActive(true);
        //}

        //private void OnOptionsHide(object sender, EventArgs e)
        //{
        //    if (m_blocker != null)
        //        m_blocker.gameObject.SetActive(false);
        //}

        public override void ToggleOutlineHighlight(bool val)
        {
            if (val && selectable.interactable)
                m_text.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
            else
                m_text.color = overrideColor ? overriddenHighlight : m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;

            if (m_image != null)
            {
                if (val && selectable.interactable)
                    m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                else
                    m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
            }

            if (overrideColor)
            {
                selection.color = val ? overriddenHighlight : Color.white;
            }
        }

        public override void ToggleBackgroundHighlight(bool val)
        {

        }

        public override void OnReset()
        {
            //print("Reset");
            if (m_image != null)
            {
                if (pointerHovering)
                    m_text.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                else
                    m_text.color = overrideColor ? overriddenHighlight : m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
            }

            if (m_image != null)
            {
                if (pointerHovering)
                    m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                else
                    m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
            }
        }

        private GameObject m_blocker = null;

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            //print("selectable.interactable: true " + "OnPointerClick");
            //m_audio.PlaySFX(Audio.PointerClick);
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (selectable.interactable)
                OnPointerClick(null);
        }

    }
}
