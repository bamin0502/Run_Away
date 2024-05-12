using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DataManager
{
    private static Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

    static DataManager()
    {
        foreach (var id in tables.Keys.ToList())
        {
            tables[id].Load(string.Format(DataTable.FormatPath, id));
            tables.Add(id,tables[id]);
        }
        
       
        
    }
    
    public static StringTable GetStringTable()
    {
        return Get<StringTable>("StringTable");
    }
    public static T Get<T> (string id) where T : DataTable
    {
        if(!tables.TryGetValue(id, out var table))
        {
            Debug.LogError("Table not found: " + id);
            return null;
        }
        return table as T;
    }
}

