using UnityEngine;
using UnityEditor;
using Fordi.Core;
using System.Collections.Generic;

public class AssetSummaryExplorer : EditorWindow
{
    private string m_name = "Name";
    private string m_description = "Hello World";
    private Sprite m_sprite;
    private AudioClip m_clip;
    private Color m_color;
    private GameObject m_gameobject;

    private Vector2 m_scrollPos;

    private bool m_nature, m_home, m_mandala, m_abstract, m_global;
    private bool m_natureLocation, m_abstractLocation;

    private static AssetSummaryExplorer m_window;

    private static AssetDB m_assetDb = null;

    private static void FindAssetDatabase()
    {
        var databaseAssets = AssetDatabase.FindAssets("t:AssetDB");

        foreach (var item in databaseAssets)
        {
            var path = AssetDatabase.GUIDToAssetPath(item);
            m_assetDb = (AssetDB)AssetDatabase.LoadAssetAtPath(path, typeof(AssetDB));
            break;
        }
    }

    [MenuItem("Window/ExperienceMachine/Assets #R")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        m_window = (AssetSummaryExplorer)EditorWindow.GetWindow(typeof(AssetSummaryExplorer));
        m_window.minSize = new Vector2(300, 400);
        //m_window.maxSize = new Vector2(300, 300);
        FindAssetDatabase();
        m_window.Show();
    }

    void OnGUI()
    {
        if (m_assetDb == null)
        {
            FindAssetDatabase();
            if (m_assetDb == null)
            {
                GUILayout.Label("", EditorStyles.label);
                GUILayout.Label("No Asset Database Found. Create one by Create > Asset Database", EditorStyles.boldLabel);
                return;
            }
        }

        GUILayout.Label("", EditorStyles.boldLabel);
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 13
        };
        EditorGUILayout.LabelField("Experience Assets", style, GUILayout.ExpandWidth(true));

        Texture2D[] textureArray = new Texture2D[2] { new Texture2D(1, 1), new Texture2D(1, 1) };
        textureArray[0].SetPixel(0, 0, Color.grey * 0.05f);
        textureArray[0].Apply();

        textureArray[1].SetPixel(0, 0, Color.clear);
        textureArray[1].Apply();

        GUIStyle rectStyle = new GUIStyle();
        rectStyle.normal.background = textureArray[0];

        if (GUILayout.Button("Select Asset Database"))
        {
            EditorGUIUtility.PingObject(m_assetDb);
        }

