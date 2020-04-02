using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VRExperience.Common;
using VRExperience.Core;

namespace VRExperience.UI
{
    public class UIJoint : MonoBehaviour
    {
        [SerializeField]
        private Transform m_hand, m_uiHandle;

        [SerializeField]
        private LineRenderer m_line;

        [SerializeField]
        private int m_numberOfSegments = 45;

        [SerializeField]
        private OVRInput.Button m_button;

        [SerializeField]
        private TextMeshProUGUI m_content;

        [SerializeField]
        private OVRInput.Controller m_controller;

        public OVRInput.Button Button {get { return m_button; } }

        public OVRInput.Controller Controller {get { return m_controller; } }

        public Transform UIHandle { get { return m_uiHandle; } }

        private Vector3 m_offset;
        private bool m_offsetInitialized = false;
        private float m_archRadius;

        private float m_speed = 6;

        private IPlayer m_player;

        private ToolTip m_tooltip;
        public ToolTip Tooltip
        {
            get
            {
                return m_tooltip;
            }
            set
            {
                m_tooltip = value;
                DataBind();
            }
        }

        public void DataBind()
        {
            m_content.text = Tooltip.Tip;
        }

        private IEnumerator Start()
        {
            m_player = IOC.Resolve<IPlayer>();
            m_line.positionCount = m_numberOfSegments + 1;

            yield return null;

            m_offset = m_uiHandle.localPosition / 100;
            m_archRadius = Vector3.Magnitude(new Vector3(m_offset.x, 0, m_offset.z));

            m_uiHandle.SetParent(m_player.PlayerCanvas);
            m_uiHandle.localRotation = Quaternion.identity;

           
            m_offsetInitialized = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_offsetInitialized)
                return;


            float step = m_speed * Time.deltaTime; // calculate distance to move

            m_uiHandle.position = m_hand.position + m_offset;
            m_line.SetPosition(0, m_uiHandle.transform.position - new Vector3(0, ((RectTransform)m_uiHandle.transform).sizeDelta.y / 200, 0));

            // Check if the position of the cube and sphere are approximately equal.
            //if (Vector3.Distance(transform.position, m_hand.position + m_offset) < 0.001f)
            //{
            //    // Swap the position of the cylinder.
            //    target.position *= -1.0f;
            //}


            DrawArch();
            m_line.SetPosition(m_line.positionCount - 1, m_hand.transform.position);
            m_line.enabled = true;
        }


        private void DrawArch()
        {
            //Vector3 tempOffset = m_uiHandle.position - m_hand.position;

            //m_archRadius = Vector3.Magnitude(new Vector3(tempOffset.x, 0, tempOffset.z));

            Vector3 archEndPoint = new Vector3(m_uiHandle.position.x, m_hand.position.y + m_archRadius, m_uiHandle.position.z);

            Vector3 center = new Vector3(m_hand.position.x, m_hand.position.y + m_archRadius, m_hand.position.z);
            Vector3 groundDirection = Vector3.Normalize(new Vector3(m_offset.x, 0, m_offset.z));

            for (int i = 1; i < m_numberOfSegments; i++)
            {
                float angle = Mathf.PI * i / (2 * m_numberOfSegments);
                Vector3 offset = groundDirection * m_archRadius * Mathf.Cos(angle);
                Vector3 groundPoint = new Vector3(m_hand.position.x, 0, m_hand.position.z) + offset;
                Vector3 position = new Vector3(groundPoint.x, center.y - m_archRadius * Mathf.Sin(angle), groundPoint.z);
                if (position.y > m_uiHandle.position.y)
                    position.y = m_uiHandle.position.y;

                m_line.SetPosition(i, position);
            }
        }

        private void OnDestroy()
        {
            Destroy(m_uiHandle.gameObject);
        }
    }
}