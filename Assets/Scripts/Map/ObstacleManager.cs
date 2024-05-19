using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    private Dictionary<int, List<GameObject>> obstaclePrefabsBySection;

    private void Awake()
    {
        var obstacleTable = DataManager.GetObstacleTable();
        obstaclePrefabsBySection = obstacleTable.GetObstaclesBySection();
    }

    public void SpawnObstacles(Transform tile, int sectionType)
    {
        var spawnPoints = GetChildTransformsWithTag(tile, "ObstacleSpawnPoint");

        if (!obstaclePrefabsBySection.TryGetValue(sectionType, out var filteredObstacles))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"No obstacles found for section type: {sectionType}");
#endif
            return;
        }

        foreach (var spawnPoint in spawnPoints)
        {
            if (filteredObstacles.Count > 0)
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