        GUILayout.BeginArea(new Rect(20, 75, position.width - 40, position.height - 120), rectStyle);
        m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);

        m_global = EditorGUILayout.Foldout(m_global, "Global");
        if (m_global)
        {
            if (m_assetDb.AudioGroups.Length == 0 || (m_assetDb.AudioGroups.Length == 1 && string.IsNullOrEmpty(m_assetDb.AudioGroups[0].Name)))
            {

                if (GUILayout.Button("Voice Overs"))
                {
                    AssetExplorer.Init(ExperienceType.GLOBAL, ResourceType.AUDIO, "");
                }
            }
            else
            {
                foreach (var item in m_assetDb.AudioGroups)
                {
                    if (GUILayout.Button(item.Name + " Voice Overs"))
                    {
                        AssetExplorer.Init(ExperienceType.GLOBAL, ResourceType.AUDIO, item.Name);
                    }
                }
            }


            EditorGUILayout.LabelField("", EditorStyles.label);
            if (m_assetDb.GuideAudioGroups.Length == 0 || (m_assetDb.GuideAudioGroups.Length == 1 && string.IsNullOrEmpty(m_assetDb.GuideAudioGroups[0].Name)))
            {
                if (GUILayout.Button("Audio Guides"))
                {
                    AssetExplorer.Init(ExperienceType.GLOBAL, ResourceType.GUIDE_AUDIO, "");
                }
            }
            else
            {
                foreach (var item in m_assetDb.GuideAudioGroups)
                {
                    if (GUILayout.Button(item.Name + " Guides"))
                    {
                        AssetExplorer.Init(ExperienceType.GLOBAL, ResourceType.GUIDE_AUDIO, item.Name);
                    }
                }
            }

            EditorGUILayout.LabelField("", EditorStyles.label);
            if (m_assetDb.ObjectGroups.Length == 0 || (m_assetDb.ObjectGroups.Length == 1 && string.IsNullOrEmpty(m_assetDb.ObjectGroups[0].Name)))
            {
                if (GUILayout.Button("Props"))
                {
                    AssetExplorer.Init(ExperienceType.GLOBAL, ResourceType.OBJECT, "");
                }
            }
            else
            {
                foreach (var item in m_assetDb.ObjectGroups)
                {
                    if (GUILayout.Button(item.Name + " Items"))
                    {
                        AssetExplorer.Init(ExperienceType.GLOBAL, ResourceType.OBJECT, item.Name);
                    }
                }
            }
        }

        EditorGUILayout.LabelField("", EditorStyles.boldLabel);

        m_nature = EditorGUILayout.Foldout(m_nature, "Nature");
        if (m_nature)
        {
            if (m_assetDb.NatureLocationsGroups.Length == 0 || (m_assetDb.NatureLocationsGroups.Length == 1 && string.IsNullOrEmpty(m_assetDb.NatureLocationsGroups[0].Name)))
            {
                if (GUILayout.Button("Nature Locations"))
                {
                    AssetExplorer.Init(ExperienceType.NATURE, ResourceType.LOCATION, "");
                }
            }
            else
            {
                for (int i = 0; i < m_assetDb.NatureLocationsGroups.Length; i++)
                {

                    if (GUILayout.Button(m_assetDb.NatureLocationsGroups[i].Name + " Locations"))
                    {
                        AssetExplorer.Init(ExperienceType.NATURE, ResourceType.LOCATION, m_assetDb.NatureLocationsGroups[i].Name);
                    }
                }
            }

            //EditorGUILayout.LabelField("", EditorStyles.label);
            //if (m_assetDb.NatureMusic.Length == 0 || (m_assetDb.NatureMusic.Length == 1 && string.IsNullOrEmpty(m_assetDb.NatureMusic[0].Name)))
            //{
            //    if (GUILayout.Button("Music"))
            //    {
            //        AssetExplorer.Init(ExperienceType.NATURE, ResourceType.MUSIC, "");
            //    }
            //}
            //else
            //{
            //    foreach (var item in m_assetDb.NatureMusic)
            //    {
            //        if (GUILayout.Button(item.Name + " Music"))
            //        {
            //            AssetExplorer.Init(ExperienceType.NATURE, ResourceType.MUSIC, item.Name);
            //        }
            //    }
            //}
        }

        EditorGUILayout.LabelField("", EditorStyles.boldLabel);

        m_abstract = EditorGUILayout.Foldout(m_abstract, "Abstract");
        if (m_abstract)
        {
            if (m_assetDb.AbstractLocationsGroups.Length == 0 || (m_assetDb.AbstractLocationsGroups.Length == 1 && string.IsNullOrEmpty(m_assetDb.AbstractLocationsGroups[0].Name)))
            {
                if (GUILayout.Button("Abstract Locations"))
                    AssetExplorer.Init(ExperienceType.ABSTRACT, ResourceType.LOCATION, "");
            }
            else
            {
                for (int i = 0; i < m_assetDb.AbstractLocationsGroups.Length; i++)
                {

                    if (GUILayout.Button(m_assetDb.AbstractLocationsGroups[i].Name + " Locations"))
                    {
                        AssetExplorer.Init(ExperienceType.ABSTRACT, ResourceType.LOCATION, m_assetDb.AbstractLocationsGroups[i].Name);
                    }
                }
            }

            //EditorGUILayout.LabelField("", EditorStyles.label);
            //if (m_assetDb.AbstractMusic.Length == 0 || (m_assetDb.AbstractMusic.Length == 1 && string.IsNullOrEmpty(m_assetDb.AbstractMusic[0].Name)))
            //{
            //    if (GUILayout.Button("Music"))
            //    {
            //        AssetExplorer.Init(ExperienceType.ABSTRACT, ResourceType.MUSIC, "");
            //    }
            //}
            //else
            //{
            //    foreach (var item in m_assetDb.AbstractMusic)
            //    {
            //        if (GUILayout.Button(item.Name + " Music"))
            //        {
            //            AssetExplorer.Init(ExperienceType.ABSTRACT, ResourceType.MUSIC, item.Name);
            //        }
            //    }
            //}
        }

        EditorGUILayout.LabelField("", EditorStyles.boldLabel);

        m_mandala = EditorGUILayout.Foldout(m_mandala, "Mandala");
        if (m_mandala)
        {
            EditorGUILayout.LabelField("", EditorStyles.label);
            if (m_assetDb.ColorGroups.Length == 0 || (m_assetDb.ColorGroups.Length == 1 && string.IsNullOrEmpty(m_assetDb.ColorGroups[0].Name)))
            {
                if (GUILayout.Button("Mandala Colors"))
                {
                    AssetExplorer.Init(ExperienceType.MANDALA, ResourceType.COLOR, "");
                }
            }
            else
            {
                foreach (var item in m_assetDb.ColorGroups)
                {
                    if (GUILayout.Button(item.Name + " Colors"))
                    {
                        AssetExplorer.Init(ExperienceType.MANDALA, ResourceType.COLOR, item.Name);
                    }
                }
            }

            //EditorGUILayout.LabelField("", EditorStyles.label);
            //if (m_assetDb.MandalaMusic.Length == 0 || (m_assetDb.MandalaMusic.Length == 1 && string.IsNullOrEmpty(m_assetDb.MandalaMusic[0].Name)))
            //{
            //    if (GUILayout.Button("Music"))
            //    {
            //        AssetExplorer.Init(ExperienceType.MANDALA, ResourceType.MUSIC, "");
            //    }
            //}
            //else
            //{
            //    foreach (var item in m_assetDb.MandalaMusic)
            //    {
            //        if (GUILayout.Button(item.Name + " Music"))
            //        {
            //            AssetExplorer.Init(ExperienceType.MANDALA, ResourceType.MUSIC, item.Name);
            //        }
            //    }
            //}
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}