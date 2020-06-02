using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fordi.Core
{
    public class ExperienceObject
    {
        public RawImage Preview;
        public GameObject ObjectPrefab;
    }

    public class ObjectGrid : MonoBehaviour
    {
        [SerializeField]
        private ObjectHolder m_objectHolderPrefab;

        [SerializeField]
        private ObjectCamera m_objectCameraPrefab;

        [SerializeField]
        private Vector2Int m_size = new Vector2Int(4, 3);

        [SerializeField]
        private Vector2 m_stretch;

        private List<ObjectHolder> m_objects = new List<ObjectHolder>();

        private ExperienceObject[] m_experienceObjects;
        public ExperienceObject[] ExperienceObjects
        {
            get
            {
                return m_experienceObjects;
            }

            set
            {
                m_experienceObjects = value;
                DataBind();
            }
        }

        private void DataBind()
        {
            foreach (var item in m_objects)
                if (item != null)
                    Destroy(item.gameObject);

            transform.DetachChildren();

            m_objects.Clear();

            foreach (var item in m_experienceObjects)
                Add(item);
        }

        public void Add(ExperienceObject experienceObject)
        {
            if (experienceObject.ObjectPrefab == null)
            {
                Debug.LogError("ObjectPrefab null");
                return;
            }
            
            if (m_size.x == 0 || m_size.y == 0)
                throw new InvalidOperationException("Object grid size can not be 0");

            var objectHolder = Instantiate(m_objectHolderPrefab, transform);
            GameObject obj = Instantiate(experienceObject.ObjectPrefab, objectHolder.transform);
            objectHolder.Object = obj;

            int xIndex = m_objects.Count % m_size.x;
            int yIndex = m_objects.Count / m_size.x;

            objectHolder.transform.localPosition = new Vector3(-m_stretch.x / 2 + m_stretch.x * xIndex / (m_size.x - 1), objectHolder.transform.localPosition.y + m_stretch.y / 2 - m_stretch.y * yIndex / (m_size.y - 1), objectHolder.transform.localPosition.z);
            m_objects.Add(objectHolder);

            var objectCamera = Instantiate(m_objectCameraPrefab);

            objectCamera.name = "" + m_objects.Count % (m_size.x * m_size.y);
            objectCamera.TargetImage = experienceObject.Preview;
            objectCamera.ObjectHolder = objectHolder;
        }
    }
}
