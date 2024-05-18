using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public Transform playerTransform;
    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public int noObstaclesInitially = 2;
    public float tileLength = 17;
    public float moveSpeed;

    private List<Transform> tiles = new List<Transform>();
    private Vector3 nextTilePosition;
    private GameManager gameManager;
    private ItemManager itemManager;
    private ObstacleManager obstacleManager;

    private List<GameObject> sectionPrefabs;

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        itemManager = GetComponent<ItemManager>();
        obstacleManager = GetComponent<ObstacleManager>();

        var sectionTable = DataManager.GetSectionTable();
        sectionPrefabs = sectionTable.GetLoadedSections("Tile");
    }

    private void Start()
    {
        nextTilePosition = startPoint;
        for (var i = 0; i < numberOfTiles; i++)
        {
            var tile = SpawnTile(i >= noObstaclesInitially);
            if (tile != null)
            {
                itemManager.SpawnItems(tile);
            }
        }
    }

    private void Update()
    {
        if (!gameManager.isGameover)
        {
            moveSpeed = gameManager.stageSpeed;
            MoveTiles();
            if (tiles.Count > 0 && tiles[0].position.z < playerTransform.position.z - 50)
            {
                ReuseTile();
            }
        }
    }

    private void MoveTiles()
    {
        foreach (var tile in tiles)
        {
            tile.Translate(-Vector3.forward * (moveSpeed * Time.deltaTime), Space.World);
        }
    }

    private Transform SpawnTile(bool spawnObstacles)
    {
        if (sectionPrefabs.Count == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Section prefabs are not assigned.");
#endif
            return null;
        }

        var sectionPrefab = sectionPrefabs[Random.Range(0, sectionPrefabs.Count)];
        var newTile = Instantiate(sectionPrefab, nextTilePosition, Quaternion.identity, transform).transform;
        if (newTile == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Failed to instantiate section prefab.");
#endif
            return null;
        }

        nextTilePosition += new Vector3(0, 0, tileLength);
        tiles.Add(newTile);

        if (spawnObstacles)
        {
            var sectionTypeComponent = newTile.GetComponent<SectionType>();
            if (sectionTypeComponent != null)
            {
                obstacleManager.SpawnObstacles(newTile, sectionTypeComponent.sectionType);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("SectionType component not found on the section prefab.");
#endif
            }
        }

        return newTile;
    }

    private void ReuseTile()
    {
        var tile = tiles[0];
        tiles.RemoveAt(0);
        tile.position = tiles[^1].position + new Vector3(0, 0, tileLength);
        tiles.Add(tile);

        var sectionTypeComponent = tile.GetComponent<SectionType>();
        if (sectionTypeComponent != null)
        {
            obstacleManager.SpawnObstacles(tile, sectionTypeComponent.sectionType);
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning("SectionType component not found on the section prefab.");
        }
#endif

        itemManager.SpawnItems(tile);
    }
}
