using Fordi.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Fordi.UnityEditor
{
    [CustomEditor(typeof(ProcessButton), true)]
    [CanEditMultipleObjects]
    public class ProcessButtonEditor : ButtonEditor
    {
        private SerializedProperty m_isOn;
        private SerializedProperty m_onImage;
        private SerializedProperty m_offImage;
        private SerializedProperty m_loadingImage;
        //private SerializedProperty m_onValueChangeRequest;
        private SerializedProperty m_autoToggle;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_isOn = serializedObject.FindProperty("m_isOn");
            m_onImage = serializedObject.FindProperty("m_onImage");
            m_offImage = serializedObject.FindProperty("m_offImage");
            m_loadingImage = serializedObject.FindProperty("m_loadingImage");
            //m_onValueChangeRequest = serializedObject.FindProperty("m_onValueChangeRequest");
            m_autoToggle = serializedObject.FindProperty("m_autoToggle");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            //EditorGUILayout.PropertyField(m_onValueChangeRequest);
            EditorGUILayout.PropertyField(m_isOn);
            EditorGUILayout.PropertyField(m_autoToggle);
            EditorGUILayout.PropertyField(m_onImage);
            EditorGUILayout.PropertyField(m_offImage);
            EditorGUILayout.PropertyField(m_loadingImage);

            serializedObject.ApplyModifiedProperties();
        }

    }
}
