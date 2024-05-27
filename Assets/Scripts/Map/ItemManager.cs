using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public GameObject primaryItemPrefab;
    public List<GameObject> otherItemPrefabs;

    private Queue<GameObject> itemPool = new Queue<GameObject>();
    private float totalDistance = 0f;
    private float lastSpecialItemSpawnDistance = 0f;
    public float specialItemSpawnDistance = 100f;
    public int coinLineLength = 5;
    public float coinSpacing = 3.0f;
    private Collider[] overlapResults = new Collider[10];

    public void SpawnItems(Transform tile, float totalDistance)
    {
        this.totalDistance = totalDistance;

        var bounds = tile.GetComponentInChildren<Collider>().bounds;
        float[] lanePositions = { -3.8f, 0f, 3.8f };

        foreach (var lane in lanePositions)
        {
            Vector3 startPosition = new Vector3(lane, bounds.min.y, bounds.min.z + Random.Range(0, bounds.size.z));
            if (!IsObstacleAtPosition(startPosition))
            {
                SpawnCoinLine(startPosition, lane, tile);
            }
        }

        if (totalDistance - lastSpecialItemSpawnDistance > specialItemSpawnDistance)
        {
            Vector3 randomPosition = new Vector3(lanePositions[Random.Range(0, lanePositions.Length)], bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));
            if (!IsObstacleAtPosition(randomPosition))
            {
                ReplaceCoinWithSpecialItem(tile);
                lastSpecialItemSpawnDistance = totalDistance;
            }
        }
    }

    private void SpawnCoinLine(Vector3 startPosition, float lane, Transform tile)
    {
        for (int i = 0; i < coinLineLength; i++)
        {
            Vector3 position = new Vector3(lane, startPosition.y, startPosition.z + i * coinSpacing);
            if (!IsObstacleAtPosition(position))
            {
                SpawnSingleItem(position, tile, true);
            }
        }
    }

    private void SpawnSingleItem(Vector3 position, Transform tile, bool isCoin)
    {
        var itemPrefab = isCoin ? primaryItemPrefab : otherItemPrefabs[Random.Range(0, otherItemPrefabs.Count)];

        GameObject item;
        if (itemPool.Count > 0)
        {
            item = itemPool.Dequeue();
            item.transform.position = position;
            item.transform.rotation = Quaternion.identity;
            item.SetActive(true);
        }
        else
        {
            item = Instantiate(itemPrefab, position, Quaternion.identity);
        }

        item.transform.SetParent(tile, true);
    }

    private void ReplaceCoinWithSpecialItem(Transform tile)
    {
        foreach (Transform child in tile)
        {
            if (child.CompareTag("Item") && child.gameObject.name.Contains("Coin"))
            {
                var specialItemPrefab = otherItemPrefabs[Random.Range(0, otherItemPrefabs.Count)];
                Destroy(child.gameObject);
                var specialItem = Instantiate(specialItemPrefab, child.position, Quaternion.identity);
                specialItem.transform.SetParent(tile, true);
                break;
            }
        }
    }

    private bool IsObstacleAtPosition(Vector3 position)
    {
        int numColliders = Physics.OverlapSphereNonAlloc(position, 1f, overlapResults);
        for (int i = 0; i < numColliders; i++)
        {
            if (overlapResults[i].CompareTag("Obstacle"))
            {
                return true;
            }
        }
        return false;
    }

    public void RecycleItem(GameObject item)
    {
        item.SetActive(false);
        itemPool.Enqueue(item);
    }
}
