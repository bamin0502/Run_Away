using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public Transform playerTransform;
    public Transform tileManagerTransform;
    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public float tileLength = 17;
    public int noObstaclesInitially = 2;
    public int noItemsInitially = 2;

    private List<Transform> tiles = new List<Transform>();
    private Vector3 nextTilePosition;

    private ObstacleManager obstacleManager;
    private ItemManager itemManager;

    void Awake()
    {
        obstacleManager = GetComponent<ObstacleManager>();
        itemManager = GetComponent<ItemManager>();
    }

    public void InitializeTiles()
    {
        nextTilePosition = startPoint;
        for (var i = 0; i < numberOfTiles; i++)
        {
            var tile = SpawnTile(i >= noObstaclesInitially);
            if (tile != null && i >= noItemsInitially)
            {
                itemManager.SpawnItems(tile);
            }
        }
    }

    public void MoveTiles(float moveSpeed)
    {
        foreach (var tile in tiles)
        {
            tile.Translate(-Vector3.forward * (moveSpeed * Time.deltaTime), Space.World);
        }

        if (tiles.Count > 0 && tiles[0].position.z < playerTransform.position.z - 50)
        {
            ReuseTile();
        }
    }

    private Transform SpawnTile(bool spawnObstacles)
    {
        var sectionPrefab = obstacleManager.GetRandomSectionPrefab();
        var newTile = Instantiate(sectionPrefab, nextTilePosition, Quaternion.identity, tileManagerTransform).transform;
        nextTilePosition += new Vector3(0, 0, tileLength);
        tiles.Add(newTile);

        if (spawnObstacles)
        {
            obstacleManager.SpawnObstacles(newTile);
        }

        return newTile;
    }

    private void ReuseTile()
    {
        var tile = tiles[0];
        tiles.RemoveAt(0);
        tiles.Add(tile);

        tile.position = tiles[^2].position + new Vector3(0, 0, tileLength);
        obstacleManager.ResetTileObstacles(tile);
        itemManager.SpawnItems(tile);
    }
}
