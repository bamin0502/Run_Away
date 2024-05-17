using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public Transform[] groundPrefabs;
    public Transform[] backgroundPrefabs;
    public int noObstaclesInitially = 2;
    public float initialOtherItemSpawnChance = 0.05f;
    public float maxOtherItemSpawnChance = 0.2f;
    public float distanceFactor = 0.0005f;

    private List<GameObject> itemPrefabs;
    private GameObject primaryItemPrefab;
    private List<GameObject> otherItemPrefabs;

    private Vector3 previousItemPosition = Vector3.zero;
    private bool isFirstItem = true;
    private float distanceCounter = 0f;

    private Collider[] overlapResults = new Collider[10];

    void Awake()
    {
        var itemTable = DataManager.GetItemTable();
        itemPrefabs = itemTable.GetLoadedItems("Item");

        primaryItemPrefab = itemPrefabs.Find(item => item.name == "Coin");
        otherItemPrefabs = itemPrefabs.FindAll(item => item.name != "Coin");
    }

    public void SpawnItems(Transform tile)
    {
        var bounds = tile.GetComponent<Collider>().bounds;
        float[] lanePositions = { -3.8f, 0f, 3.8f };
        int laneCount = lanePositions.Length;

        Vector3 currentItemPosition = previousItemPosition;
        int currentLaneIndex = 0;
        float itemSpacing = 5f;

        while (currentItemPosition.z < bounds.max.z)
        {
            currentItemPosition.z += itemSpacing;

            if (IsObstacleAtPosition(currentItemPosition))
            {
                currentLaneIndex = (currentLaneIndex + 1) % laneCount;
                currentItemPosition.x = lanePositions[currentLaneIndex];
                continue;
            }

            if (!isFirstItem && Mathf.Approximately(previousItemPosition.z, currentItemPosition.z))
            {
                Vector3 midPosition = (previousItemPosition + currentItemPosition) / 2;
                midPosition.y = bounds.min.y;
                SpawnSingleItem(midPosition, tile);
            }

            currentItemPosition.x = lanePositions[currentLaneIndex];
            currentItemPosition.y = bounds.min.y;
            SpawnSingleItem(currentItemPosition, tile);

            previousItemPosition = currentItemPosition;
            isFirstItem = false;
        }
    }

    private void SpawnSingleItem(Vector3 position, Transform tile)
    {
        GameObject itemPrefab;
        float currentDistance = Vector3.Distance(Vector3.zero, position);

        float otherItemSpawnChance = initialOtherItemSpawnChance + (currentDistance * distanceFactor);
        otherItemSpawnChance = Mathf.Clamp(otherItemSpawnChance, initialOtherItemSpawnChance, maxOtherItemSpawnChance);

        if (Random.value < otherItemSpawnChance)
        {
            itemPrefab = otherItemPrefabs[Random.Range(0, otherItemPrefabs.Count)];
        }
        else
        {
            itemPrefab = primaryItemPrefab;
        }

        var item = Instantiate(itemPrefab, position, Quaternion.identity);
        item.transform.SetParent(tile,true); 
        //item.transform.localScale = itemPrefab.transform.localScale; 
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
