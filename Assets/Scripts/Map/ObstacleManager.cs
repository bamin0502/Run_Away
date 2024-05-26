using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    private Dictionary<int, List<GameObject>> obstaclePrefabsBySection;
    private List<GameObject> sectionPrefabs;
    public Queue<GameObject> obstaclePool = new Queue<GameObject>();

    void Awake()
    {
        sectionPrefabs = DataManager.GetSectionTable().GetLoadedSections("Tile");
        obstaclePrefabsBySection = DataManager.GetObstacleTable().GetObstaclesBySection();
    }

    public GameObject GetRandomSectionPrefab()
    {
        return sectionPrefabs[Random.Range(0, sectionPrefabs.Count)];
    }

    public void SpawnObstacles(Transform tile)
    {
        var sectionTypeComponent = tile.GetComponent<SectionType>();
        if (sectionTypeComponent == null) return;

        var spawnPoints = GetAllChildTransforms(tile, "ObstacleSpawnPoint");
        var filteredObstacles = obstaclePrefabsBySection[sectionTypeComponent.sectionType];
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

    public void ResetTileObstacles(Transform tile)
    {
        foreach (Transform child in tile)
        {
            if (child.CompareTag("Obstacle"))
            {
                GameObject o;
                (o = child.gameObject).SetActive(false);
                obstaclePool.Enqueue(o);
            }
        }

        var newSectionPrefab = GetRandomSectionPrefab();
        var newSectionTypeComponent = newSectionPrefab.GetComponent<SectionType>();

        if (newSectionTypeComponent)
        {
            tile.GetComponent<SectionType>().sectionType = newSectionTypeComponent.sectionType;
            SpawnObstacles(tile);
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
