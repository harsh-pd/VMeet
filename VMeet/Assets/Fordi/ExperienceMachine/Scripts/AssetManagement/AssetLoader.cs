using Fordi.AssetManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fordi.AssetManagement
{
    public interface IAssetLoader
    {
        void LoadAndSpawn<TObject>(AssetArgs args) where TObject : Object;
        void LoadAndSpawn<TObject>(AssetReference assetRef, bool unloadOnDestroy = false) where TObject : Object;
    }

    public struct AssetArgs
    {
        public string Key;
        public bool AutoUnload;

        public AssetArgs(string key, bool unloadOnDestroy) : this()
        {
            this.Key = key;
            this.AutoUnload = unloadOnDestroy;
        }
    }

    public class AssetLoader : MonoBehaviour, IAssetLoader
    {

        private readonly Dictionary<AssetArgs, AsyncOperationHandle<Object>> m_asyncOperationHandles = new Dictionary<AssetArgs, AsyncOperationHandle<Object>>();
        private Dictionary<string, List<GameObject>> m_spawnedObjects = new Dictionary<string, List<GameObject>>();

        public void LoadAndSpawn<TObject>(AssetArgs args) where TObject : Object
        {
            var op = Addressables.LoadAssetAsync<Object>(args.Key);

            m_asyncOperationHandles[args] = op;

            op.Completed += (operation) =>
            {
                if (op.Result is GameObject)
                {
                    Addressables.InstantiateAsync(args.Key).Completed += (asyncOpHandle) =>
                    {
                        Debug.LogError("Spawn operation status: " + asyncOpHandle.Status.ToString());
                        if (!m_spawnedObjects.ContainsKey(args.Key))
                            m_spawnedObjects[args.Key] = new List<GameObject>();
                        m_spawnedObjects[args.Key].Add(asyncOpHandle.Result);
                        var obj = asyncOpHandle.Result;
                        obj.AddComponent<NotifyOnDestroy>().Destroyed += (sender, eventArgs) =>
                        {
                            Addressables.ReleaseInstance(obj);
                            Debug.LogError("Released: " + obj.name);
                            m_spawnedObjects[args.Key].Remove(obj);
                            if (args.AutoUnload && m_spawnedObjects[args.Key].Count == 0)
                            {
                                Debug.LogError("Unloaded: " + m_asyncOperationHandles[args].DebugName);
                                Addressables.Release(m_asyncOperationHandles[args]);
                                m_asyncOperationHandles.Remove(args);
                            }
                        };
                    };
                }
            };
        }

        public void LoadAndSpawn<TObject>(AssetReference assetRef, bool unloadOnDestroy = false) where TObject: Object
        {
            LoadAndSpawn<TObject>(new AssetArgs(assetRef.RuntimeKey.ToString(), unloadOnDestroy));
        }
    }
}
