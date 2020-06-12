using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Fordi.Core
{
    public class LoadingScene : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_title, m_loadingText;
        [SerializeField]
        private Image m_border1, m_border2;

        [SerializeField]
        private List<Shader> m_shaders;

        private IEnumerator Start()
        {
            yield return null;
            MinimumWindowSize.Set(1472, 828);
            yield return null;

#if !UNITY_EDITOR
            XRSettings.LoadDeviceByName("");
            yield return null;
            XRSettings.enabled = false;
#endif
            Color titleColor = m_title.color;
            Color clearTitleColor = new Color(titleColor.r, titleColor.g, titleColor.b, 0);
            m_title.color = clearTitleColor;
            m_border1.color = clearTitleColor;
            m_border2.color = clearTitleColor;
            yield return new WaitForSeconds(3);
            m_title.DOColor(titleColor, 1.5f);
            m_border1.DOColor(titleColor, 1.5f);
            m_border2.DOColor(titleColor, 1.5f);
            yield return new WaitForSeconds(2);
            m_title.DOColor(Color.clear, 1.5f);
            m_border1.DOColor(Color.clear, 1.5f);
            m_border2.DOColor(Color.clear, 1.5f);
            yield return new WaitForSeconds(1.5f);
            m_loadingText.gameObject.SetActive(true);
            yield return null;
            var ao = SceneManager.LoadSceneAsync("Home", LoadSceneMode.Single);
            ao.allowSceneActivation = false;
            while (ao.progress < .9f)
                yield return null;
            ao.allowSceneActivation = true;
        }
    }
}
