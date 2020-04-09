using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fordi.Sync.UI
{
    [DisallowMultipleComponent]
    public class UISync : MonoBehaviour, IFordiObservable, IPointerClickHandler
    {
        [SerializeField]
        private Selectable m_selectable;

        public EventHandler<bool> ActiveStateToggleEvent { get; set; }
        public EventHandler ClickEvent { get; set; }


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

        private void Awake()
        {
            AwakeOverride();
        }

        private void OnDestroy()
        {
            if (m_selectable != null && m_selectable is Toggle toggle)
                toggle.onValueChanged.RemoveAllListeners();
            if (m_selectable != null && m_selectable is Button button)
                button.onClick.RemoveAllListeners();
            if (m_selectable != null && m_selectable is Slider slider)
                slider.onValueChanged.RemoveAllListeners();
            if (m_selectable != null && m_selectable is TMP_InputField inputField)
                inputField.onValueChanged.RemoveAllListeners();
            DestroyOverride();
        }

        private void OnEnable()
        {
            ActiveStateToggleEvent?.Invoke(this, true);
        }

        private void OnDisable()
        {
            ActiveStateToggleEvent?.Invoke(this, false);
        }

        protected virtual void AwakeOverride() { }
        protected virtual void DestroyOverride() { }

        public Selectable Selectable { get { return m_selectable; } }

        public void OnFordiSerializeView(FordiStream stream, FordiMessageInfo info)
        {
            throw new System.NotImplementedException();
        }

        public void Select(int viewId)
        {
            if (m_selectable is TMP_InputField inputField)
                inputField.Select();
        }

        public void OnValueChanged<T>(int viewId, T val)
        {
            //Debug.LogError(viewId);
            if (typeof(string) == typeof(T) && m_selectable is TMP_InputField inputField)
                inputField.SetValue((string)(object)val);

            if (typeof(bool) == typeof(T) && m_selectable is Toggle toggle)
                toggle.SetValue((bool)(object)val);

            if (typeof(float) == typeof(T) && m_selectable is Slider slider)
                slider.SetValue((float)(object)val);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ClickEvent?.Invoke(this, EventArgs.Empty);
        }

        public void PointerClickEvent(int viewId)
        {
            Button button = (Button)Selectable;
            button?.onClick.Invoke();
        }
        #endregion
    }
}
