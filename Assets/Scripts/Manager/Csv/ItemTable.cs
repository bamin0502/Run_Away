using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using CsvHelper;

public class ItemTable : DataTable
{
    public List<ItemData> itemData = new List<ItemData>();
    
    public override void Load(string path)
    {
        path = string.Format(FormatPath, path);
        Debug.Log($"Formatted path for loading: {path}");

        itemData.Clear();

        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load text asset from path: {path}");
            return;
        }
        
        using (var reader = new StringReader(textAsset.text))
        using (var csvReader = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
        {
            var records = csvReader.GetRecords<ItemData>();
            itemData.AddRange(records);
        }

        Debug.Log($"Loaded {itemData.Count} item data records.");
    }
    
    public List<GameObject> GetLoadedItems(string itemFolderPath)
    {
        List<GameObject> loadedItems = new List<GameObject>();

        foreach (var item in itemData)
        {
            string fullPath = $"{itemFolderPath}/{item.ItemNameEnglish}";
            Debug.Log($"Loading item prefab from path: {fullPath}");
            var itemPrefab = Resources.Load<GameObject>(fullPath);
            if (itemPrefab != null)
            {
                loadedItems.Add(itemPrefab);
                Debug.Log($"Successfully loaded item prefab: {fullPath}");
            }
            else
            {
                Debug.LogWarning($"Item prefab not found: {fullPath}");
            }
        }

        return loadedItems;
    }
}
