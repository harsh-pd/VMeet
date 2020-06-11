using Fordi.AssetManagement;
using Fordi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Fordi.Plugins
{
    public interface IPluginHook
    {
    }

    public class PluginHook : MonoBehaviour, IPluginHook
    {
        [ImportMany(typeof(IFordiComponent))]
        Lazy<IFordiComponent, Dictionary<string, object>>[] loggers { get; set; }

        private IAssetLoader m_assetLoader = null;

        private void Start()
        {
            m_assetLoader = IOC.Resolve<IAssetLoader>();
            Compose();
            Init();
        }

        private void Init()
        {
            foreach (var item in loggers)
                LoadDependency(item.Value.DepsKey);
        }

        private void Compose()
        {
            Debug.LogError(Directory.GetCurrentDirectory().ToString());

            var agreegateCatalog = new AggregateCatalog();

            var directories = Directory.GetDirectories(Path.Combine(Application.persistentDataPath, "Plugins"));

            foreach (var item in directories)
                agreegateCatalog.Catalogs.Add(new DirectoryCatalog(item));

            var assem = System.Reflection.Assembly.GetExecutingAssembly();
            var catThisAssembly = new AssemblyCatalog(assem);
            agreegateCatalog.Catalogs.Add(catThisAssembly);

            var compose = new CompositionContainer(agreegateCatalog);

            compose.ComposeParts(this);
        }

        private void LoadDependency(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            Debug.LogError("LoadDependency: " + key);
            m_assetLoader.LoadAndSpawn<GameObject>(new AssetArgs(key, false));
        }
    }
}