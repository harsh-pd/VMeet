using Papae.UnitySDK.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fordi.Common;
using Fordi.Core;

namespace Fordi.UI
{
    public class VRUIInteraction : UIInteractionBase, IPointerClickHandler
    {
        [SerializeField]
        protected TextMeshProUGUI m_text;
        [SerializeField]
        protected Image m_image;
        [SerializeField]
        private Image m_toggleImage;
        [SerializeField]
        private Image m_additionalImage = null;

        public Color overriddenHighlight = Color.white;

        public bool overrideColor = false;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            if (m_image != null && m_image.transform.childCount != 0 && m_additionalImage == null)
                m_additionalImage = m_image.transform.GetChild(0).GetComponent<Image>();
        }

        public override void ToggleOutlineHighlight(bool val)
        {
            if (val && selectable.interactable)
                m_text.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
            else
                m_text.color = overrideColor? overriddenHighlight : m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;

            if (m_image != null)
            {
                if (val && selectable.interactable)
                    m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                else
                    m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
            }

            if (m_additionalImage != null)
            {
                if (val && selectable.interactable)
                    m_additionalImage.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                else
                    m_additionalImage.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
            }

            if (m_toggleImage != null)
            {
                if (val && selectable.interactable)
                    m_toggleImage.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                else
                    m_toggleImage.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
            }

            //if (overrideColor)
            //{
            //    selection.color = val ? overriddenHighlight : Color.white;
            //}
        }

        public override void OnReset()
        {
            //print("Reset");
            if (m_image != null)
            {
                if (pointerHovering)
                    m_text.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                else
                    m_text.color = overrideColor? overriddenHighlight : m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
            }

            if (m_image != null)
            {
                if (pointerHovering)
                    m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                else
                    m_image.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
            }

            if (m_additionalImage != null)
            {
                if (pointerHovering)
                    m_additionalImage.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                else
                    m_additionalImage.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
            }

            if (m_toggleImage != null)
            {
                if (pointerHovering)
                    m_toggleImage.color = m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                else
                    m_toggleImage.color = m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        public void ToggleIcon()
        {
            if (m_toggleImage != null && m_image != null)
            {
                m_image.gameObject.SetActive(!m_image.gameObject.activeSelf);
                m_toggleImage.gameObject.SetActive(!m_toggleImage.gameObject.activeSelf);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (selectable.interactable)
                OnPointerClick(null);
        }

    }
}
