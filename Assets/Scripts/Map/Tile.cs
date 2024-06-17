using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Player and Tile Manager")]
    public Transform playerTransform;
    public Transform tileManagerTransform;
    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public int noObstaclesInitially = 2;
    public int noItemsInitially = 2;
    public float tileLength = 17;

    [Header("Speed Settings")]
    public float moveSpeedIncreaseDistance = 200f;
    public float moveSpeedMultiplier = 1.01f;
    public float initialMoveSpeed = 5f;
    public float maxMoveSpeed = 15f;
    public float speedIncreaseDistanceIncrement = 50f;

    [Header("Coin Settings")]
    public float specialItemSpawnDistance = 100f;
    public int coinLineLength = 5;
    public float coinSpacing = 3.0f;
    public int parabolicPoints = 8;
    public float parabolicCurveHeight = 2.0f;

    private List<Transform> tiles = new List<Transform>();
    private Vector3 nextTilePosition;
    private GameManager gameManager;

    private List<GameObject> itemPrefabs;
    private List<GameObject> sectionPrefabs;
    private Dictionary<int, List<GameObject>> obstaclePrefabsBySection;

    private GameObject primaryItemPrefab;
    private List<GameObject> otherItemPrefabs;

    public Queue<GameObject> itemPool = new Queue<GameObject>();
    private Queue<GameObject> obstaclePool = new Queue<GameObject>();

    [SerializeField] private float totalDistance = 0f;

    private Collider[] overlapResults = new Collider[10];

    private float moveSpeed;
    [SerializeField] private float distanceTravelled = 0f;
    private float nextSpeedIncreaseDistance;
    private float itemSpawnInterval = 2f;
    private float itemSpawnTimer = 0f;
    private float lastSpecialItemSpawnDistance = 0f;

    private Vector3 lastCoinPosition = Vector3.zero;
    private bool hasLastCoinPosition = false;
    private bool isPaused = false;
    private List<Vector3> parabolicPointsCache;

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
#endif

        otherItemPrefabs = itemPrefabs.FindAll(item => item.name != "Coin");

        CacheParabolicPoints();
    }

    private void Start()
    {
        nextTilePosition = startPoint;
        nextSpeedIncreaseDistance = moveSpeedIncreaseDistance;

        for (var i = 0; i < numberOfTiles; i++)
        {
            var tile = SpawnTile(i >= noObstaclesInitially);
            if (tile != null && i >= noItemsInitially)
            {
                SpawnItems(tile);
            }
        }

        moveSpeed = initialMoveSpeed;
    }

    private void Update()
    {
        if (isPaused)
        {
            return;
        }
        
        if (!gameManager.isGameover && gameManager.isPlaying)
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
                nextSpeedIncreaseDistance += moveSpeedIncreaseDistance + speedIncreaseDistanceIncrement;
                moveSpeedIncreaseDistance += speedIncreaseDistanceIncrement;
            }

            totalDistance += moveSpeed * Time.deltaTime;
            gameManager.stageSpeed = moveSpeed;

            itemSpawnTimer += Time.deltaTime;
            if (itemSpawnTimer >= itemSpawnInterval)
            {
                itemSpawnTimer = 0f;
                SpawnItems(tiles[^1]);
            }
        }
    }

    public void ResumeTileMovement()
    {
        isPaused = false;
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
        }

        return newTile;
    }

    private void SpawnObstacles(Transform tile, int sectionType)
    {
        var spawnPoints = GetAllChildTransforms(tile, "ObstacleSpawnPoint");

        if (spawnPoints.Count == 0) return;

        if (!obstaclePrefabsBySection.TryGetValue(sectionType, out var filteredObstacles)) return;

        var selectedLanes = GetTwoUniqueRandomLanes();

        foreach (var spawnPoint in spawnPoints)
        {
            var lanePosition = Mathf.RoundToInt((spawnPoint.localPosition.x + 3.8f) / 3.8f);

            if (selectedLanes.Contains(lanePosition))
            {
                GameObject obstaclePrefab;

                if (obstaclePool.Count > 0)
                {
                    obstaclePrefab = obstaclePool.Dequeue();
                    obstaclePrefab.transform.position = spawnPoint.position;
                    obstaclePrefab.transform.rotation = spawnPoint.rotation;
                    obstaclePrefab.SetActive(true);
                    obstaclePrefab.transform.SetParent(tile, true);

                    ResetRb(obstaclePrefab);
                    SetLayerRecursively(obstaclePrefab, LayerMask.NameToLayer("Obstacle"));
                }
                else
                {
                    obstaclePrefab = filteredObstacles[Random.Range(0, filteredObstacles.Count)];
                    var obstacle = Instantiate(obstaclePrefab, spawnPoint.position, spawnPoint.rotation);
                    obstacle.transform.SetParent(tile, true);
                }
            }
        }
    }

    private void ReuseTile()
    {
        var tile = tiles[0];
        tiles.RemoveAt(0);
        tiles.Add(tile);

        ResetChildObjects(tile);

        tile.position = tiles[^2].position + new Vector3(0, 0, tileLength);

        var newSectionPrefab = sectionPrefabs[Random.Range(0, sectionPrefabs.Count)];
        var newSectionTypeComponent = newSectionPrefab.GetComponent<SectionType>();

        if (newSectionTypeComponent)
        {
            tile.GetComponent<SectionType>().sectionType = newSectionTypeComponent.sectionType;
            SpawnObstacles(tile, newSectionTypeComponent.sectionType);
        }

        if (tiles.IndexOf(tile) >= noItemsInitially)
        {
            SpawnItems(tile);
        }
    }

    private void SpawnItems(Transform tile)
    {
        if (!tile) return;

        var bounds = tile.GetComponentInChildren<Collider>().bounds;
        float[] lanePositions = { -3.8f, 0f, 3.8f };

        float chosenLane = lanePositions[Random.Range(0, lanePositions.Length)];

        bool hasObstacleInLane = IsObstacleInLane(tile, chosenLane);
        var obstacle = GetObstacleInLane(tile, chosenLane);

        if (hasObstacleInLane && obstacle != null)
        {
            var obstacleTypeComponent = obstacle.GetComponent<ObstacleType>();
            if (obstacleTypeComponent != null)
            {
                if (obstacleTypeComponent.ObstacleTypeNum == 5)
                {
                    Vector3 startPosition = new Vector3(chosenLane, bounds.min.y, bounds.min.z + Random.Range(0, bounds.size.z - coinSpacing * coinLineLength));
                    SpawnCoinLine(startPosition, chosenLane, tile);
                }
                else if (obstacleTypeComponent.ObstacleTypeNum == 6)
                {
                    var walkByCollider = obstacle.Find("WalkBy").GetComponent<Collider>();
                    if (walkByCollider != null)
                    {
                        var bounds1 = walkByCollider.bounds;
                        Vector3 walkByBoundsMin = bounds1.min;
                        Vector3 walkByBoundsMax = bounds1.max;
                        float walkByHeight = walkByBoundsMax.y - walkByBoundsMin.y;

                        for (int i = 0; i < coinLineLength; i++)
                        {
                            Vector3 position = new Vector3(
                                chosenLane, 
                                walkByBoundsMin.y + walkByHeight + 0.5f, 
                                walkByBoundsMin.z + i * coinSpacing);

                            if (!IsObstacleAtPosition(position))
                            {
                                SpawnSingleItem(position, tile, true);
                            }
                        }
                    }
                }
                else
                {
                    var boxCollider = obstacle.GetComponent<BoxCollider>();
                    if (boxCollider)
                    {
                        var size = boxCollider.size;
                        Vector3 centerPos = obstacle.position + new Vector3(0, size.y / 2, 0);
                        float height = size.y;
                        SpawnParabolicCoins(centerPos, height, tile);
                    }
                }
            }
        }
        else
        {
            Vector3 startPosition = new Vector3(chosenLane, bounds.min.y, bounds.min.z + Random.Range(0, bounds.size.z - coinSpacing * coinLineLength));
            SpawnCoinLine(startPosition, chosenLane, tile);
        }

        if (totalDistance - lastSpecialItemSpawnDistance > specialItemSpawnDistance)
        {
            Vector3 randomPosition = new Vector3(lanePositions[Random.Range(0, lanePositions.Length)], bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));
            SpawnSpecialItem(randomPosition, tile);
            lastSpecialItemSpawnDistance = totalDistance;
        }
    }

    private void CacheParabolicPoints()
    {
        parabolicPointsCache = new List<Vector3>();
        for (int i = 0; i <= parabolicPoints; i++)
        {
            float t = (float)i / parabolicPoints;
            float z = (t - 0.5f) * coinSpacing * parabolicPoints;
            float y = parabolicCurveHeight * 4 * t * (1 - t);
            parabolicPointsCache.Add(new Vector3(0, y, z));
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

    private void SpawnParabolicCoins(Vector3 centerPos, float height, Transform tile)
    {
        foreach (var point in parabolicPointsCache)
        {
            Vector3 position = centerPos + point;
            if (!IsObstacleAtPosition(position))
            {
                SpawnSingleItem(position, tile, true);
            }
        }
    }

    private void SpawnSingleItem(Vector3 position, Transform tile, bool isCoin)
    {
        var itemPrefab = isCoin ? primaryItemPrefab : otherItemPrefabs[Random.Range(0, otherItemPrefabs.Count)];

        GameObject item;
        if (itemPool.Count > 0)
        {
            item = itemPool.Dequeue();
            item.transform.position = position;
            item.transform.rotation = Quaternion.identity;
            item.SetActive(true);
        }
        else
        {
            item = Instantiate(itemPrefab, position, Quaternion.identity);
        }

        item.transform.SetParent(tile, true);
    }

    private void SpawnSpecialItem(Vector3 position, Transform tile)
    {
        var specialItemPrefab = otherItemPrefabs[Random.Range(0, otherItemPrefabs.Count)];
        GameObject specialItem;
        if (itemPool.Count > 0)
        {
            specialItem = itemPool.Dequeue();
            specialItem.transform.position = position;
            specialItem.transform.rotation = Quaternion.identity;
            specialItem.SetActive(true);
        }
        else
        {
            specialItem = Instantiate(specialItemPrefab, position, Quaternion.identity);
        }

        specialItem.transform.SetParent(tile, true);
    }

    private bool IsObstacleAtPosition(Vector3 position)
    {
        int numColliders = Physics.OverlapSphereNonAlloc(position, 1f, overlapResults);
        for (int i = 0; i < numColliders; i++)
        {
            if (overlapResults[i].CompareTag("Obstacle") || overlapResults[i].CompareTag("Item"))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsObstacleInLane(Transform tile, float lane)
    {
        foreach (Transform child in tile)
        {
            if (child.CompareTag("Obstacle"))
            {
                float lanePosition = Mathf.Round((child.localPosition.x + 3.8f) / 3.8f) * 3.8f - 3.8f;
                if (Mathf.Approximately(lanePosition, lane))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private Transform GetObstacleInLane(Transform tile, float lane)
    {
        foreach (Transform child in tile)
        {
            if (child.CompareTag("Obstacle"))
            {
                float lanePosition = Mathf.Round((child.localPosition.x + 3.8f) / 3.8f) * 3.8f - 3.8f;
                if (Mathf.Approximately(lanePosition, lane))
                {
                    return child;
                }
            }
        }
        return null;
    }

    private List<Transform> GetAllChildTransforms(Transform parent, string tag)
    {
        var result = new List<Transform>();
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                result.Add(child);
            }
            result.AddRange(GetAllChildTransforms(child, tag));
        }
        return result;
    }

    private Transform GetChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
        }
        return null;
    }

    private HashSet<int> GetTwoUniqueRandomLanes()
    {
        var lanes = new HashSet<int>();
        while (lanes.Count < 2)
        {
            lanes.Add(Random.Range(0, 3));
        }
        return lanes;
    }

    private void ResetRb(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private void ResetChildObjects(Transform tile)
    {
        foreach (Transform child in tile)
        {
            if (child.CompareTag("Obstacle"))
            {
                GameObject o;
                (o = child.gameObject).SetActive(false);
                obstaclePool.Enqueue(o);
                ResetRb(o);
                SetLayerRecursively(o, LayerMask.NameToLayer("Obstacle"));
            }
            else if (child.CompareTag("Item"))
            {
                GameObject o;
                (o = child.gameObject).SetActive(false);
                itemPool.Enqueue(o);
            }
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
