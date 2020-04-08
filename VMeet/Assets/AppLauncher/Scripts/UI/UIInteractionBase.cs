using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using VRExperience.Core;
using VRExperience.Common;
using Fordi.Sync;

namespace AL.UI
{
    public class UIInteractionBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IFordiObservable
    {
        [SerializeField]
        protected Shadow shadow;

        [SerializeField]
        protected Image image;

        [SerializeField]
        protected Selectable selectable;

        protected IAppTheme m_appTheme;

        protected bool mouseHovering = false;

        private void Awake()
        {
            m_appTheme = IOC.Resolve<IAppTheme>();
            AwakeOverride();
        }

        protected virtual void AwakeOverride() { }
       
        private void Start()
        {
            Init();
        }

        private void OnDestroy()
        {
            OnDestroyOverride();
        }

        protected virtual void OnDestroyOverride() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable()
        {
            ToggleOutlineHighlight(false);
            ToggleBackgroundHighlight(false);
        }

        public virtual void Init()
        {
            ToggleBackgroundHighlight(false);
            ToggleOutlineHighlight(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseHovering = true;
            ToggleOutlineHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseHovering = false;
            ToggleOutlineHighlight(false);
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
            if (shadow == null)
                return;

            if (val && selectable.interactable)
                shadow.effectColor = m_appTheme.SelectedTheme.colorMix2;
            else
                shadow.effectColor = m_appTheme.SelectedTheme.panelInteractionOutline;
        }

        public virtual void ToggleBackgroundHighlight(bool val) { }

        public virtual void OnReset() { }

        public void HighlightOutline(Color col)
        {
            shadow.effectColor = col;
        }

        public void HardSelect()
        {
            ToggleBackgroundHighlight(true);
            ToggleOutlineHighlight(true);
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

            set
            {

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
            Debug.LogError(name + " " + viewId);
            if (typeof(string) == typeof(T) && selectable is TMP_InputField inputField)
                inputField.SetValue((string)(object)val);

            if (typeof(bool) == typeof(T) && selectable is Toggle toggle)
                toggle.SetValue((bool)(object)val);

            if (typeof(float) == typeof(T) && selectable is Slider slider)
                slider.SetValue((float)(object)val);
        }
        #endregion
    }
}