using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fordi.Common;
using Fordi.Core;

namespace Fordi.UI
{
    public class VRUIInteractionBase : UIInteractionBase
    {
        public override void ToggleOutlineHighlight(bool val)
        {
        }

        public override void ToggleBackgroundHighlight(bool val)
        {
        }
    }


    public class VRButtonInteraction : VRUIInteractionBase, IPointerClickHandler
    {
        [SerializeField]
        protected TextMeshProUGUI m_text;
        [SerializeField]
        protected Image m_image;

        public Color overriddenHighlight = Color.white;

        public bool overrideColor = false;

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

        public override void ToggleBackgroundHighlight(bool val) {
            
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
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            //print("selectable.interactable: true " + "OnPointerClick");
            //m_audio.PlaySFX(Audio.PointerClick);
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (selectable.interactable)
                OnPointerClick(null);
        }

    }
}
