using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
    [SerializeField]
    private GameObject m_object;

    public GameObject Object { get { return m_object; } set { m_object = value; } }
}
