using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using VRExperience.Core;
using VRExperience.Common;
using System;

namespace VRExperience.UI
{
    [DisallowMultipleComponent]
    public class UIInteractionBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField]
        protected Shadow shadow;

        [SerializeField]
        protected Image selection;

        [SerializeField]
        protected Selectable selectable;

        protected bool pointerHovering = false;

        protected IAppTheme m_appTheme;
        protected IAudio m_audio;

        public static event EventHandler<PointerEventData> OnClick;

        private void Awake()
        {
            m_appTheme = IOC.Resolve<IAppTheme>();
            m_audio = IOC.Resolve<IAudio>();
            AwakeOverride();
        }

        private void OnDestroy()
        {
            OnDestroyOverride();
        }

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            UpdateOverride();
        }

        protected virtual void AwakeOverride()
        {
            
        }

        protected virtual void OnDestroyOverride()
        {
            
        }

        protected virtual void UpdateOverride()
        {

        }

        public void OnDisable()
        {
            pointerHovering = false;
            OnDisableOverride();
        }

        protected virtual void OnDisableOverride()
        {
            ToggleOutlineHighlight(false);
            ToggleBackgroundHighlight(false);
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public virtual void Init()
        {
            ToggleBackgroundHighlight(false);
            ToggleOutlineHighlight(false);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            pointerHovering = true;
            ToggleOutlineHighlight(true);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            pointerHovering = false;
            ToggleOutlineHighlight(false);
            ToggleBackgroundHighlight(false);
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ToggleBackgroundHighlight(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ToggleBackgroundHighlight(false);
        }

        public virtual void ToggleOutlineHighlight(bool val)
        {
            if (val && shadow && selectable.interactable)
                shadow.effectColor = m_appTheme.SelectedTheme.baseColor;
            else if(shadow)
                shadow.effectColor = m_appTheme.SelectedTheme.panelInteractionOutline;
        }

        public virtual void ToggleBackgroundHighlight(bool val) { }

        public virtual void OnReset() { }

        public void HighlightOutline(Color col)
        {
            if (shadow)
                shadow.effectColor = col;
        }

        public void HardSelect()
        {
            ToggleBackgroundHighlight(true);
            ToggleOutlineHighlight(true);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this, eventData);
        }
    }
}