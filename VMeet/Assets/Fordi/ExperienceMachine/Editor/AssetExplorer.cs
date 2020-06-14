using UnityEngine;
using UnityEditor;
using Fordi.Core;
using System;

public class AssetExplorer : EditorWindow
{
    private static string[] m_name;
    private static string[] m_description;
    private static Sprite[] m_sprite;
    private static AudioClip[] m_clip;
    private static Color[] m_color;
    private static GameObject[] m_gameobject;

    private Vector2 m_scrollPos;

    private static ExperienceResource[] m_resources;

    private static AssetExplorer m_window;

    private static AssetDB m_assetDb = null;

    private ExperienceType m_experienceType;

    private static ResourceType m_resourceType;

    private static string m_title;

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

    private static void InitializeResources(ExperienceType experienceType, ResourceType resourceType, string category)
    {
        m_resources = null;
        switch (experienceType)
        {
            case ExperienceType.NATURE:
                if (resourceType == ResourceType.LOCATION )
                {
                    ExperienceGroup group = Array.Find(m_assetDb.NatureLocationsGroups, item => item.Name == category);
                    if (group != null)
                        m_resources = group.Resources;
                }

                if (resourceType == ResourceType.MUSIC)
                {
                    AudioGroup group = Array.Find(m_assetDb.NatureMusic, item => item.Name == category);
                    if (group != null)
                        m_resources = group.Resources;
                }
                break;
            case ExperienceType.MANDALA:
                if (resourceType == ResourceType.COLOR)
                {
                    ColorGroup group = Array.Find(m_assetDb.ColorGroups, item => item.Name == category);
                    if (group != null)
                        m_resources = group.Resources;
                }

                if (resourceType == ResourceType.MUSIC)
                {
                    AudioGroup group = Array.Find(m_assetDb.MandalaMusic, item => item.Name == category);
                    if (group != null)
                        m_resources = group.Resources;
                }
                break;
            case ExperienceType.ABSTRACT:
                if (resourceType == ResourceType.LOCATION)
                {
                    ExperienceGroup group = Array.Find(m_assetDb.AbstractLocationsGroups, item => item.Name == category);
                    if (group != null)
                        m_resources = group.Resources;
                }

                if (resourceType == ResourceType.MUSIC)
                {
                    AudioGroup group = Array.Find(m_assetDb.AbstractMusic, item => item.Name == category);
                    if (group != null)
                        m_resources = group.Resources;
                }
                break;
            case ExperienceType.GLOBAL:
                if (resourceType == ResourceType.GUIDE_AUDIO)
                {
                    GuideAudioGroup group = Array.Find(m_assetDb.GuideAudioGroups, item => item.Name == category);
                    if (group != null)
                        m_resources = group.Resources;
                }

                if (resourceType == ResourceType.AUDIO)
                {
                    AudioGroup group = Array.Find(m_assetDb.AudioGroups, item => item.Name == category);
                    if (group != null)
                        m_resources = group.Resources;
                }

                if (resourceType == ResourceType.OBJECT)
                {
                    ObjectGroup group = Array.Find(m_assetDb.ObjectGroups, item => item.Name == category);
                    if (group != null)
                        m_resources = group.Resources;
                }
                break;
            default:
                break;
        }

        if (m_resources == null)
            return;

        m_name = new string[m_resources.Length];
        m_color = new Color[m_resources.Length];
        m_description = new string[m_resources.Length];
        m_sprite = new Sprite[m_resources.Length];
        m_clip = new AudioClip[m_resources.Length];
    }

    //[MenuItem("Window/ExperienceMachine/Asset Explorer")]
    public static void Init(ExperienceType experienceType, ResourceType resourceType, string category)
    {
        //Debug.LogError(resourceType.ToString());
        // Get existing open window or if none, make a new one:
        m_window = (AssetExplorer)EditorWindow.GetWindow(typeof(AssetExplorer));
        m_window.minSize = new Vector2(1200, 300);
        m_resourceType = resourceType;
        FindAssetDatabase();
        string categoryText = string.IsNullOrEmpty(category) ? " " : " " + category.ToUpper() + " ";
        m_title = experienceType.ToString().ToUpper() + categoryText + resourceType.ToString().ToUpper() + " EXPLORER";
        InitializeResources(experienceType, resourceType, category);
        m_window.Show();
    }

    string tempName;
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
        var style = new GUIStyle(GUI.skin.label) {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 10
        };
        EditorGUILayout.LabelField(m_title, style, GUILayout.ExpandWidth(true));

        if (GUILayout.Button("Select Asset Database"))
        {
            EditorGUIUtility.PingObject(m_assetDb);
        }

        Texture2D[] textureArray = new Texture2D[2] { new Texture2D(1, 1), new Texture2D(1, 1) };

        textureArray[0].SetPixel(0, 0, Color.grey * 0.05f);
        textureArray[0].Apply();

        textureArray[1].SetPixel(0, 0, Color.clear);
        textureArray[1].Apply();


        GUIStyle rectStyle = new GUIStyle();
        rectStyle.normal.background = textureArray[0];

        
        GUILayout.BeginArea(new Rect(20, 75, position.width - 40, position.height - 120), rectStyle);

