using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using UnityEngine;

public class ObstacleTable : DataTable
{
    private List<ObstacleData> obstacleData = new List<ObstacleData>();

    public override void Load(string path)
    {
        path = string.Format(FormatPath, path);
        Debug.Log($"Formatted path for loading: {path}");

        obstacleData.Clear();

        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load text asset from path: {path}");
            return;
        }

        using (var reader = new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(textAsset.text)), System.Text.Encoding.UTF8))
        using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csvReader.GetRecords<ObstacleData>().ToList();
            obstacleData.AddRange(records);
        }

        Debug.Log($"Loaded {obstacleData.Count} obstacle data records.");
    }

    public List<GameObject> GetLoadedObstacles(string obstacleFolderPath)
    {
        var loadedObstacles = new List<GameObject>();

        foreach (var obstacle in obstacleData)
        {
            var fullPath = $"{obstacleFolderPath}/{obstacle.ObstacleNameEnglish}";
            Debug.Log($"Loading obstacle prefab from path: {fullPath}");
            var obstaclePrefab = Resources.Load<GameObject>(fullPath);
            if (obstaclePrefab != null)
            {
                var existingObstacleTypeComponent = obstaclePrefab.GetComponent<ObstacleType>();
                if (existingObstacleTypeComponent == null)
                {
                    var obstacleTypeComponent = obstaclePrefab.AddComponent<ObstacleType>();
                    obstacleTypeComponent.obstacleID = obstacle.ObstacleID;
                    obstacleTypeComponent.obstacleNameEnglish = obstacle.ObstacleNameEnglish;
                    obstacleTypeComponent.obstacleCoin = obstacle.ObstacleCoin;
                    obstacleTypeComponent.obstacleType = obstacle.ObstacleType;
                    obstacleTypeComponent.obstacleSpeed = obstacle.ObstacleSpeed;
                    obstacleTypeComponent.obstacleSection = obstacle.ObstacleSection;
                }

                loadedObstacles.Add(obstaclePrefab);
                Debug.Log($"Successfully loaded obstacle prefab: {fullPath}");
            }
            else
            {
                Debug.LogWarning($"Obstacle prefab not found: {fullPath}");
            }
        }

        return loadedObstacles;
    }

    public Dictionary<int, List<GameObject>> GetObstaclesBySection()
    {
        var obstaclesBySection = new Dictionary<int, List<GameObject>>();

        foreach (var obstacle in obstacleData)
        {
            var obstaclePrefab = Resources.Load<GameObject>($"Obstacle/{obstacle.ObstacleNameEnglish}");
            if (obstaclePrefab != null)
            {
                if (!obstaclesBySection.ContainsKey(obstacle.ObstacleSection))
                {
                    obstaclesBySection[obstacle.ObstacleSection] = new List<GameObject>();
                }
                obstaclesBySection[obstacle.ObstacleSection].Add(obstaclePrefab);
            }
        }

        return obstaclesBySection;
    }
}
