using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private GameObject primaryItemPrefab;
    private List<GameObject> otherItemPrefabs;
    private ObjectPool itemPool;

    public float initialOtherItemSpawnChance = 0.05f;
    public float maxOtherItemSpawnChance = 0.2f;
    public float distanceFactor = 0.0005f;

    private Vector3 previousItemPosition = Vector3.zero;
    private bool isFirstItem = true;
    private Collider[] overlapResults = new Collider[10];

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        var itemTable = DataManager.GetItemTable();
        var itemPrefabs = itemTable.GetLoadedItems("Item");

        primaryItemPrefab = itemPrefabs.Find(item => item.name == "Coin");
        otherItemPrefabs = itemPrefabs.FindAll(item => item.name != "Coin");

        itemPool = new ObjectPool();
        itemPool.InitializePool(itemPrefabs.ConvertAll(item => item.transform).ToArray(), 10);
    }

    public void SpawnItems(Transform tile)
    {
        var bounds = tile.GetComponentInChildren<Collider>().bounds;
        float[] lanePositions = { -3.8f, 0f, 3.8f };

        foreach (var lane in lanePositions)
        {
            for (int attempts = 0; attempts < 10; attempts++)
            {
                var randomPosition = new Vector3(lane, bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));

                if (!IsObstacleAtPosition(randomPosition))
                {
                    if (!isFirstItem && Mathf.Approximately(previousItemPosition.z, randomPosition.z))
                    {
                        SpawnSingleItem((previousItemPosition + randomPosition) / 2, tile);
                    }

                    SpawnSingleItem(randomPosition, tile);
                    previousItemPosition = randomPosition;
                    isFirstItem = false;
                    break;
                }
            }
        }
    }

    private void SpawnSingleItem(Vector3 position, Transform tile)
    {
        GameObject itemPrefab;
        float currentDistance = Vector3.Distance(Vector3.zero, position);

        float otherItemSpawnChance = Mathf.Clamp(initialOtherItemSpawnChance + (currentDistance * distanceFactor), initialOtherItemSpawnChance, maxOtherItemSpawnChance);

        if (Random.value < otherItemSpawnChance)
        {
            itemPrefab = otherItemPrefabs[Random.Range(0, otherItemPrefabs.Count)];
        }
        else
        {
            itemPrefab = primaryItemPrefab;
        }

        var itemTransform = itemPool.GetRandomPooledObject(itemPrefab.transform);
        itemTransform.position = position;
        itemTransform.rotation = Quaternion.identity;
        itemTransform.gameObject.SetActive(true);
        itemTransform.SetParent(tile, true);
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
}
