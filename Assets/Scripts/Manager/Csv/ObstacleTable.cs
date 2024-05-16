using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        
        using (var reader = new StringReader(textAsset.text))
        using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csvReader.GetRecords<ObstacleData>().ToList();
            obstacleData.AddRange(records);
        }

        Debug.Log($"Loaded {obstacleData.Count} obstacle data records.");
    }

    public List<GameObject> GetLoadedObstacles(string obstacleFolderPath)
    {
        List<GameObject> loadedObstacles = new List<GameObject>();

        foreach (var obstacle in obstacleData)
        {
            string fullPath = $"{obstacleFolderPath}/{obstacle.ObstacleNameEnglish}";
            Debug.Log($"Loading obstacle prefab from path: {fullPath}");
            var obstaclePrefab = Resources.Load<GameObject>(fullPath);
            if (obstaclePrefab != null)
            {
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
}