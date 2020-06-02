
using UnityEngine;
using System.Collections; // required for Coroutines
using Fordi.Core;
using Fordi.Common;

public class VRFadeScript : MonoBehaviour
{

    public float fadeTime = 2.0f;
    Material fadeMaterial = null;
    bool isFading = false;
    YieldInstruction fadeInstruction = new WaitForEndOfFrame();

    private ISettings m_settings;

    void Awake()
    {
        m_settings = IOC.Resolve<ISettings>();
        // create the fade material
        fadeMaterial = new Material(Shader.Find("Unlit/TransparentColor"));
    }


    void OnEnable()
    {
        StartCoroutine(FadeIn());
    }
 
    void OnDestroy()
    {
        if (fadeMaterial != null)
        {
            Destroy(fadeMaterial);
        }
    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0.0f;
        fadeMaterial.color = m_settings.SelectedPreferences.FadeColor;
        Color color = m_settings.SelectedPreferences.FadeColor;
        isFading = true;
        while (elapsedTime < fadeTime)
        {
            yield return fadeInstruction;
            elapsedTime += Time.deltaTime;
            color.a = 1.0f - Mathf.Clamp01(elapsedTime / fadeTime);
            fadeMaterial.color = color;
        }
        isFading = false;
    }

    IEnumerator FadeIn(float time)
    {
        float elapsedTime = 0.0f;
        fadeMaterial.color = m_settings.SelectedPreferences.FadeColor;
        Color color = m_settings.SelectedPreferences.FadeColor;
        isFading = true;
        while (elapsedTime < time)
        {
            yield return fadeInstruction;
            elapsedTime += Time.deltaTime;
            color.a = 1.0f - Mathf.Clamp01(elapsedTime / time);
            fadeMaterial.color = color;
        }
        isFading = false;
    }

    IEnumerator FadeOut(float time)
    {
        float elapsedTime = 0.0f;
        fadeMaterial.color = m_settings.SelectedPreferences.FadeColor;
        Color color = m_settings.SelectedPreferences.FadeColor;
        color.a = 0;
        fadeMaterial.color = color;
        color.a = 1;
        isFading = true;
        while (elapsedTime < time)
        {
            yield return fadeInstruction;
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / time);
            Debug.Log(color.a);
            fadeMaterial.color = color;
        }
        isFading = false;
    }

    public void StartFadeIn(float time)
    {
        StartCoroutine(FadeIn(time));
    }

    public void StartFadeOut(float time)
    {
        StartCoroutine(FadeOut(time));
    }

    void OnPostRender()
    {
        if (isFading)
        {
            fadeMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Color(fadeMaterial.color);
            GL.Begin(GL.QUADS);
            GL.Vertex3(0f, 0f, -12f);
            GL.Vertex3(0f, 1f, -12f);
            GL.Vertex3(1f, 1f, -12f);
            GL.Vertex3(1f, 0f, -12f);
            GL.End();
            GL.PopMatrix();
        }
    }
}
