using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform playerTransform;

    public Transform[] groundPrefabs;
    public Transform[] backgroundPrefabs;

    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public int noObstaclesInitially = 2;
    public float tileLength = 17;
    public float moveSpeed;

    private readonly List<Transform> tiles = new List<Transform>();
    private Vector3 nextTilePosition;

    private GameManager gameManager;

    private readonly List<Transform> groundPool = new List<Transform>();
    private readonly List<Transform> backgroundPool = new List<Transform>();

    private List<GameObject> obstaclePrefabs;
    private List<GameObject> itemPrefabs;

    private GameObject primaryItemPrefab;
    private List<GameObject> otherItemPrefabs;

    private Vector3 previousItemPosition = Vector3.zero;
    private bool isFirstItem = true;
    private float distanceCounter = 0f;
    public float initialOtherItemSpawnChance = 0.05f; // 초기 아이템 출현 확률
    public float maxOtherItemSpawnChance = 0.2f; // 최대 아이템 출현 확률
    public float distanceFactor = 0.0005f; // 거리 증가에 따른 확률 증가 비율

    private Collider[] overlapResults = new Collider[10]; // OverlapSphereNonAlloc 결과를 저장할 배열

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();

        InitializeObjectPool(groundPrefabs, groundPool, 10);
        InitializeObjectPool(backgroundPrefabs, backgroundPool, 10);

        var obstacleTable = DataManager.GetObstacleTable();
        obstaclePrefabs = obstacleTable.GetLoadedObstacles("Obstacle");
        
        var itemTable = DataManager.GetItemTable();
        itemPrefabs = itemTable.GetLoadedItems("Item");

        primaryItemPrefab = itemPrefabs.Find(item => item.name == "Coin");
        otherItemPrefabs = itemPrefabs.FindAll(item => item.name != "Coin");
    }

    private void Start()
    {
        nextTilePosition = startPoint;
        for (var i = 0; i < numberOfTiles; i++)
        {
            var tile = SpawnTile(i >= noObstaclesInitially);
            SpawnItems(tile);
        }
        CheckObstacleColliders();
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
        if (tilePrefab == null)
        {
            Debug.LogWarning("Tile prefab is not assigned.");
            return null;
        }

        var newTile = Instantiate(tilePrefab, nextTilePosition, Quaternion.identity, transform);
        if (newTile == null)
        {
            Debug.LogWarning("Failed to instantiate tile prefab.");
            return null;
        }

        nextTilePosition += new Vector3(0, 0, tileLength);
        tiles.Add(newTile);

        if (spawnObstacles)
        {
            SpawnObstacles(newTile);
        }

        SpawnGroundElements(newTile);
        SpawnBackgroundElements(newTile);

        return newTile;
    }

    private void CheckObstacleColliders()
    {
        foreach (var obstaclePrefab in obstaclePrefabs)
        {
            var component = obstaclePrefab.GetComponent<Collider>();
            if (component == null)
            {
                Debug.LogWarning($"Obstacle prefab {obstaclePrefab.name} does not have a Collider component.");
            }
            else
            {
                Debug.Log($"Obstacle prefab {obstaclePrefab.name} Collider found. Tag: {component.tag}, isTrigger: {component.isTrigger}");
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
                    var obstacle = Instantiate(obstaclePrefab, spawnPoint.position, spawnPoint.rotation);

                    obstacle.transform.SetParent(tile, true);

                    var component = obstacle.GetComponent<Collider>();
                    if (component == null)
                    {
                        Debug.LogWarning($"Obstacle {obstacle.name} does not have a Collider component.");
                    }
                    else
                    {
                        Debug.Log($"Obstacle {obstacle.name} Collider found. Tag: {component.tag}, isTrigger: {component.isTrigger}");
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
        var endPoint = tiles[^1].position + new Vector3(0, 0, tileLength);
        tile.position = endPoint;
        tiles.Add(tile);

        DeactivateAndEnqueue(tile, "SmallSpawnPoint", groundPool);
        DeactivateAndEnqueue(tile, "TallSpawnPoint", backgroundPool);

        SpawnGroundElements(tile);
        SpawnBackgroundElements(tile);
        SpawnItems(tile);
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

    private void SpawnItems(Transform tile)
    {
        var bounds = tile.GetComponent<Collider>().bounds;
        float[] lanePositions = { -3.8f, 0f, 3.8f };
        int laneCount = lanePositions.Length;
        
        Vector3 currentItemPosition = Vector3.zero;

        for (int lane = 0; lane < laneCount; lane++)
        {
            int attempts = 0;
            int maxAttempts = 10;
            bool itemSpawned = false;

            while (attempts < maxAttempts && !itemSpawned)
            {
                Vector3 randomPosition = GetRandomPositionInLane(bounds, lanePositions[lane]);
                
                if (!IsObstacleAtPosition(randomPosition))
                {
                    currentItemPosition = randomPosition;

                    if (!isFirstItem && previousItemPosition.z != currentItemPosition.z)
                    {
                        Vector3 midPosition = (previousItemPosition + currentItemPosition) / 2;
                        midPosition.y = bounds.min.y;
                        SpawnSingleItem(midPosition, tile);
                    }

                    SpawnSingleItem(currentItemPosition, tile);
                    previousItemPosition = currentItemPosition;
                    itemSpawned = true;
                    isFirstItem = false;
                }
                attempts++;
            }
        }
    }

    private void SpawnSingleItem(Vector3 position, Transform tile)
    {
        GameObject itemPrefab;
        float currentDistance = Vector3.Distance(Vector3.zero, position); // 시작 위치부터 현재 위치까지의 거리

        float otherItemSpawnChance = initialOtherItemSpawnChance + (currentDistance * distanceFactor);
        otherItemSpawnChance = Mathf.Clamp(otherItemSpawnChance, initialOtherItemSpawnChance, maxOtherItemSpawnChance);

        if (Random.value < otherItemSpawnChance)
        {
            itemPrefab = otherItemPrefabs[Random.Range(0, otherItemPrefabs.Count)];
            distanceCounter = 0f;
        }
        else
        {
            itemPrefab = primaryItemPrefab;
            distanceCounter += Vector3.Distance(previousItemPosition, position);
        }

        var item = Instantiate(itemPrefab, position, Quaternion.identity);
        item.transform.SetParent(tile, true);
    }

    private Vector3 GetRandomPositionInLane(Bounds bounds, float lanePosition)
    {
        float x = lanePosition;
        float y = bounds.min.y;
        float z = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(x, y, z);
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
