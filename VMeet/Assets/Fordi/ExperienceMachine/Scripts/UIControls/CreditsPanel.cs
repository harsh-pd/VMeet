using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace Fordi.UI
{
    public class CreditsPanel : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_content;
        [SerializeField]
        [Range(0, 1)]
        private float m_textSpeed = .5f;

        private Vector3? m_initialPosition = null;
        private Tween m_tweener = null;

        private IEnumerator Start()
        {
            yield return null;
            yield return null;
            m_initialPosition = m_content.rectTransform.position;
        }


        private void OnEnable()
        {
            if (m_tweener != null)
                m_tweener.Kill()
;           
            if (m_initialPosition != null)
                m_content.rectTransform.position = (Vector3)m_initialPosition;

            var contentHeigt = m_content.preferredHeight;
            m_tweener = m_content.rectTransform.DOMove(m_content.rectTransform.position + new Vector3(0, contentHeigt, 0), 15000/(1 + 2 * m_textSpeed));
        }
    }
}
