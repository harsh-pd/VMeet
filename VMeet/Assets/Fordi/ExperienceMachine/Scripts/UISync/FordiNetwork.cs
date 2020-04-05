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
        //private static Dictionary<int, SyncView> m_syncViewList = new Dictionary<int, SyncView>();

        //public static SyncView GetPhotonView(int viewID)
        //{
        //    SyncView result = null;
        //    m_syncViewList.TryGetValue(viewID, out result);

        //    if (result == null)
        //    {
        //        SyncView[] views = GameObject.FindObjectsOfType(typeof(SyncView)) as SyncView[];

        //        for (int i = 0; i < views.Length; i++)
        //        {
        //            SyncView view = views[i];
        //            if (view.ViewID == viewID)
        //            {
        //                return view;
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public static void RegisterPhotonView(SyncView syncView)
        //{
        //    if (!Application.isPlaying)
        //    {
        //        m_syncViewList = new Dictionary<int, SyncView>();
        //        return;
        //    }

        //    if (syncView.ViewID == 0)
        //    {
        //        // don't register views with ID 0 (not initialized). they register when a ID is assigned later on
        //        Debug.Log("SyncView register is ignored, because viewID is 0. No id assigned yet to: " + syncView);
        //        return;
        //    }

        //    SyncView listedView = null;
        //    bool isViewListed = m_syncViewList.TryGetValue(syncView.ViewID, out listedView);
        //    if (isViewListed)
        //    {
        //        // if some other view is in the list already, we got a problem. it might be undestructible. print out error
        //        if (syncView != listedView)
        //        {
        //            Debug.LogError(string.Format("SyncView ID duplicate found: {0}. New: {1} old: {2}. Maybe one wasn't destroyed on scene load?! Check for 'DontDestroyOnLoad'. Destroying old entry, adding new.", syncView.ViewID, syncView, listedView));
        //        }
        //        else
        //        {
        //            return;
        //        }
        //    }

        //    // Debug.Log("adding view to known list: " + netView);
        //    m_syncViewList.Add(syncView.ViewID, syncView);
        //    //Debug.LogError("view being added. " + netView);	// Exit Games internal log
        //}
    }
}
