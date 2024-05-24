using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly List<Transform> pool = new List<Transform>();

    public void InitializePool(Transform prefab, int initialSize)
    {
        for (var i = 0; i < initialSize; i++)
        {
            var instance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            instance.gameObject.SetActive(false);
            pool.Add(instance);
        }
    }

    public Transform GetRandomPooledObject(Transform prefab)
    {
        if (pool.Count > 0)
        {
            var obj = pool[0];
            pool.RemoveAt(0);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            return Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }
    }

    public void DeactivateAndEnqueue(Transform tile)
    {
        tile.gameObject.SetActive(false);
        pool.Add(tile);
    }
}