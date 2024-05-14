using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using CsvHelper;
using System.IO;
using System.Linq;
using CsvHelper.Configuration;

public class ObstacleTable : DataTable
{
    private List<GameObject> loadedObstacles = new List<GameObject>();

    public override void Load(string path)
    {
        path = string.Format(FormatPath, path);
        
        var fullPath = Path.Combine(Application.dataPath, path);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"CSV file not found at path: {fullPath}");
            return;
        }
        
        using (var reader = new StreamReader(fullPath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
               {
                   Delimiter = ",",
                   HeaderValidated = null,
                   MissingFieldFound = null
               }))
        {
            var obstacles = csv.GetRecords<ObstacleData>().ToList();
            foreach (var obstacle in obstacles)
            {
                Debug.Log($"한국어 확인용: {obstacle.ObstacleNameKorean}");
                
                GameObject prefab = Resources.Load<GameObject>($"Obstacle/{obstacle.ObstacleNameEnglish}");
                if (prefab != null)
                {
                    loadedObstacles.Add(prefab);
                    Debug.Log($"불러올 프리팹 {obstacle.ObstacleNameEnglish}");
                }
                else
                {
                    Debug.LogWarning($"해당 위치에 프리팹이 존재하지 않음 {obstacle.ObstacleNameEnglish}");
                }
            }
        }
    }

    public List<GameObject> GetLoadedObstacles()
    {
        return loadedObstacles;
    }
}