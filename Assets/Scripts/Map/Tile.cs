using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Player and Tile Manager"), Tooltip("플레이어와 타일 매니저 오브젝트")]
    public Transform playerTransform;
    public Transform tileManagerTransform;
    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public int noObstaclesInitially = 2;
    public int noItemsInitially = 2;
    public float tileLength = 17;

    [Header("Speed Settings")]
    public float moveSpeedIncreaseDistance = 20f;
    public float moveSpeedMultiplier = 1.05f;
    public float initialMoveSpeed = 5f;
    public float maxMoveSpeed = 30f;
    public float speedIncreaseDistanceIncrement = 5f;

    private List<Transform> tiles = new List<Transform>();
    private Vector3 nextTilePosition;
    private GameManager gameManager;

    private List<GameObject> itemPrefabs;
    private List<GameObject> sectionPrefabs;
    private Dictionary<int, List<GameObject>> obstaclePrefabsBySection;

    private GameObject primaryItemPrefab;
    private List<GameObject> otherItemPrefabs;

    public Queue<GameObject> itemPool = new Queue<GameObject>();
    public Queue<GameObject> obstaclePool = new Queue<GameObject>();

    private float totalDistance = 0f;
    public float specialItemSpawnDistance = 100f;
    public int coinLineLength = 5;
    public float coinSpacing = 3.0f;

    private Collider[] overlapResults = new Collider[10];

    private float moveSpeed;
    private float distanceTravelled = 0f;
    private float nextSpeedIncreaseDistance;
    private float itemSpawnInterval = 2f;
    private float itemSpawnTimer = 0f;
    private float lastSpecialItemSpawnDistance = 0f;

    private Vector3 lastCoinPosition = Vector3.zero;
    private bool hasLastCoinPosition = false;

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

                    Rigidbody rb = obstaclePrefab.GetComponent<Rigidbody>();
                    if (rb)
                    {
                        rb.isKinematic = false;
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.isKinematic = true;
                    }

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

    private HashSet<int> GetTwoUniqueRandomLanes()
    {
        var lanes = new HashSet<int>();
        while (lanes.Count < 2)
        {
            lanes.Add(Random.Range(0, 3));
        }
        return lanes;
    }

    private void ReuseTile()
    {
        var tile = tiles[0];
        tiles.RemoveAt(0);
        tiles.Add(tile);

        foreach (Transform child in tile)
        {
            if (child.CompareTag("Obstacle"))
            {
                GameObject o;
                (o = child.gameObject).SetActive(false);
                obstaclePool.Enqueue(o);

                Rigidbody rb = o.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.isKinematic = false;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }

                SetLayerRecursively(o, LayerMask.NameToLayer("Obstacle"));
            }
            else if (child.CompareTag("Item"))
            {
                GameObject o;
                (o = child.gameObject).SetActive(false);
                itemPool.Enqueue(o);
            }
        }

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
        if (tile == null) return;

        var bounds = tile.GetComponentInChildren<Collider>().bounds;
        float[] lanePositions = { -3.8f, 0f, 3.8f };

        HashSet<int> occupiedLanes = new HashSet<int>();

        foreach (Transform child in tile)
        {
            if (child.CompareTag("Obstacle"))
            {
                var lanePosition = Mathf.RoundToInt((child.localPosition.x + 3.8f) / 3.8f);
                occupiedLanes.Add(lanePosition);
            }
        }

        foreach (var lane in lanePositions)
        {
            var laneIndex = Mathf.RoundToInt((lane + 3.8f) / 3.8f);
            if (!occupiedLanes.Contains(laneIndex))
            {
                Vector3 startPosition = new Vector3(lane, bounds.min.y, bounds.min.z + Random.Range(0, bounds.size.z - coinSpacing * coinLineLength));
                SpawnCoinLine(startPosition, lane, tile);
            }
        }

        if (totalDistance - lastSpecialItemSpawnDistance > specialItemSpawnDistance)
        {
            Vector3 randomPosition = new Vector3(lanePositions[Random.Range(0, lanePositions.Length)], bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));
            if (!IsObstacleAtPosition(randomPosition))
            {
                SpawnSpecialItem(randomPosition, tile);
                lastSpecialItemSpawnDistance = totalDistance;
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

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(obj.transform);

        while (stack.Count > 0)
        {
            Transform current = stack.Pop();
            current.gameObject.layer = newLayer;

            foreach (Transform child in current)
            {
                stack.Push(child);
            }
        }
    }
}
