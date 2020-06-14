using Fordi.UI.MenuControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Fordi.Plugins
{
    public interface IFordiComponent
    {
        string Version { get; }
        string DepsKey { get; }
    }

    public interface IPlatformComponent : IFordiComponent
    {

    }

    public interface IFordiModule : IFordiComponent
    {
        
    }
}
