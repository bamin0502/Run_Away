using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Transforms"), Tooltip("The transforms of the tile, obstacle, and player.")]
    public Transform tilePrefab;
    public Transform playerTransform;

    [Header("BackGround"), Tooltip("The transforms of the ground and background.")]
    public Transform[] groundPrefabs;
    public Transform[] backgroundPrefabs;

    [Header("Tile Settings"), Tooltip("The tile prefab to spawn.")]
    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public int noObstaclesInitially = 2;
    public float tileLength = 30.0f;
    public float moveSpeed;

    private List<Transform> tiles = new List<Transform>();
    private Vector3 nextTilePosition;

    private GameManager gameManager;
    
    private List<Transform> groundPool = new List<Transform>();
    private List<Transform> backgroundPool = new List<Transform>();

    private List<GameObject> obstaclePrefabs;

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        
        InitializeObjectPool(groundPrefabs, groundPool, 10);
        InitializeObjectPool(backgroundPrefabs, backgroundPool, 10);

        // DataManager를 통해 장애물 로드
        var obstacleTable = DataManager.GetObstacleTable();
        obstaclePrefabs = obstacleTable.GetLoadedObstacles("Obstacle");

        Debug.Log($"Loaded {obstaclePrefabs.Count} obstacle prefabs.");
        
    }

    void Start()
    {
        nextTilePosition = startPoint;
        for (var i = 0; i < numberOfTiles; i++)
        {
            SpawnTile(i >= noObstaclesInitially);
        }
        CheckObstacleColliders();
    }
 
    void Update()
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

    private void SpawnTile(bool spawnObstacles)
    {
        if (tilePrefab == null)
        {
            Debug.LogWarning("Tile prefab is not assigned.");
            return;
        }

        var newTile = Instantiate(tilePrefab, nextTilePosition, Quaternion.identity, transform);
        if (newTile == null)
        {
            Debug.LogWarning("Failed to instantiate tile prefab.");
            return;
        }

        var tileEndPoint = newTile.GetComponent<TileEndPoint>();
        if (tileEndPoint == null)
        {
            Debug.LogWarning("TileEndPoint component is missing on the tile prefab.");
            return;
        }

        if (tileEndPoint.endPoint == null)
        {
            Debug.LogWarning("EndPoint is not assigned in TileEndPoint component.");
            return;
        }

        nextTilePosition = tileEndPoint.endPoint.position;
        tiles.Add(newTile);

        if (spawnObstacles)
        {
            SpawnObstacles(newTile);
        }

        SpawnGroundElements(newTile);
        SpawnBackgroundElements(newTile);
    }

    private void CheckObstacleColliders()
    {
        foreach (var obstaclePrefab in obstaclePrefabs)
        {
            var collider = obstaclePrefab.GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogWarning($"Obstacle prefab {obstaclePrefab.name} does not have a Collider component.");
            }
            else
            {
                Debug.Log($"Obstacle prefab {obstaclePrefab.name} Collider found. Tag: {collider.tag}, isTrigger: {collider.isTrigger}");
            }
        }
    }

    private void SpawnObstacles(Transform tile)
    {
        var spawnPoints = new List<Transform>();
        foreach (Transform child in tile)
        {
            if (child.CompareTag("ObstacleSpawnPoint"))
            {
                spawnPoints.Add(child);
            }
        }

        var emptyIndex = Random.Range(0, spawnPoints.Count);

        for (var i = 0; i < spawnPoints.Count; i++)
        {
            if (i != emptyIndex && obstaclePrefabs.Count > 0)
            {
                var spawnPoint = spawnPoints[i];
                if (Random.Range(0, 2) == 0)
                {
                    var obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
                    var obstacle = Instantiate(obstaclePrefab, spawnPoint.position, Quaternion.identity);

                    // 장애물을 타일의 자식으로 설정
                    obstacle.transform.SetParent(tile);
                    obstacle.transform.localPosition = spawnPoint.localPosition;
                    obstacle.transform.localRotation = spawnPoint.localRotation;
                    obstacle.transform.localScale = Vector3.one; // 스케일 고정

                    // 콜라이더 검사 추가
                    var collider = obstacle.GetComponent<Collider>();
                    if (collider == null)
                    {
                        Debug.LogWarning($"Obstacle {obstacle.name} does not have a Collider component.");
                    }
                    else
                    {
                        Debug.Log($"Obstacle {obstacle.name} Collider found. Tag: {collider.tag}, isTrigger: {collider.isTrigger}");
                    }
                }
            }
        }
    }

    private void SpawnGroundElements(Transform tile)
    {
        var groundPoints = new List<Transform>();
        foreach (Transform child in tile)
        {
            if (child.CompareTag("SmallSpawnPoint"))
            {
                groundPoints.Add(child);
            }
        }

        foreach (var point in groundPoints)
        {
            if (groundPrefabs.Length > 0 && Random.Range(0f, 1f) <= 0.7f)
            {
                var groundElement = GetRandomPooledObject(groundPool, groundPrefabs);
                groundElement.position = point.position;
                groundElement.rotation = point.rotation;
                groundElement.gameObject.SetActive(true);
                groundElement.SetParent(point);
            }
        }
    }

    private void SpawnBackgroundElements(Transform tile)
    {
        var backgroundPoints = new List<Transform>();
        foreach (Transform child in tile)
        {
            if (child.CompareTag("TallSpawnPoint"))
            {
                backgroundPoints.Add(child);
            }
        }

        foreach (var point in backgroundPoints)
        {
            if (backgroundPrefabs.Length > 0 && Random.Range(0f, 1f) <= 0.7f)
            {
                var backgroundElement = GetRandomPooledObject(backgroundPool, backgroundPrefabs);
                backgroundElement.position = point.position;
                backgroundElement.rotation = point.rotation;
                backgroundElement.gameObject.SetActive(true);
                backgroundElement.SetParent(point);
            }
        }
    }

    private void ReuseTile()
    {
        var tile = tiles[0];
        tiles.RemoveAt(0);
        var endPoint = tiles[^1].GetComponent<TileEndPoint>().endPoint.position;
        tile.position = endPoint;
        tiles.Add(tile);

        DeactivateAndEnqueue(tile, "SmallSpawnPoint", groundPool);
        DeactivateAndEnqueue(tile, "TallSpawnPoint", backgroundPool);
        
        SpawnGroundElements(tile);
        SpawnBackgroundElements(tile);
    }

    private void DeactivateAndEnqueue(Transform tile, string spawnPointTag, List<Transform> pool)
    {
        foreach (Transform child in tile)
        {
            if (child.CompareTag(spawnPointTag))
            {
                foreach (Transform element in child)
                {
                    element.gameObject.SetActive(false);
                    pool.Add(element);
                }
            }
        }
    }

    private void InitializeObjectPool(Transform[] prefabs, List<Transform> pool, int initialSize)
    {
        foreach (var prefab in prefabs)
        {
            for (var i = 0; i < initialSize; i++)
            {
                var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                instance.gameObject.SetActive(false);
                pool.Add(instance);
            }
        }
    }

    private Transform GetRandomPooledObject(List<Transform> pool, Transform[] prefabs)
    {
        if (pool.Count > 0)
        {
            var randomIndex = Random.Range(0, pool.Count);
            var obj = pool[randomIndex];
            pool.RemoveAt(randomIndex);
            return obj;
        }
        else
        {
            var prefab = prefabs[Random.Range(0, prefabs.Length)];
            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            return instance;
        }
    }
}
