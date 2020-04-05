using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Sync
{
    public interface IFordiNetwork
    {

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

            if (syncView.ViewID == 0)
            {
                // don't register views with ID 0 (not initialized). they register when a ID is assigned later on
                Debug.Log("SyncView register is ignored, because viewID is 0. No id assigned yet to: " + syncView);
                return;
            }

            bool isViewListed = m_syncViewList.TryGetValue(syncView.ViewID, out SyncViewPair listedViewPair);
            if (isViewListed)
            {
                listedViewPair.Register(syncView);
                m_syncViewList[syncView.ViewID] = listedViewPair;
            }
            else
            {
                var viewPair = new SyncViewPair();
                viewPair.Register(syncView);
                m_syncViewList.Add(syncView.ViewID, viewPair);
            }
        }
    }
}
