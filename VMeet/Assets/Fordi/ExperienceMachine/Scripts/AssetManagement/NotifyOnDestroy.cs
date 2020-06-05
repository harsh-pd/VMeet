using System;
using UnityEngine;

namespace Fordi.AssetManagement
{
    internal class NotifyOnDestroy : MonoBehaviour
    {
        public EventHandler Destroyed;
        private void OnDestroy()
        {
            Destroyed?.Invoke(this, EventArgs.Empty);
        }
    }
}