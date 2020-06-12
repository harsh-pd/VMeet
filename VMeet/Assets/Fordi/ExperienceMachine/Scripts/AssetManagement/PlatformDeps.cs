using Fordi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fordi.AssetManagement
{
    public class PlatformDeps : Deps
    {
        private int m_successfulLoad = 0;

        public static EventHandler AllDependenciesLoaded;

        protected override void Start()
        {
            foreach (var item in m_deps)
                m_assetLoader.LoadAndSpawn<GameObject>(new AssetArgs(item.AssetReference.RuntimeKey.ToString(), item.UnloadOnDestroy), (result) =>
                {
                    if (result.OperationException == null && result.Status == AsyncOperationStatus.Succeeded && result.Result != null)
                        m_successfulLoad++;
                    if (m_successfulLoad == m_deps.Count)
                        AllDependenciesLoaded?.Invoke(this, EventArgs.Empty);
                });
        }
    }
}
