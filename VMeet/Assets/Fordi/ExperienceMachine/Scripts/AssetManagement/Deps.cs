using Fordi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Fordi.AssetManagement
{
    [System.Serializable]
    public class AssetReferenceWrapper
    {
        public AssetReference AssetReference;
        public bool UnloadOnDestroy = false;
    }

    public class Deps : MonoBehaviour
    {
        [SerializeField]
        protected List<AssetReferenceWrapper> m_deps;

        protected IAssetLoader m_assetLoader = null;

        public List<AssetReferenceWrapper> Dependencies { get { return m_deps; } }

        private void Awake()
        {
            m_assetLoader = IOC.Resolve<IAssetLoader>();
        }

        protected virtual void Start()
        {
            foreach (var item in m_deps)
                m_assetLoader.LoadAndSpawn<GameObject>(new AssetArgs(item.AssetReference.RuntimeKey.ToString(), item.UnloadOnDestroy));
        }
    }
}
