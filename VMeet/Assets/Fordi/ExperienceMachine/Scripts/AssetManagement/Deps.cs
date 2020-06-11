using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Fordi.AssetManagement
{
    public class Deps : MonoBehaviour
    {
        [SerializeField]
        private List<AssetReference> m_deps;

        //#region TEMP
        //[SerializeField]
        //private AssetReference m_self;

        //public AssetReference Self { get { return m_self; } }
        //#endregion


        public List<AssetReference> Dependencies { get { return m_deps; } }
        

        private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> m_asyncOperationHandles = new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();
        private Dictionary<AssetReference, List<GameObject>> m_spawnedObjects = new Dictionary<AssetReference, List<GameObject>>();

        private void Start()
        {
            foreach (var item in m_deps)
                LoadAndSpawn(item);
        }

        private void LoadAndSpawn(AssetReference assetRef)
        {
            var op = Addressables.LoadAssetAsync<GameObject>(assetRef);

            m_asyncOperationHandles[assetRef] = op;
            op.Completed += (operation) =>
            {
                assetRef.InstantiateAsync().Completed += (asyncOpHandle) =>
                {
                    Debug.LogError("Spawn operation status: " + asyncOpHandle.Status.ToString());
                    if (!m_spawnedObjects.ContainsKey(assetRef))
                        m_spawnedObjects[assetRef] = new List<GameObject>();
                    m_spawnedObjects[assetRef].Add(asyncOpHandle.Result);
                    var obj = asyncOpHandle.Result;
                    obj.AddComponent<NotifyOnDestroy>().Destroyed += (sender, args) =>
                    {
                        Addressables.ReleaseInstance(obj);
                        Debug.LogError("Released: " + obj.name);
                        m_spawnedObjects[assetRef].Remove(obj);
                        if (m_spawnedObjects[assetRef].Count == 0)
                        {
                            Debug.LogError("Unloaded: " + m_asyncOperationHandles[assetRef].DebugName);
                            Addressables.Release(m_asyncOperationHandles[assetRef]);
                            m_asyncOperationHandles.Remove(assetRef);
                        }
                    };
                };
            };
        }
    }
}
