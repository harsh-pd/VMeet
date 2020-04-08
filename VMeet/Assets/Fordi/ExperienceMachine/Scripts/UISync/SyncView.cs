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
    using UnityEngine.UI;
    using TMPro;
    using VRExperience.Common;
#endif



    /// <summary>
    /// A SyncView identifies an object across the network (viewID) and configures how the controlling client updates remote instances.
    /// </summary>
    /// \ingroup publicApi
    [AddComponentMenu("Fordi Networking/Sync View")]
    public class SyncView : MonoBehaviour, IFordiObservable
    {
        [SerializeField]
        private List<Component> ObservedComponents;


        [SerializeField]
        private int viewIdField = 0;

        public int ViewId { get { return viewIdField; } set { viewIdField = value; } }

        public Selectable Selectable { get { return null; } }

        private IFordiNetwork m_fordiNetwork;

        protected internal void Awake()
        {
            m_fordiNetwork = IOC.Resolve<IFordiNetwork>();
            if (viewIdField == 0)
                viewIdField = gameObject.GetInstanceID();

            if (this.ViewId != 0 && Application.isPlaying)
                FordiNetwork.RegisterPhotonView(this);

            foreach (var item in ObservedComponents)
            {
                if (((IFordiObservable)item).Selectable is TMP_InputField inputField)
                    inputField.onValueChanged.AddListener(OnValueChanged);
                if (((IFordiObservable)item).Selectable is Toggle toggle)
                    toggle.onValueChanged.AddListener(OnValueChanged);
                if (((IFordiObservable)item).Selectable is Slider slider)
                    slider.onValueChanged.AddListener(OnValueChanged);
            }
        }

        private void OnDestroy()
        {
            foreach (var item in ObservedComponents)
            {
                if (((IFordiObservable)item).Selectable is TMP_InputField inputField)
                    inputField.onValueChanged.RemoveAllListeners();
                if (((IFordiObservable)item).Selectable is Toggle toggle)
                    toggle.onValueChanged.RemoveAllListeners();
                if (((IFordiObservable)item).Selectable is Slider slider)
                    slider.onValueChanged.RemoveAllListeners();
            }
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
            var observable = (IFordiObservable)ObservedComponents.Find(item => (IFordiObservable)item != null && ((IFordiObservable)item).ViewId == viewId);
            observable?.OnValueChanged(ViewId, val);
            if (observable != null)
            {
                Debug.LogError(viewId + " ");
            }
            else
                Debug.LogError("obsrvr null ");

        }

        public void OnFordiSerializeView(FordiStream stream, FordiMessageInfo info)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region SYNC_EVENT_SENDERS
        private void OnValueChanged(string value)
        {
            m_fordiNetwork.OnValueChanged(this, ViewId, value);
        }

        private void OnValueChanged(bool value)
        {
            Debug.LogError(name + " " + value);
            m_fordiNetwork.OnValueChanged(this, ViewId, value);
        }

        private void OnValueChanged(float value)
        {
            m_fordiNetwork.OnValueChanged(this, ViewId, value);
        }
        #endregion
    }
}