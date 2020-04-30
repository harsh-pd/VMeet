using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using VRExperience.Core;
using VRExperience.Common;
using System;
using Fordi.Sync;

namespace VRExperience.UI
{
    [DisallowMultipleComponent]
    public class UIInteractionBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IFordiObservable
    {
        [SerializeField]
        private Transform m_root;

        [SerializeField]
        protected Shadow shadow;

        [SerializeField]
        protected GameObject m_solidBackground = null;

        [SerializeField]
        protected Image selection;

        [SerializeField]
        protected Selectable selectable;

        protected bool pointerHovering = false;

        protected IAppTheme m_appTheme;
        protected IAudio m_audio;
        protected IExperienceMachine m_experienceMachine = null;

        public static event EventHandler<PointerEventData> OnClick;

        private void Awake()
        {
            m_appTheme = IOC.Resolve<IAppTheme>();
            m_audio = IOC.Resolve<IAudio>();
            m_experienceMachine = IOC.Resolve<IExperienceMachine>();
            if (m_solidBackground)
                m_solidBackground.SetActive(m_experienceMachine.CurrentExperience != ExperienceType.HOME);
            AwakeOverride();

        }

        private void OnDestroy()
        {
            if (selectable != null && selectable is Toggle toggle)
                toggle.onValueChanged.RemoveAllListeners();
            if (selectable != null && selectable is Button button)
                button.onClick.RemoveAllListeners();
            if (selectable != null && selectable is Slider slider)
                slider.onValueChanged.RemoveAllListeners();
            if (selectable != null && selectable is TMP_InputField inputField)
                inputField.onValueChanged.RemoveAllListeners();

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


        private void OnEnable()
        {
            OnEnableOverride();
        }

        public void OnDisable()
        {
            pointerHovering = false;
            OnDisableOverride();
        }

        protected virtual void OnEnableOverride() { }

        protected virtual void OnDisableOverride()
        {
            ToggleOutlineHighlight(false);
            ToggleBackgroundHighlight(false);
            Pop(false);
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public virtual void Init()
        {
            ToggleBackgroundHighlight(false);
            ToggleOutlineHighlight(false);
            Pop(false);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            pointerHovering = true;
            ToggleOutlineHighlight(true);
            Pop(true);
        }

        protected virtual void Pop(bool val)
        {
            if (m_solidBackground)
            {
                if (m_root == null)
                    m_root = transform;
                m_root.localPosition = new Vector3(m_root.localPosition.x, m_root.localPosition.y, val ? -5 : -2);
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            pointerHovering = false;
            ToggleOutlineHighlight(false);
            ToggleBackgroundHighlight(false);
            Pop(false);
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ToggleBackgroundHighlight(true);
            Pop(false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ToggleBackgroundHighlight(false);
            if (pointerHovering)
                Pop(true);
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
            Pop(true);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this, eventData);
        }

        #region DESKTOP_VR_SYNC
        private SyncView m_syncView = null;

        public int ViewId
        {
            get
            {
                if (m_syncView != null)
                    return m_syncView.ViewId;
                m_syncView = GetComponent<SyncView>();
                if (m_syncView != null)
                    return m_syncView.ViewId;
                return 0;
            }
        }

        public Selectable Selectable { get { return selectable; } }

        public void OnFordiSerializeView(FordiStream stream, FordiMessageInfo info)
        {
            throw new System.NotImplementedException();
        }

        public void Select(int viewId)
        {
            if (selectable is TMP_InputField inputField)
                inputField.Select();
        }

        
        public void OnValueChanged<T>(int viewId, T val)
        {
            //Debug.LogError(name + " " + viewId);
            if (typeof(string) == typeof(T) && selectable is TMP_InputField inputField)
                inputField.text = (string)(object)val;

            if (typeof(bool) == typeof(T) && selectable is Toggle toggle)
                toggle.isOn = (bool)(object)val;

            if (typeof(float) == typeof(T) && selectable is Slider slider)
                slider.value = (float)(object)val;
        }

        public void PointerClickEvent(int viewId)
        {
            Button button = (Button)Selectable;
            button?.onClick.Invoke();
        }
        #endregion
    }
}