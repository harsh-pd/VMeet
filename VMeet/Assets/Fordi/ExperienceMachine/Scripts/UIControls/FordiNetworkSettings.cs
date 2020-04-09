using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Sync
{
    public class FordiNetworkSettings : ScriptableObject
    {
        public List<SyncView> SyncViews = new List<SyncView>();

    }
}
