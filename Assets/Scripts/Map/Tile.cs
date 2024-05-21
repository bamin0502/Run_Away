using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Transform playerTransform;
    public Transform tileManagerTransform;
    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public int noObstaclesInitially = 2;
    public float tileLength = 17;
    public float initialMoveSpeed = 5f;
    public float moveSpeedIncreaseDistance = 20f; 
    public float moveSpeedMultiplier = 1.05f; 
    

    private List<Transform> tiles = new List<Transform>();
    private Vector3 nextTilePosition;
    private GameManager gameManager;

    private List<GameObject> itemPrefabs;
    private List<GameObject> sectionPrefabs;
    private Dictionary<int, List<GameObject>> obstaclePrefabsBySection;

    private GameObject primaryItemPrefab;
    private List<GameObject> otherItemPrefabs;

    private Vector3 previousItemPosition = Vector3.zero;
    private bool isFirstItem = true;
    private float totalDistance = 0f;
    public float specialItemSpawnDistance = 100f;
    public float initialOtherItemSpawnChance = 0.05f;
    public float maxOtherItemSpawnChance = 0.2f;
    public float distanceFactor = 0.0005f;
    public int coinLineLength = 5;
    public float coinSpacing = 3.0f;

    private Collider[] overlapResults = new Collider[10];
    
    private float moveSpeed;
    private float distanceTravelled = 0f;
    [Header("일정 거리마다 속도 증가"),Tooltip("여기서 설정한 거리마다 속도가 증가합니다.")]
    [SerializeField]private float nextSpeedIncreaseDistance = 20f;
    
    [Header("최대 속도"),Tooltip("최대 속도 조절을 여기서 하시면 됩니다.")]
    [SerializeField]public float maxMoveSpeed = 15f;
    
    

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();

        itemPrefabs = DataManager.GetItemTable().GetLoadedItems("Item");
        sectionPrefabs = DataManager.GetSectionTable().GetLoadedSections("Tile");
        obstaclePrefabsBySection = DataManager.GetObstacleTable().GetObstaclesBySection();

        primaryItemPrefab = itemPrefabs.Find(item => item.name == "Coin");

#if UNITY_EDITOR
        if (primaryItemPrefab == null)
        {
            Debug.LogError("Primary item prefab (Coin) not found!");
        }
        else
        {
            Debug.Log($"Primary Item Prefab: {primaryItemPrefab.name}");
        }
#endif

        otherItemPrefabs = itemPrefabs.FindAll(item => item.name != "Coin");
#if UNITY_EDITOR
        foreach (var item in otherItemPrefabs)
        {
            Debug.Log($"Other Item Prefab: {item.name}");
        }
