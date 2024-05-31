using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public Transform playerTransform;
    public Transform tileManagerTransform;
    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public float tileLength = 17;

    private List<Transform> tiles = new List<Transform>();
    private Vector3 nextTilePosition;

    private TileMovement tileMovement;
    private TileSpawner tileSpawner;
    private ItemManager itemManager;
    private float totalDistance = 0f;

    private void Awake()
    {
        tileMovement = GetComponent<TileMovement>();
        tileSpawner = GetComponent<TileSpawner>();
        itemManager = FindObjectOfType<ItemManager>();
    }

    public void Initialize()
    {
        nextTilePosition = startPoint;

        for (var i = 0; i < numberOfTiles; i++)
        {
            var tile = tileSpawner.SpawnTile(nextTilePosition, tileManagerTransform, i >= 2);
            if (tile != null)
            {
                nextTilePosition += new Vector3(0, 0, tileLength);
                tiles.Add(tile);
            }
        }

        tileMovement.InitializeMovement();
    }

    public void UpdateTiles(float moveSpeed)
    {
        tileMovement.MoveTiles(tiles, moveSpeed);

        if (tiles.Count > 0 && tiles[0].position.z < playerTransform.position.z - 50)
        {
            tileSpawner.ReuseTile(tiles, ref nextTilePosition, tileLength, ref totalDistance);
        }
    }

    public void SpawnItems(Transform tile)
    {
        itemManager.SpawnItems(tile, totalDistance);
    }
}