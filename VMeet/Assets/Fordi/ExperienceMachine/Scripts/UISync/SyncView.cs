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



    /// <summary>
    /// A SyncView identifies an object across the network (viewID) and configures how the controlling client updates remote instances.
    /// </summary>
    /// \ingroup publicApi
    [AddComponentMenu("Fordi Networking/Sync View")]
    [ExecuteInEditMode]
    public class SyncView : MonoBehaviour
    {
        public List<Component> ObservedComponents;


        [SerializeField]
        private int viewIdField = 0;

        public int ViewID { get { return viewIdField; } }

        protected internal void Awake()
        {
            if (viewIdField == 0)
                viewIdField = gameObject.GetInstanceID();

            if (this.ViewID != 0 && Application.isPlaying)
                FordiNetwork.RegisterPhotonView(this);
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
            return string.Format("View {0} on {1}", this.ViewID, (this.gameObject != null) ? this.gameObject.name : "GO==null");
        }
    }
}