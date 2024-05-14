using System.Collections.Generic;
using UnityEngine;

public static class DataManager
{
    private static Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

    static DataManager()
    {
        // 테이블 초기화 및 로드
        AddTable<ObstacleTable>("RunAway_Obstacle");
    }

    private static void AddTable<T>(string id) where T : DataTable, new()
    {
        if (!tables.ContainsKey(id))
        {
            var table = new T();
            table.Load(id);
            tables.Add(id, table);
        }
    }

    public static ObstacleTable GetObstacleTable()
    {
        return Get<ObstacleTable>("RunAway_Obstacle");
    }

    private static T Get<T> (string id) where T : DataTable
    {
        if (!tables.TryGetValue(id, out var table))
        {
            Debug.LogError("Table not found: " + id);
            return null;
        }
        return table as T;
    }
}