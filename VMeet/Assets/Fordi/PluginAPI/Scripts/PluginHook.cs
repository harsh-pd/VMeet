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
        EventHandler AllPlatformDependenciesLoaded { get; set; }
    }

    public class PluginHook : MonoBehaviour, IPluginHook
    {
        [ImportMany(typeof(IPlatformComponent))]
        Lazy<IPlatformComponent, Dictionary<string, object>>[] Platforms { get; set; }

        [ImportMany(typeof(IFordiModule))]
        Lazy<IFordiModule, Dictionary<string, object>>[] Modules { get; set; }

        public EventHandler AllPlatformDependenciesLoaded { get; set; }

        private IAssetLoader m_assetLoader = null;

        private void Awake()
        {
            PlatformDeps.AllDependenciesLoaded += AllPlatformDependencies;
        }

        private void OnDestroy()
        {
            PlatformDeps.AllDependenciesLoaded -= AllPlatformDependencies;
        }

        private void AllPlatformDependencies(object sender, EventArgs e)
        {
            AllPlatformDependenciesLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void Start()
        {
            m_assetLoader = IOC.Resolve<IAssetLoader>();
            Compose();
            Init();
        }

        private void Init()
        {
            if (Platforms.Length == 0)
                AllPlatformDependenciesLoaded?.Invoke(this, EventArgs.Empty);
            foreach (var item in Platforms)
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
            AssetArgs args = new AssetArgs(key, false);
            m_assetLoader.LoadAndSpawn<GameObject>(args);
        }
    }
}