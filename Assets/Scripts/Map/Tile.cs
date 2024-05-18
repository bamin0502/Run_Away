using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Transform playerTransform;
    public Transform tileManagerTransform;  // 타일의 부모를 지정하기 위한 Transform
    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public int noObstaclesInitially = 2;
    public float tileLength = 17;
    public float moveSpeed;

    private readonly List<Transform> tiles = new List<Transform>();
    private Vector3 nextTilePosition;
    private GameManager gameManager;

    private List<GameObject> itemPrefabs;
    private List<GameObject> sectionPrefabs;
    private Dictionary<int, List<GameObject>> obstaclePrefabsBySection;

    private GameObject primaryItemPrefab;
    private List<GameObject> otherItemPrefabs;

    private Vector3 previousItemPosition = Vector3.zero;
    private bool isFirstItem = true;
    private float distanceCounter = 0f;
    public float initialOtherItemSpawnChance = 0.05f;
    public float maxOtherItemSpawnChance = 0.2f;
    public float distanceFactor = 0.0005f;

    private Collider[] overlapResults = new Collider[10];

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();

        var itemTable = DataManager.GetItemTable();
        itemPrefabs = itemTable.GetLoadedItems("Item");

        var sectionTable = DataManager.GetSectionTable();
        sectionPrefabs = sectionTable.GetLoadedSections("Tile");

        var obstacleTable = DataManager.GetObstacleTable();
        obstaclePrefabsBySection = obstacleTable.GetObstaclesBySection();

        primaryItemPrefab = itemPrefabs.Find(item => item.name == "Coin");
        otherItemPrefabs = itemPrefabs.FindAll(item => item.name != "Coin");
    }

    private void Start()
    {
        nextTilePosition = startPoint;
        for (var i = 0; i < numberOfTiles; i++)
        {
            var tile = SpawnTile(i >= noObstaclesInitially);
            if (tile != null)
            {
                SpawnItems(tile);
            }
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
        if (sectionPrefabs == null || sectionPrefabs.Count == 0)
        {
            Debug.LogWarning("Section prefabs are not assigned.");
            return null;
        }

        var sectionPrefab = sectionPrefabs[Random.Range(0, sectionPrefabs.Count)];
        var newTile = Instantiate(sectionPrefab, nextTilePosition, Quaternion.identity, tileManagerTransform).transform; // 부모를 지정하여 생성
        if (newTile == null)
        {
            Debug.LogWarning("Failed to instantiate section prefab.");
            return null;
        }

        nextTilePosition += new Vector3(0, 0, tileLength);
        tiles.Add(newTile);

        if (spawnObstacles)
        {
            var sectionTypeComponent = newTile.GetComponent<SectionType>();
            if (sectionTypeComponent != null)
            {
                Debug.Log($"Spawning obstacles for section type: {sectionTypeComponent.sectionType}");
                SpawnObstacles(newTile, sectionTypeComponent.sectionType);
            }
            else
            {
                Debug.LogWarning("SectionType component not found on the section prefab.");
            }
        }

        return newTile;
    }

    private void CheckObstacleColliders()
    {
        foreach (var obstacleType in obstaclePrefabsBySection.Keys)
        {
            foreach (var obstaclePrefab in obstaclePrefabsBySection[obstacleType])
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
    }

    private void SpawnObstacles(Transform tile, int sectionType)
    {
        var spawnPoints = GetAllChildTransforms(tile, "ObstacleSpawnPoint");

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No ObstacleSpawnPoint found in the tile.");
            return;
        }

        if (!obstaclePrefabsBySection.TryGetValue(sectionType, out var filteredObstacles))
        {
            Debug.LogWarning($"No obstacles found for section type: {sectionType}");
            return;
        }

        // 타입별 장애물 리스트 출력
        Debug.Log($"Obstacles for section type {sectionType}: {filteredObstacles.Count}");

        // 스폰할 레인 선택 (최대 2개의 레인)
        List<int> lanes = new List<int> { 0, 1, 2 };
        int lanesToUse = Random.Range(1, 3); // 1 또는 2개의 레인을 선택
        HashSet<int> selectedLanes = new HashSet<int>();

        while (selectedLanes.Count < lanesToUse)
        {
            int lane = lanes[Random.Range(0, lanes.Count)];
            selectedLanes.Add(lane);
            lanes.Remove(lane); // 선택된 레인을 리스트에서 제거
        }

        foreach (var spawnPoint in spawnPoints)
        {
            // 레인 위치 계산 (왼쪽, 중앙, 오른쪽)
            var lanePosition = Mathf.FloorToInt((spawnPoint.localPosition.x + 3.8f) / 3.8f);

            if (selectedLanes.Contains(lanePosition))
            {
                var obstaclePrefab = filteredObstacles[Random.Range(0, filteredObstacles.Count)];
                var obstacleCollider = obstaclePrefab.GetComponent<Collider>();

                if (obstacleCollider == null)
                {
                    Debug.LogWarning($"Obstacle prefab {obstaclePrefab.name} does not have a Collider component.");
                    continue;
                }

                var obstacleSize = obstacleCollider.bounds.size;
                var obstacleCenter = spawnPoint.position + obstacleCollider.bounds.center;

                // CheckBox를 사용하여 충돌 체크
                if (!Physics.CheckBox(obstacleCenter, obstacleSize / 2, spawnPoint.rotation, LayerMask.GetMask("Obstacle")))
                {
                    var obstacle = Instantiate(obstaclePrefab, spawnPoint.position, spawnPoint.rotation);

                    if (obstacle == null)
                    {
                        Debug.LogError($"Failed to instantiate obstacle prefab: {obstaclePrefab.name}");
                        continue;
                    }

                    obstacle.transform.SetParent(tile, true);

                    Debug.Log($"Obstacle {obstacle.name} successfully instantiated and set active at position {spawnPoint.position}");
                }
                else
                {
                    Debug.LogWarning($"Spawn point at {spawnPoint.position} is already occupied by another obstacle.");
                }
            }
        }
    }

    private List<Transform> GetAllChildTransforms(Transform parent, string tag)
    {
        var result = new List<Transform>();
        var queue = new Queue<Transform>();

        queue.Enqueue(parent);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (Transform child in current)
            {
                if (child.CompareTag(tag))
                {
                    result.Add(child);
                }
                queue.Enqueue(child);
            }
        }

        return result;
    }

    private void ReuseTile()
    {
        var tile = tiles[0];
        tiles.RemoveAt(0);
        var endPoint = tiles[^1].position + new Vector3(0, 0, tileLength);
        tile.position = endPoint;
        tiles.Add(tile);

        var sectionTypeComponent = tile.GetComponent<SectionType>();
        if (sectionTypeComponent != null)
        {
            SpawnObstacles(tile, sectionTypeComponent.sectionType);
        }
        else
        {
            Debug.LogWarning("SectionType component not found on the section prefab.");
        }
        SpawnItems(tile);
    }

    private void SpawnItems(Transform tile)
    {
        var bounds = tile.GetComponentInChildren<Collider>().bounds;
        float[] lanePositions = { -3.8f, 0f, 3.8f };
        int laneCount = lanePositions.Length;

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
                    var currentItemPosition = randomPosition;

                    if (!isFirstItem && Mathf.Approximately(previousItemPosition.z, currentItemPosition.z))
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
        float currentDistance = Vector3.Distance(Vector3.zero, position);

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
