using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private List<GameObject> itemPrefabs;
    private GameObject primaryItemPrefab;
    private List<GameObject> otherItemPrefabs;
    public Queue<GameObject> itemPool = new Queue<GameObject>();

    public float specialItemSpawnDistance = 100f;
    public float initialOtherItemSpawnChance = 0.05f;
    public float maxOtherItemSpawnChance = 0.2f;
    public float distanceFactor = 0.0005f;
    public int coinLineLength = 5;
    public float coinSpacing = 3.0f;

    private float totalDistance = 0f;
    private bool isFirstItem = true;

    void Awake()
    {
        itemPrefabs = DataManager.GetItemTable().GetLoadedItems("Item");
        primaryItemPrefab = itemPrefabs.Find(item => item.name == "Coin");
        otherItemPrefabs = itemPrefabs.FindAll(item => item.name != "Coin");
    }

    public void UpdateDistance(float distance)
    {
        totalDistance += distance;
    }

    public void SpawnItems(Transform tile)
    {
        if (tile == null)
        {
            return;
        }

        var bounds = tile.GetComponentInChildren<Collider>().bounds;
        float[] lanePositions = { -3.8f, 0f, 3.8f };

        foreach (var lane in lanePositions)
        {
            for (int attempts = 0; attempts < 10; attempts++)
            {
                var randomPosition = new Vector3(lane, bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));

                if (!IsObstacleAtPosition(randomPosition))
                {
                    if (isFirstItem || Random.value < initialOtherItemSpawnChance + (totalDistance * distanceFactor))
                    {
                        SpawnSingleItem(randomPosition, tile);
                    }
                    else
                    {
                        SpawnCoinLine(randomPosition, lane, tile);
                    }
                    isFirstItem = false;
                    break;
                }
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

    private void SpawnSingleItem(Vector3 position, Transform tile, bool isCoin = false)
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
        if (tile)
        {
            item.transform.SetParent(tile, true);
        }
    }

    private bool IsObstacleAtPosition(Vector3 position)
    {
        var overlapResults = new Collider[10];
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
}
