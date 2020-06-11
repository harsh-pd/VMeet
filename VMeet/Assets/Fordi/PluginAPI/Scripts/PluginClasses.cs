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
}
