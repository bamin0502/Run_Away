using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    private Dictionary<int, List<GameObject>> obstaclePrefabsBySection;
    private ObjectPool obstaclePool;

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        var obstacleTable = DataManager.GetObstacleTable();
        obstaclePrefabsBySection = obstacleTable.GetObstaclesBySection();

        var allObstaclePrefabs = new List<GameObject>();
        foreach (var obstacleList in obstaclePrefabsBySection.Values)
        {
            allObstaclePrefabs.AddRange(obstacleList);
        }

        obstaclePool = new ObjectPool();
        obstaclePool.InitializePool(allObstaclePrefabs.ConvertAll(item => item.transform).ToArray(), 10);
    }

    public void SpawnObstacles(Transform tile, int sectionType)
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
                    var obstacleTransform = obstaclePool.GetRandomPooledObject(obstaclePrefab.transform);
                    if (obstacleTransform != null)
                    {
                        obstacleTransform.position = spawnPoint.position;
                        obstacleTransform.rotation = spawnPoint.rotation;
                        obstacleTransform.gameObject.SetActive(true);
                        obstacleTransform.SetParent(tile, true);
                    }
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
