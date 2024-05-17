using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    private List<GameObject> obstaclePrefabs;

    void Awake()
    {
        var obstacleTable = DataManager.GetObstacleTable();
        obstaclePrefabs = obstacleTable.GetLoadedObstacles("Obstacle");
    }

    public void SpawnObstacles(Transform tile)
    {
        var spawnPoints = GetChildTransformsWithTag(tile, "ObstacleSpawnPoint");
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
                    //obstacle.transform.localScale = obstaclePrefab.transform.localScale;
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