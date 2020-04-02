using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectCamera : MonoBehaviour
{
    [SerializeField]
    private Camera m_camera;

    [SerializeField]
    private RawImage m_targetImage;

    public ObjectHolder ObjectHolder { get; set; }

    public RawImage TargetImage { get { return m_targetImage; } set { m_targetImage = value; } }

    void Update()
    {
        PopulateObjectUI();
    }

    private void PopulateObjectUI()
    {
        if (m_targetImage == null || ObjectHolder == null)
            return;

        RenderTexture texture = null;
        if (m_targetImage.texture is RenderTexture)
            texture = (RenderTexture)m_targetImage.texture;

        if (texture == null)
        {
            ObjectHolder.gameObject.layer = LayerMask.NameToLayer(name);

            foreach (Transform item in ObjectHolder.transform)
                item.gameObject.layer = LayerMask.NameToLayer(name);

            m_camera.cullingMask = (1 << ObjectHolder.gameObject.layer) | (1 << LayerMask.NameToLayer("Objects"));

            texture = new RenderTexture(Mathf.Max(1, 200), Mathf.Max(1, 200), 24, RenderTextureFormat.ARGB32)
            {
                name = m_targetImage.name + " RenderTexture",
                filterMode = FilterMode.Point,
                antiAliasing = Mathf.Max(1, QualitySettings.antiAliasing)
            };
            m_camera.targetTexture = texture;
        }
        
        m_targetImage.texture = texture;
    }
}
