using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using CsvHelper;

public class SectionTable : DataTable
{
    private List<SectionData> sectionData = new List<SectionData>();
    
    public override void Load(string path)
    {
        path = string.Format(FormatPath, path);
#if UNITY_EDITOR
        Debug.Log($"Formatted path for loading: {path}");
#endif
        

        sectionData.Clear();

        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"Failed to load text asset from path: {path}");
#endif
            
            return;
        }
        
        using (var reader = new StringReader(textAsset.text))
        using (var csvReader = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
        {
            var records = csvReader.GetRecords<SectionData>().ToList();
            sectionData.AddRange(records);
        }
#if UNITY_EDITOR
        Debug.Log($"Loaded {sectionData.Count} section data records.");  
#endif
        
    }
    
    public List<GameObject> GetLoadedSections(string sectionFolderPath)
    {
        var loadedSections = new List<GameObject>();

        foreach (var section in sectionData)
        {
            var fullPath = $"{sectionFolderPath}/{section.SectionName}";
            #if UNITY_EDITOR
            Debug.Log($"Loading section prefab from path: {fullPath}");
            #endif
            var sectionPrefab = Resources.Load<GameObject>(fullPath);
            if (sectionPrefab != null)
            {
                loadedSections.Add(sectionPrefab);
                #if UNITY_EDITOR
                Debug.Log($"Successfully loaded section prefab: {fullPath}");
                #endif
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning($"Section prefab not found: {fullPath}");
                #endif
            }
        }

        return loadedSections;
    }
}
