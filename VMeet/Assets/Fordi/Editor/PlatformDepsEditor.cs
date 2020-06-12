//C# Example (LookAtPointEditor.cs)
using UnityEngine;
using UnityEditor;
using Fordi.AssetManagement;

namespace Fordi.UnityEditor
{
    [CustomEditor(typeof(PlatformDeps))]
    [CanEditMultipleObjects]
    public class PlatformDepsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            PlatformDeps deps = (PlatformDeps)target;
            if (GUILayout.Button("Print Key"))
            {
                if (deps.Dependencies.Count > 0)
                    Debug.LogError(deps.Dependencies[0].AssetReference.RuntimeKey.ToString());
            }
        }
    }
}
