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
    }

    public class FordiNetwork : MonoBehaviour, IFordiNetwork
    {
        private static Dictionary<int, SyncViewPair> m_syncViewList = new Dictionary<int, SyncViewPair>();

        public static void RegisterPhotonView(SyncView syncView)
        {
            if (!Application.isPlaying)
            {
                m_syncViewList = new Dictionary<int, SyncViewPair>();
                return;
            }

            if (syncView.ViewId == 0)
            {
                // don't register views with ID 0 (not initialized). they register when a ID is assigned later on
                Debug.Log("SyncView register is ignored, because viewID is 0. No id assigned yet to: " + syncView);
                return;
            }

            bool isViewListed = m_syncViewList.TryGetValue(syncView.ViewId, out SyncViewPair listedViewPair);
            if (isViewListed)
            {
                listedViewPair.Register(syncView);
                m_syncViewList[syncView.ViewId] = listedViewPair;
                Debug.LogError(m_syncViewList.Count + " " + listedViewPair.First.name);
            }
            else
            {
                var viewPair = new SyncViewPair();
                viewPair.Register(syncView);
                m_syncViewList.Add(syncView.ViewId, viewPair);
                Debug.LogError(m_syncViewList.Count + " " + viewPair.First.name);
            }

            //Debug.LogError(m_syncViewList.Count + " " + m_syncViewList[syncView.ViewId].First.name);
        }

        public void OnValueChanged<T>(SyncView sender, int viewId, T val)
        {
            var viewPair = m_syncViewList[viewId];
            if (viewPair == null)
                Debug.LogError("Can't sync view. ViewPair not found");
            viewPair.GetPair(sender).OnValueChanged(viewId, val);
        }

        public void Select(SyncView sender, int viewId)
        {
            var viewPair = m_syncViewList[viewId];
            if (viewPair == null)
                Debug.LogError("Can't sync view. ViewPair not found");
            viewPair.GetPair(sender).Select(viewId);
        }
    }
}
