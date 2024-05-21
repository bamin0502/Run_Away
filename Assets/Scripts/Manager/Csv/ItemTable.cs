using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using CsvHelper;
using CsvHelper.Configuration;

public class ItemTable : DataTable
{
    private List<ItemData> itemData = new List<ItemData>();

    public override void Load(string path)
    {
        path = string.Format(FormatPath, path);
#if UNITY_EDITOR
        Debug.Log($"Formatted path for loading: {path}");
#endif
       

        itemData.Clear();

        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"Failed to load text asset from path: {path}"); 
#endif
            
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Encoding = System.Text.Encoding.UTF8,
            Delimiter = ",",
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using (var reader = new StringReader(textAsset.text))
        using (var csvReader = new CsvReader(reader, config))
        {
            var records = csvReader.GetRecords<ItemData>();
            itemData.AddRange(records);
        }

#if UNITY_EDITOR
        Debug.Log($"Loaded {itemData.Count} item data records.");
#endif
        
    }

    public List<GameObject> GetLoadedItems(string itemFolderPath)
    {
        List<GameObject> loadedItems = new List<GameObject>();

        foreach (var item in itemData)
        {
            string fullPath = $"{itemFolderPath}/{item.ItemNameEnglish}";
#if UNITY_EDITOR
            Debug.Log($"Loading item prefab from path: {fullPath}");
#endif
            
            var itemPrefab = Resources.Load<GameObject>(fullPath);
            if (itemPrefab != null)
            {
                var itemInstance = (itemPrefab);
                var itemTypeComponent = itemInstance.GetComponent<ItemType>();
                if (itemTypeComponent == null)
                {
                    itemTypeComponent = itemInstance.AddComponent<ItemType>();
                }

                itemTypeComponent.ItemID = item.ItemID;
                itemTypeComponent.ItemNameEnglish = item.ItemNameEnglish;
                itemTypeComponent.ItemTypeNum = item.ItemType;
                itemTypeComponent.ItemSave = item.ItemSave;
                itemTypeComponent.ItemEffect = item.ItemEffect;
                itemTypeComponent.ItemPrice = item.ItemPrice;
                itemTypeComponent.ItemAmount = item.ItemAmount;
                itemTypeComponent.ItemDuration = item.ItemDuration;

#if UNITY_EDITOR
                Debug.Log($"Set ItemType for {itemTypeComponent.ItemNameEnglish}: ID={itemTypeComponent.ItemID}, Type={itemTypeComponent.ItemTypeNum}");
#endif
                loadedItems.Add(itemInstance);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Item prefab not found: {fullPath}");
#endif
                
            }
        }

        return loadedItems;
    }
}
