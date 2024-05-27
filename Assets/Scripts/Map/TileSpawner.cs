using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    public int noObstaclesInitially = 2;
    public int noItemsInitially = 2;
    public Queue<GameObject> itemPool = new Queue<GameObject>();
    public Queue<GameObject> obstaclePool = new Queue<GameObject>();

    private List<GameObject> sectionPrefabs;
    private ObstacleManager obstacleManager;
    private TileManager tileManager;

    private void Awake()
    {
        sectionPrefabs = DataManager.GetSectionTable().GetLoadedSections("Tile");
        obstacleManager = GetComponent<ObstacleManager>();
        tileManager = GetComponent<TileManager>();
    }

    public Transform SpawnTile(Vector3 position, Transform parent, bool spawnObstacles)
    {
        if (sectionPrefabs.Count == 0) return null;

        var sectionPrefab = sectionPrefabs[Random.Range(0, sectionPrefabs.Count)];
        var newTile = Instantiate(sectionPrefab, position, Quaternion.identity, parent).transform;
        if (newTile == null) return null;

        if (spawnObstacles)
        {
            var sectionTypeComponent = newTile.GetComponent<SectionType>();
            if (sectionTypeComponent != null)
            {
                obstacleManager.SpawnObstacles(newTile, sectionTypeComponent.sectionType);
            }
        }

        tileManager.SpawnItems(newTile); // 아이템 스폰

        return newTile;
    }

    public void ReuseTile(List<Transform> tiles, ref Vector3 nextTilePosition, float tileLength, ref float totalDistance)
    {
        var tile = tiles[0];
        tiles.RemoveAt(0);
        tiles.Add(tile);

        foreach (Transform child in tile)
        {
            if (child.CompareTag("Obstacle"))
            {
                obstacleManager.RecycleObstacle(child.gameObject);
            }
            else if (child.CompareTag("Item"))
            {
                itemPool.Enqueue(child.gameObject);
                child.gameObject.SetActive(false);
            }
        }

        tile.position = nextTilePosition;
        nextTilePosition += new Vector3(0, 0, tileLength);

        var newSectionPrefab = sectionPrefabs[Random.Range(0, sectionPrefabs.Count)];
        var newSectionTypeComponent = newSectionPrefab.GetComponent<SectionType>();

        if (newSectionTypeComponent)
        {
            tile.GetComponent<SectionType>().sectionType = newSectionTypeComponent.sectionType;
            obstacleManager.SpawnObstacles(tile, newSectionTypeComponent.sectionType);
        }

        tileManager.SpawnItems(tile); // 아이템 스폰
    }
}
