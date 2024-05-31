using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    private Dictionary<int, List<GameObject>> obstaclePrefabsBySection;
    private Queue<GameObject> obstaclePool = new Queue<GameObject>();

    private void Awake()
    {
        obstaclePrefabsBySection = DataManager.GetObstacleTable().GetObstaclesBySection();
    }

    public void SpawnObstacles(Transform tile, int sectionType)
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

    public void RecycleObstacle(GameObject obstacle)
    {
        obstacle.SetActive(false);
        obstaclePool.Enqueue(obstacle);
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
