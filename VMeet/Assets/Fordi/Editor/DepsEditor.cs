//C# Example (LookAtPointEditor.cs)
using UnityEngine;
using UnityEditor;
using Fordi.AssetManagement;

namespace Fordi.UnityEditor
{
    [CustomEditor(typeof(Deps))]
    [CanEditMultipleObjects]
    public class LookAtPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Deps deps = (Deps)target;
            if (GUILayout.Button("Print Key"))
            {
                if (deps.Dependencies.Count > 0)
                    Debug.LogError(deps.Dependencies[0].RuntimeKey.ToString());
            }
        }
    }
}
