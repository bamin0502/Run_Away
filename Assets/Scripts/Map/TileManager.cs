using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform playerTransform;
    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public float tileLength = 17;
    public float moveSpeed;

    private readonly List<Transform> tiles = new List<Transform>();
    private Vector3 nextTilePosition;

    private GameManager gameManager;
    private ItemManager itemManager;
    private ObstacleManager obstacleManager;
    private ObjectPool groundPool;
    private ObjectPool backgroundPool;

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        itemManager = GetComponent<ItemManager>();
        obstacleManager = GetComponent<ObstacleManager>();

        groundPool = new ObjectPool();
        backgroundPool = new ObjectPool();

        groundPool.InitializePool(itemManager.groundPrefabs, 10);
        backgroundPool.InitializePool(itemManager.backgroundPrefabs, 10);
    }

    private void Start()
    {
        nextTilePosition = startPoint;
        for (var i = 0; i < numberOfTiles; i++)
        {
            var tile = SpawnTile(i >= itemManager.noObstaclesInitially);
            itemManager.SpawnItems(tile);
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
        var newTile = Instantiate(tilePrefab, nextTilePosition, Quaternion.identity, transform);
        nextTilePosition += new Vector3(0, 0, tileLength);
        tiles.Add(newTile);

        if (spawnObstacles)
        {
            obstacleManager.SpawnObstacles(newTile);
        }

        SpawnGroundElements(newTile);
        SpawnBackgroundElements(newTile);

        return newTile;
    }

    private void ReuseTile()
    {
        var tile = tiles[0];
        tiles.RemoveAt(0);
        var endPoint = tiles[^1].position + new Vector3(0, 0, tileLength);
        tile.position = endPoint;
        tiles.Add(tile);

        groundPool.DeactivateAndEnqueue(tile, "SmallSpawnPoint");
        backgroundPool.DeactivateAndEnqueue(tile, "TallSpawnPoint");

        SpawnGroundElements(tile);
        SpawnBackgroundElements(tile);
        itemManager.SpawnItems(tile);
    }

    private void SpawnGroundElements(Transform tile)
    {
        var groundPoints = GetChildTransformsWithTag(tile, "SmallSpawnPoint");

        foreach (var point in groundPoints)
        {
            if (itemManager.groundPrefabs.Length > 0 && Random.Range(0f, 1f) <= 0.7f)
            {
                var groundElement = groundPool.GetRandomPooledObject(itemManager.groundPrefabs);
                groundElement.position = point.position;
                groundElement.rotation = point.rotation;
                groundElement.gameObject.SetActive(true);
                groundElement.SetParent(point);  // SetParent의 두 번째 매개변수를 false로 설정하여 로컬 스케일 유지
            }
        }
    }

    private void SpawnBackgroundElements(Transform tile)
    {
        var backgroundPoints = GetChildTransformsWithTag(tile, "TallSpawnPoint");

        foreach (var point in backgroundPoints)
        {
            if (itemManager.backgroundPrefabs.Length > 0 && Random.Range(0f, 1f) <= 0.7f)
            {
                var backgroundElement = backgroundPool.GetRandomPooledObject(itemManager.backgroundPrefabs);
                backgroundElement.position = point.position;
                backgroundElement.rotation = point.rotation;
                backgroundElement.gameObject.SetActive(true);
                backgroundElement.SetParent(point);  // SetParent의 두 번째 매개변수를 기본값으로 하여 부모의 스케일 영향을 받도록 설정
            }
        }
    }

    private List<Transform> GetChildTransformsWithTag(Transform parent, string tag)
    {
        var result = new List<Transform>();
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                result.Add(child);
            }
        }
        return result;
    }
}