        EditorGUILayout.BeginHorizontal();

        if (m_resourceType == ResourceType.LOCATION || m_resourceType == ResourceType.MANDALA)
            EditorGUILayout.LabelField("Scene Name", EditorStyles.boldLabel);
        else
            EditorGUILayout.LabelField("Name", EditorStyles.boldLabel);

        if (m_resourceType == ResourceType.MANDALA)
            EditorGUILayout.LabelField("Mandala Name", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Descrition", EditorStyles.boldLabel);

        if (m_resourceType != ResourceType.COLOR)
        {
            if (m_resourceType != ResourceType.OBJECT)
                EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Large Preview", EditorStyles.boldLabel);
        }
        else
        {
            EditorGUILayout.LabelField("Primary Description", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Secondary Description", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Tertiary Description", EditorStyles.boldLabel);
        }

        if (m_resourceType == ResourceType.MANDALA)
            EditorGUILayout.LabelField("Mandala", EditorStyles.boldLabel);

        if (m_resourceType == ResourceType.OBJECT)
        {
            EditorGUILayout.LabelField("Object Prefab", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Object UI Prefab", EditorStyles.boldLabel);
        }

        if (m_resourceType == ResourceType.AUDIO || m_resourceType == ResourceType.MUSIC || m_resourceType == ResourceType.GUIDE_AUDIO)
            EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);

        if (m_resourceType == ResourceType.COLOR)
            EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);

        //EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        if (m_resources != null && m_resources.Length > 0)
        {
            m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);

            for (int i = 0; i < m_resources.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (m_resourceType == ResourceType.MANDALA)
                    ((MandalaResource)m_resources[i]).SceneName = EditorGUILayout.TextField(((MandalaResource)m_resources[i]).SceneName);
                else
                    m_resources[i].Name = EditorGUILayout.TextField(m_resources[i].Name);

                if (m_resourceType == ResourceType.MANDALA)
                    ((MandalaResource)m_resources[i]).Name = EditorGUILayout.TextField(((MandalaResource)m_resources[i]).Name);

                m_resources[i].Description = EditorGUILayout.TextField(m_resources[i].Description);
                if (m_resourceType != ResourceType.COLOR)
                {
                    if (m_resourceType != ResourceType.OBJECT)
                        m_resources[i].Preview = (Sprite)EditorGUILayout.ObjectField(m_resources[i].Preview, typeof(Sprite), true);
                    m_resources[i].LargePreview = (Sprite)EditorGUILayout.ObjectField(m_resources[i].LargePreview, typeof(Sprite), true);
                }
                else
                {
                    ((ColorResource)m_resources[i]).PrimaryDescription = EditorGUILayout.TextField(((ColorResource)m_resources[i]).PrimaryDescription);
                    ((ColorResource)m_resources[i]).SecondaryDescription = EditorGUILayout.TextField(((ColorResource)m_resources[i]).SecondaryDescription);
                    ((ColorResource)m_resources[i]).TertiaryDescription = EditorGUILayout.TextField(((ColorResource)m_resources[i]).TertiaryDescription);
                }
                switch (m_resourceType)
                {
                    case ResourceType.MUSIC:
                        ((AudioResource)m_resources[i]).Clip = (AudioClip)EditorGUILayout.ObjectField(((AudioResource)m_resources[i]).Clip, typeof(AudioClip), false);
                        break;
                    case ResourceType.COLOR:
                        //((ColorResource)m_resources[i]).Clip = (AudioClip)EditorGUILayout.ObjectField(((ColorResource)m_resources[i]).Clip, typeof(AudioClip), false);
                        ((ColorResource)m_resources[i]).Color = EditorGUILayout.ColorField(((ColorResource)m_resources[i]).Color);
                        break;
                    case ResourceType.AUDIO:
                        ((AudioResource)m_resources[i]).Clip = (AudioClip)EditorGUILayout.ObjectField(((AudioResource)m_resources[i]).Clip, typeof(AudioClip), false);
                        break;
                    case ResourceType.GUIDE_AUDIO:
                        ((AudioResource)m_resources[i]).Clip = (AudioClip)EditorGUILayout.ObjectField(((AudioResource)m_resources[i]).Clip, typeof(AudioClip), false);
                        break;
                    case ResourceType.MANDALA:
                        ((MandalaResource)m_resources[i]).Mandala = (GameObject)EditorGUILayout.ObjectField(((MandalaResource)m_resources[i]).Mandala, typeof(GameObject), false);
                        break;
                    case ResourceType.OBJECT:
                        ((ObjectResource)m_resources[i]).ObjectPrefab = (GameObject)EditorGUILayout.ObjectField(((ObjectResource)m_resources[i]).ObjectPrefab, typeof(GameObject), false);
                        ((ObjectResource)m_resources[i]).ObjectUIPrefab = (GameObject)EditorGUILayout.ObjectField(((ObjectResource)m_resources[i]).ObjectUIPrefab, typeof(GameObject), false);
                        break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndArea();
        //m_assetDb = (AssetDB)EditorGUILayout.ObjectField("", m_assetDb, typeof(AssetDB), false);
    }
}