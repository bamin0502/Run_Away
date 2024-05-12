using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DataManager
{
    private static Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

    public static void LoadTable<T>(string path) where T : DataTable, new()
    {
        var table = new T();
        table.Load(path);
        tables.Add(typeof(T).Name, table);
    }

    public static T GetTable<T>() where T : DataTable
    {
        return tables[typeof(T).Name] as T;
    }
}

