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

    public Transform GetRandomPooledObject(Transform[] prefabs)
    {
        if (pool.Count > 0)
        {
            var randomIndex = Random.Range(0, pool.Count);
            var obj = pool[randomIndex];
            pool.RemoveAt(randomIndex);
            return obj;
        }
        else
        {
            var prefab = prefabs[Random.Range(0, prefabs.Length)];
            return Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }
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