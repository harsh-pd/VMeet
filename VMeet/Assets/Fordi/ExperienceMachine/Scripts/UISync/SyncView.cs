// ----------------------------------------------------------------------------
// <copyright file="SyncView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
// Contains the SyncView class.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Fordi.Sync
{
    using System;
    using UnityEngine;
    using UnityEngine.Serialization;
    using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine.UI;
    using TMPro;
    using VRExperience.Common;
    using Fordi.Sync.UI;
    using VRExperience.Core;
    using VRExperience.UI.MenuControl;



    /// <summary>
    /// A SyncView identifies an object across the network (viewID) and configures how the controlling client updates remote instances.
    /// </summary>
    /// \ingroup publicApi
    [AddComponentMenu("Fordi Networking/Sync View")]
    [DisallowMultipleComponent]
    public class SyncView : MonoBehaviour, IFordiObservable
    {
        [SerializeField]
        private List<Component> ObservedComponents = new List<Component>();
        [SerializeField]
        private bool m_syncState = false;


        [SerializeField]
        private int viewIdField = 0;

        public int ViewId { get { return viewIdField; } }

        public Selectable Selectable { get { return null; } }

        private IFordiNetwork m_fordiNetwork;

        private IExperienceMachine m_experienceMachine;


        protected internal void Awake()
        {
            m_fordiNetwork = IOC.Resolve<IFordiNetwork>();

            if (m_syncState)
                return;

            foreach (var item in ObservedComponents)
            {
                if (((IFordiObservable)item) is UISync uiSync)
                {
                    //uiSync.ActiveStateToggleEvent += ActiveStateToggle;
                    uiSync.ClickEvent += ClickEvent;
                }

                if (((IFordiObservable)item).Selectable is TMP_InputField inputField)
                    inputField.onValueChanged.AddListener(OnValueChanged);
                if (((IFordiObservable)item).Selectable is Toggle toggle)
                    toggle.onValueChanged.AddListener((val) => OnValueChanged(toggle, val));
                if (((IFordiObservable)item).Selectable is Slider slider)
                    slider.onValueChanged.AddListener(OnValueChanged);
            }
        }

        private void OnDestroy()
        {
            foreach (var item in ObservedComponents)
            {
                if (((IFordiObservable)item) is UISync uiSync)
                {
                    //uiSync.ActiveStateToggleEvent -= ActiveStateToggle;
                    uiSync.ClickEvent -= ClickEvent;
                }

                if (((IFordiObservable)item).Selectable is TMP_InputField inputField)
                    inputField.onValueChanged.RemoveAllListeners();
                if (((IFordiObservable)item).Selectable is Toggle toggle)
                    toggle.onValueChanged.RemoveAllListeners();
                if (((IFordiObservable)item).Selectable is Slider slider)
                    slider.onValueChanged.RemoveAllListeners();
            }
        }

        private bool m_remoteValueChange = false;
        private void OnEnable()
        {
            if (m_remoteValueChange)
            {
                m_remoteValueChange = false;
                return;
            }
            if (m_syncState)
            {
                //Debug.LogError(name + " under " + transform.parent.name + " enabled");
                m_fordiNetwork.ActiveStateToggle(this, ViewId, true);
            }
        }

        private void OnDisable()
        {
            if (m_remoteValueChange)
            {
                m_remoteValueChange = false;
                return;
            }
            if (m_syncState)
            {
                //Debug.LogError(name + " under " + transform.parent.name + " disabled");
                m_fordiNetwork.ActiveStateToggle(this, ViewId, false);
            }
        }

        /// <summary>
        /// Temporary code used for listing the SyncView on FordiNetwork.
        /// </summary>
        private void Reset()
        {
            IScreen menu = transform.root.GetComponentInChildren<IScreen>();
            if (menu != null)
                menu.AttachSyncView(this);
        }

        public void SerializeView(FordiStream stream, FordiMessageInfo info)
        {
            if (this.ObservedComponents != null && this.ObservedComponents.Count > 0)
            {
                for (int i = 0; i < this.ObservedComponents.Count; ++i)
                {
                    SerializeComponent(this.ObservedComponents[i], stream, info);
                }
            }
        }

        public void DeserializeView(FordiStream stream, FordiMessageInfo info)
        {
            if (this.ObservedComponents != null && this.ObservedComponents.Count > 0)
            {
                for (int i = 0; i < this.ObservedComponents.Count; ++i)
                {
                    DeserializeComponent(this.ObservedComponents[i], stream, info);
                }
            }
        }

        protected internal void DeserializeComponent(Component component, FordiStream stream, FordiMessageInfo info)
        {
            if (component is IFordiObservable observable)
            {
                observable.OnFordiSerializeView(stream, info);
            }
            else
            {
                Debug.LogError("Observed scripts have to implement IPunObservable. " + component + " does not. It is Type: " + component.GetType(), component.gameObject);
            }
        }

        protected internal void SerializeComponent(Component component, FordiStream stream, FordiMessageInfo info)
        {
            IFordiObservable observable = component as IFordiObservable;
            if (observable != null)
            {
                observable.OnFordiSerializeView(stream, info);
            }
            else
            {
                Debug.LogError("Observed scripts have to implement IPunObservable. " + component + " does not. It is Type: " + component.GetType(), component.gameObject);
            }
        }

        public static SyncView Get(Component component)
        {
            return component.GetComponent<SyncView>();
        }

        public static SyncView Get(GameObject gameObj)
        {
            return gameObj.GetComponent<SyncView>();
        }

        //public static SyncView Find(int viewID)
        //{
        //    return FordiNetwork.GetPhotonView(viewID);
        //}

        public override string ToString()
        {
            return string.Format("View {0} on {1}", this.ViewId, (this.gameObject != null) ? this.gameObject.name : "GO==null");
        }

        #region SYNC_EVENT_RECEIVERS
        public void Select(int viewId)
        {
            var observable = (IFordiObservable)ObservedComponents.Find(item => (IFordiObservable)item != null && ((IFordiObservable)item).ViewId == viewId);
            observable?.Select(viewId);
        }

        public void OnValueChanged<T>(int viewId, T val)
        {
            m_remoteValueChange = true;
            var observable = (IFordiObservable)ObservedComponents.Find(item => (IFordiObservable)item != null && ((IFordiObservable)item).ViewId == viewId);
            observable?.OnValueChanged(ViewId, val);
        }

        public void OnFordiSerializeView(FordiStream stream, FordiMessageInfo info)
        {
            throw new NotImplementedException();
        }

        public void ActiveStateToggle(int viewId, bool e)
        {
            m_remoteValueChange = true;
            //Debug.LogError(name +  " ActiveStateToggle: " + viewId + " " + e);
            gameObject.SetActive(e);
        }

        public void PointerClickEvent(int viewId)
        {
            Button button = (Button)Selectable;
            button?.onClick.Invoke();
        }
        #endregion

        #region SYNC_EVENT_SENDERS
        private void OnValueChanged(string value)
        {
            if (m_remoteValueChange)
            {
                m_remoteValueChange = false;
                return;
            }
            m_fordiNetwork.OnValueChanged(this, ViewId, value);
        }

        private void OnValueChanged(Toggle toggle, bool value)
        {
            try
            {
                if (!value && toggle.group != null)
                    return;
            }
            catch (NullReferenceException e)
            {
                Debug.LogException(e);
            }
            

            if (m_remoteValueChange)
            {
                m_remoteValueChange = false;
                return;
            }

            //Debug.LogError(name + " value change: " + value);
            m_fordiNetwork.OnValueChanged(this, ViewId, value);
        }

        private void OnValueChanged(float value)
        {
            if (m_remoteValueChange)
            {
                m_remoteValueChange = false;
                return;
            }
            m_fordiNetwork.OnValueChanged(this, ViewId, value);
        }

        //Click sync disabled for now.
        private void ClickEvent(object sender, EventArgs e)
        {
            //Button button = (Button)Selectable;
            //button?.onClick.Invoke();
        }
        #endregion
    }
}