using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Transform playerTransform;
    public Transform tileManagerTransform;
    public Vector3 startPoint = new Vector3(0, 0, 17);
    public int numberOfTiles = 5;
    public int noObstaclesInitially = 2;
    public float tileLength = 17;
    public float moveSpeed;

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
    private float distanceCounter = 0f;
    public float initialOtherItemSpawnChance = 0.05f;
    public float maxOtherItemSpawnChance = 0.2f;
    public float distanceFactor = 0.0005f;

    private Collider[] overlapResults = new Collider[10];

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();

        itemPrefabs = DataManager.GetItemTable().GetLoadedItems("Item");
        sectionPrefabs = DataManager.GetSectionTable().GetLoadedSections("Tile");
        obstaclePrefabsBySection = DataManager.GetObstacleTable().GetObstaclesBySection();

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

        var selectedLanes = new HashSet<int>();
        while (selectedLanes.Count < Random.Range(1, 3))
        {
            selectedLanes.Add(Random.Range(0, 3));
        }

        foreach (var spawnPoint in spawnPoints)
        {
            var lanePosition = Mathf.FloorToInt((spawnPoint.localPosition.x + 3.8f) / 3.8f);

            if (selectedLanes.Contains(lanePosition))
            {
                var obstaclePrefab = filteredObstacles[Random.Range(0, filteredObstacles.Count)];
                var obstacleCollider = obstaclePrefab.GetComponent<Collider>();

                if (obstacleCollider == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Obstacle prefab {obstaclePrefab.name} does not have a Collider component.");
#endif
                    continue;
                }

                var obstacleSize = obstacleCollider.bounds.size;
                var obstacleCenter = spawnPoint.position + obstacleCollider.bounds.center;

                if (!Physics.CheckBox(obstacleCenter, obstacleSize / 2, spawnPoint.rotation, LayerMask.GetMask("Obstacle")))
                {
                    var obstacle = Instantiate(obstaclePrefab, spawnPoint.position, spawnPoint.rotation);
                    obstacle.transform.SetParent(tile, true);
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
        tile.position = tiles[^1].position + new Vector3(0, 0, tileLength);
        tiles.Add(tile);

        var sectionTypeComponent = tile.GetComponent<SectionType>();
        if (sectionTypeComponent != null)
        {
            SpawnObstacles(tile, sectionTypeComponent.sectionType);
        }
        else
        {
#if DEBUG
            Debug.LogWarning("SectionType component not found on the section prefab.");
#endif
        }

        SpawnItems(tile);
    }

    private void SpawnItems(Transform tile)
    {
        var bounds = tile.GetComponentInChildren<Collider>().bounds;
        float[] lanePositions = { -3.8f, 0f, 3.8f };

        foreach (var lane in lanePositions)
        {
            for (int attempts = 0; attempts < 10; attempts++)
            {
                var randomPosition = new Vector3(lane, bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));

                if (!IsObstacleAtPosition(randomPosition))
                {
                    if (!isFirstItem && Mathf.Approximately(previousItemPosition.z, randomPosition.z))
                    {
                        SpawnSingleItem((previousItemPosition + randomPosition) / 2, tile);
                    }

                    SpawnSingleItem(randomPosition, tile);
                    previousItemPosition = randomPosition;
                    isFirstItem = false;
                    break;
                }
            }
        }
    }

    private void SpawnSingleItem(Vector3 position, Transform tile)
    {
        GameObject itemPrefab;
        float currentDistance = Vector3.Distance(Vector3.zero, position);

        float otherItemSpawnChance = Mathf.Clamp(initialOtherItemSpawnChance + (currentDistance * distanceFactor), initialOtherItemSpawnChance, maxOtherItemSpawnChance);

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
