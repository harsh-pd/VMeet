using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi
{
    public interface IResettable
    {
        void OnReset();
    }

    public enum Platform
    {
        DESKTOP = 0,
        VR = 1
    }

    public class Pool<T> where T : MonoBehaviour, IResettable
    {
        List<T> availableItems = new List<T>();
        Transform itemRoot;
        GameObject itemPrefab;

        public Pool(Transform _itemRoot, GameObject _itemPrefab)
        {
            itemRoot = _itemRoot;
            itemPrefab = _itemPrefab;
        }

        public T FetchItem()
        {
            if (availableItems.Count > 0)
            {
                T itemToGive = availableItems[availableItems.Count - 1];
                availableItems.Remove(availableItems[availableItems.Count - 1]);
                itemToGive.gameObject.SetActive(true);
                return itemToGive;
            }
            else
            {
                var obj = GameObject.Instantiate(itemPrefab, itemRoot);
                return obj.GetComponent<T>();
            }
        }

        public void Retrieve(T item)
        {
            item.gameObject.SetActive(true);
            availableItems.Remove(item);
        }

        public void Surrender(T item)
        {
            item.gameObject.SetActive(false);
            item.OnReset();
            availableItems.Add(item);
        }
    }
}
