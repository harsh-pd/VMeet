using Fordi.AssetManagement;
using Fordi.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Fordi.AssetManagement
{
    public interface IAssetLoader
    {
        void LoadAndSpawn<TObject>(AssetArgs args, Action<OperationResult> Completed = null) where TObject : Object;
        void LoadAndSpawn<TObject>(AssetReference assetRef, bool unloadOnDestroy = false) where TObject : Object;
    }

    public struct AssetArgs
    {
        public string Key;
        public bool AutoUnload;

        public AssetArgs(string key, bool unloadOnDestroy)
        {
            this.Key = key;
            this.AutoUnload = unloadOnDestroy;
        }
    }

    public class OperationResult
    {
        public Exception OperationException = null;
        public Object Result = null;
        public AsyncOperationStatus Status = AsyncOperationStatus.None;

        public OperationResult(Exception operationException, Object result, AsyncOperationStatus status)
        {
            OperationException = operationException;
            Result = result;
            Status = status;
        }
    }

    public class AssetLoader : MonoBehaviour, IAssetLoader
    {

        private static Dictionary<AssetArgs, AsyncOperationHandle<Object>> m_asyncOperationHandles = new Dictionary<AssetArgs, AsyncOperationHandle<Object>>();
        private Dictionary<string, List<GameObject>> m_spawnedObjects = new Dictionary<string, List<GameObject>>();

        public void LoadAndSpawn<TObject>(AssetArgs args, Action<OperationResult> OnComplete = null) where TObject : Object
        {
            //args.Key = "7723c6a301c4c404d99a27a6bf62a291";
            var assetRef = new AssetReference(args.Key);

            if (!assetRef.RuntimeKeyIsValid())
            {
                Debug.LogError("Invalid asset guid: " + args.Key);
                return;
            }

            Action<AsyncOperationHandle<Object>> spawnAction = (asyncOp) =>
            {
                if (asyncOp.Result is GameObject)
                {
                    Addressables.InstantiateAsync(args.Key).Completed += (asyncOpHandle) =>
                    {

                        if (asyncOpHandle.OperationException != null || asyncOpHandle.Status == AsyncOperationStatus.Failed || asyncOpHandle.Status == AsyncOperationStatus.None)
                        {
                            OnComplete?.Invoke(new OperationResult(asyncOpHandle.OperationException, asyncOpHandle.Result, asyncOpHandle.Status));
                            return;
                        }

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

                        OnComplete?.Invoke(new OperationResult(asyncOpHandle.OperationException, asyncOpHandle.Result, asyncOpHandle.Status));

                    };
                }
                else
                    OnComplete?.Invoke(new OperationResult(asyncOp.OperationException, asyncOp.Result, asyncOp.Status));
            };

            var op = Addressables.LoadAssetAsync<Object>(args.Key);

            if (m_asyncOperationHandles.ContainsKey(args))
            {
                var operation = m_asyncOperationHandles[args];
                if (operation.Result != null && operation.Result is GameObject)
                {
                    //Debug.LogError("Asset: " + operation.Result.name + " already loaded in memory, instantiating");
                    spawnAction.Invoke(m_asyncOperationHandles[args]);
                }
                return;
            }


            op.Completed += (operation) =>
            {
                if (op.OperationException != null || op.Status == AsyncOperationStatus.Failed || op.Status == AsyncOperationStatus.None)
                {
                    OnComplete?.Invoke(new OperationResult(op.OperationException, op.Result, op.Status));
                    Addressables.Release(op);
                    return;
                }

                m_asyncOperationHandles[args] = op;

                spawnAction.Invoke(op);
            };
        }

        public void LoadAndSpawn<TObject>(AssetReference assetRef, bool unloadOnDestroy = false) where TObject: Object
        {
            if (!assetRef.RuntimeKeyIsValid())
            {
                Debug.LogError("Invalid asset guid: " + assetRef.RuntimeKey.ToString());
                return;
            }

            LoadAndSpawn<TObject>(new AssetArgs(assetRef.RuntimeKey.ToString(), unloadOnDestroy));
        }
    }
}
