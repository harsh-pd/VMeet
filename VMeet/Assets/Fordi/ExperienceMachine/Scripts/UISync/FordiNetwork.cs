using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Sync
{
    public interface IFordiNetwork
    {
        void OnValueChanged<T>(SyncView sender, int viewId, T val);
        void Select(SyncView sender, int viewId);
        void OnClick(SyncView sender, int viewId);
        void ActiveStateToggle(SyncView sender, int viewId, bool val);
    }

    public class FordiNetwork : MonoBehaviour, IFordiNetwork
    {
        private static Dictionary<int, SyncViewPair> m_syncViewList = new Dictionary<int, SyncViewPair>();
        

        public static void RegisterPhotonView(SyncView syncView)
        {
            if (syncView.ViewId == 0)
            {
                // don't register views with ID 0 (not initialized). they register when a ID is assigned later on
                Debug.LogError("SyncView register is ignored, because viewID is 0. No id assigned yet to: " + syncView);
                return;
            }

            bool isViewListed = m_syncViewList.TryGetValue(syncView.ViewId, out SyncViewPair listedViewPair);
            if (isViewListed)
            {
                listedViewPair.Register(syncView);
                m_syncViewList[syncView.ViewId] = listedViewPair;
                //Debug.LogError(m_syncViewList.Count + " " + listedViewPair.First.name);
            }
            else
            {
                var viewPair = new SyncViewPair();
                viewPair.Register(syncView);
                m_syncViewList.Add(syncView.ViewId, viewPair);
                //Debug.LogError(m_syncViewList.Count + " " + viewPair.First.name);
            }

            //Debug.LogError(m_syncViewList.Count + " " + m_syncViewList[syncView.ViewId].First.name);
        }

        public void ActiveStateToggle(SyncView sender, int viewId, bool val)
        {
            var viewPair = m_syncViewList[viewId];
            if (viewPair == null)
            {
                Debug.LogError("Can't sync view. ViewPair not found");
                return;
            }
            var pair = viewPair.GetPair(sender);
            if (pair == null)
            {
                Debug.LogError("No pair found for " + sender.name + " Id: " + viewId);
                return;
            }
            pair.ActiveStateToggle(viewId, val);
        }

        public void OnClick(SyncView sender, int viewId)
        {
            var viewPair = m_syncViewList[viewId];
            if (viewPair == null)
            {
                Debug.LogError("Can't sync view. ViewPair not found");
                return;
            }
            viewPair.GetPair(sender);
            var pair = viewPair.GetPair(sender);
            if (pair == null)
            {
                Debug.LogError("No pair found for " + sender.name + " Id: " + viewId);
                return;
            }
            pair.PointerClickEvent(viewId);
        }

        public void OnValueChanged<T>(SyncView sender, int viewId, T val)
        {
            //try
            //{
            //    Debug.LogError(sender.name + " " + viewId + " " + (bool)(object)val);
            //}
            //catch
            //{

            //}
            var viewPair = m_syncViewList[viewId];
            if (viewPair == null)
            {
                Debug.LogError("Can't sync view. ViewPair not found");
                return;
            }
            
            var pair = viewPair.GetPair(sender);
            if (pair == null)
            {
                Debug.LogError("No pair found for " + sender.name + " Id: " + viewId);
                return;
            }
            pair.OnValueChanged(viewId, val);
        }

        public void Select(SyncView sender, int viewId)
        {
            var viewPair = m_syncViewList[viewId];
            if (viewPair == null)
            {
                Debug.LogError("Can't sync view. ViewPair not found");
                return;
            }
            var pair = viewPair.GetPair(sender);
            if (pair == null)
            {
                Debug.LogError("No pair found for " + sender.name + " Id: " + viewId);
                return;
            }

            pair.Select(viewId);
        }
    }
}