#endif
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

        moveSpeed = initialMoveSpeed;
    }

    private void Update()
    {
        if (!gameManager.isGameover && !gameManager.isPaused && gameManager.isPlaying)
        {
            MoveTiles();
            if (tiles.Count > 0 && tiles[0].position.z < playerTransform.position.z - 50)
            {
                ReuseTile();
            }

            distanceTravelled += moveSpeed * Time.deltaTime;

            if (distanceTravelled > nextSpeedIncreaseDistance)
            {
                moveSpeed = Mathf.Min(moveSpeed * moveSpeedMultiplier, maxMoveSpeed); 
                nextSpeedIncreaseDistance += moveSpeedIncreaseDistance;
            }

            totalDistance += moveSpeed * Time.deltaTime;
            gameManager.stageSpeed = moveSpeed; // Update GameManager's stageSpeed
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
        var newTile = Instantiate(sectionPrefab, nextTilePosition, Quaternion.identity, tileManagerTransform).transform;
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
                SpawnObstacles(newTile, sectionTypeComponent.sectionType);
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

    private void SpawnObstacles(Transform tile, int sectionType)
    {
        var spawnPoints = GetAllChildTransforms(tile, "ObstacleSpawnPoint");

        if (spawnPoints.Count == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning("No ObstacleSpawnPoint found in the tile.");
#endif
            return;
        }

        if (!obstaclePrefabsBySection.TryGetValue(sectionType, out var filteredObstacles))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"No obstacles found for section type: {sectionType}");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log($"Spawn points found: {spawnPoints.Count}, filtered obstacles: {filteredObstacles.Count}");
#endif

        var selectedLanes = new HashSet<int>();
        while (selectedLanes.Count < Random.Range(1, 3))
        {
            selectedLanes.Add(Random.Range(0, 3));
        }

        foreach (var spawnPoint in spawnPoints)
        {
            var lanePosition = Mathf.RoundToInt((spawnPoint.localPosition.x + 3.8f) / 3.8f);

            if (selectedLanes.Contains(lanePosition))
            {
                var obstaclePrefab = filteredObstacles[Random.Range(0, filteredObstacles.Count)];
                var obstacleCollider = obstaclePrefab.GetComponent<Collider>();

                if (!obstacleCollider)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Obstacle prefab {obstaclePrefab.name} does not have a Collider component.");
#endif
                    continue;
                }

                var bounds = obstacleCollider.bounds;
                var obstacleSize = bounds.size;
                var obstacleCenter = spawnPoint.position + bounds.center;

                if (Physics.OverlapBox(obstacleCenter, obstacleSize / 2, spawnPoint.rotation, LayerMask.GetMask("Obstacle")).Length == 0)
                {
                    var obstacle = Instantiate(obstaclePrefab, spawnPoint.position, spawnPoint.rotation);
                    obstacle.transform.SetParent(tile, true);
#if UNITY_EDITOR
                    Debug.Log($"Spawned obstacle: {obstacle.name} at position: {spawnPoint.position}");
#endif
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Spawn point at {spawnPoint.position} is already occupied by another obstacle.");
#endif
                }
            }
        }
    }

    private void ReuseTile()
    {
        var tile = tiles[0];
        tiles.RemoveAt(0);

        // 기존 타일의 장애물 제거
        foreach (Transform child in tile)
        {
            if (child.CompareTag("Obstacle"))
            {
                Destroy(child.gameObject);
            }
        }

        // 새로운 섹션 타입 무작위 선택
        var newSectionPrefab = sectionPrefabs[Random.Range(0, sectionPrefabs.Count)];
        var newSectionTypeComponent = newSectionPrefab.GetComponent<SectionType>();

        if (newSectionTypeComponent != null)
        {
            tile.position = tiles[^1].position + new Vector3(0, 0, tileLength);
            tile.GetComponent<SectionType>().sectionType = newSectionTypeComponent.sectionType;

            SpawnObstacles(tile, newSectionTypeComponent.sectionType);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("SectionType component not found on the new section prefab.");
#endif
        }

        tiles.Add(tile);
        SpawnItems(tile);
    }

    private void SpawnItems(Transform tile)
    {
        var bounds = tile.GetComponentInChildren<Collider>().bounds;
        float[] lanePositions = { -3.8f, 0f, 3.8f };

        float selectedLane = lanePositions[Random.Range(0, lanePositions.Length)];

        for (int attempts = 0; attempts < 10; attempts++)
        {
            var randomPosition = new Vector3(selectedLane, bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));

            if (!IsObstacleAtPosition(randomPosition))
            {
                SpawnCoinLine(randomPosition, selectedLane, tile);
                previousItemPosition = randomPosition;
                isFirstItem = false;
                break;
            }
        }
    }

    private void SpawnCoinLine(Vector3 startPosition, float lane, Transform tile)
    {
        for (int i = 0; i < coinLineLength; i++)
        {
            Vector3 position = new Vector3(lane, startPosition.y, startPosition.z + i * coinSpacing);
            if (!IsObstacleAtPosition(position))
            {
                SpawnSingleItem(position, tile, true);
            }
        }
    }

    private void SpawnSingleItem(Vector3 position, Transform tile, bool isCoin = false)
    {
        GameObject itemPrefab;

        if (isCoin)
        {
            itemPrefab = primaryItemPrefab;
        }
        else
        {
            float currentDistance = Vector3.Distance(Vector3.zero, position);

            if (totalDistance >= specialItemSpawnDistance)
            {
                itemPrefab = otherItemPrefabs[Random.Range(0, otherItemPrefabs.Count)];
                totalDistance = 0f;
            }
            else
            {
                float otherItemSpawnChance = Mathf.Clamp(initialOtherItemSpawnChance + (currentDistance * distanceFactor), initialOtherItemSpawnChance, maxOtherItemSpawnChance);
                if (Random.value < otherItemSpawnChance)
                {
                    itemPrefab = otherItemPrefabs[Random.Range(0, otherItemPrefabs.Count)];
                }
                else
                {
                    itemPrefab = primaryItemPrefab;
                    Vector3.Distance(previousItemPosition, position);
                }
            }
        }

        var item = Instantiate(itemPrefab, position, Quaternion.identity);
        item.transform.SetParent(tile, true);

        var itemTypeComponent = item.GetComponent<ItemType>();
        if (itemTypeComponent)
        {
#if UNITY_EDITOR
            Debug.Log($"Spawned Item: {itemTypeComponent.ItemNameEnglish}, ID: {itemTypeComponent.ItemID}");
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("ItemType component not found on the spawned item.");
#endif
        }
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
}
