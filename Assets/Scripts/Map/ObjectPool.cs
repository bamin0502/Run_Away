using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly List<Transform> pool = new List<Transform>();

    public void InitializePool(Transform[] prefabs, int initialSize)
    {
        foreach (var prefab in prefabs)
        {
            for (var i = 0; i < initialSize; i++)
            {
                var instance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                instance.gameObject.SetActive(false);
                pool.Add(instance);
            }
        }
    }

    public Transform GetRandomPooledObject(Transform prefab)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].name.Contains(prefab.name) && !pool[i].gameObject.activeInHierarchy)
            {
                var obj = pool[i];
                pool.RemoveAt(i);
                return obj;
            }
        }

        var newInstance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        return newInstance;
    }

    public void DeactivateAndEnqueue(Transform tile, string spawnPointTag)
    {
        foreach (Transform child in tile)
        {
            if (child.CompareTag(spawnPointTag))
            {
                foreach (Transform element in child)
                {
                    element.gameObject.SetActive(false);
                    pool.Add(element);
                }
            }
        }
    }
}