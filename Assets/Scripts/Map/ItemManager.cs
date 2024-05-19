using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private List<GameObject> itemPrefabs;
    private GameObject primaryItemPrefab; // 코인
    private List<GameObject> specialItemPrefabs = new List<GameObject>(); // 특별 아이템
    public float specialItemInterval = 100f; // 특별 아이템 스폰 간격

    private float distanceCounter = 0f;
    private Vector3 previousItemPosition = Vector3.zero;
    private bool isFirstItem = true;

    private Collider[] overlapResults = new Collider[10];

    void Awake()
    {
        var itemTable = DataManager.GetItemTable();
        itemPrefabs = itemTable.GetLoadedItems("Item");

        primaryItemPrefab = itemPrefabs.Find(item => item.name == "Coin");
        specialItemPrefabs = itemPrefabs.FindAll(item => item.name != "Coin");
    }

    public void SpawnItems(Transform tile)
    {
        var bounds = tile.GetComponentInChildren<Collider>().bounds;
        float[] lanePositions = { -3.8f, 0f, 3.8f };
        int laneIndex = 0;

        for (float z = bounds.min.z; z < bounds.max.z; z += 1f)
        {
            var position = new Vector3(lanePositions[laneIndex], bounds.min.y, z);
            if (!IsObstacleAtPosition(position))
            {
                SpawnSingleItem(position, tile);
                laneIndex = (laneIndex + 1) % lanePositions.Length;
            }
        }
    }

    private void SpawnSingleItem(Vector3 position, Transform tile)
    {
        GameObject itemPrefab;
        distanceCounter += Vector3.Distance(previousItemPosition, position);

        if (distanceCounter >= specialItemInterval)
        {
            if (specialItemPrefabs.Count > 0)
            {
                itemPrefab = specialItemPrefabs[Random.Range(0, specialItemPrefabs.Count)];
                distanceCounter = 0f;
            }
            else
            {
                itemPrefab = primaryItemPrefab;
            }
        }
        else
        {
            itemPrefab = primaryItemPrefab;
        }

        var item = Instantiate(itemPrefab, position, Quaternion.identity);
        item.transform.SetParent(tile, true);
        previousItemPosition = position;
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
