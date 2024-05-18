using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using UnityEngine;

public class SectionTable : DataTable
{
    private List<SectionData> sectionData = new List<SectionData>();

    public override void Load(string path)
    {
        path = string.Format(FormatPath, path);
        Debug.Log($"Formatted path for loading: {path}");

        sectionData.Clear();

        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load text asset from path: {path}");
            return;
        }

        using (var reader = new StringReader(textAsset.text))
        using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csvReader.GetRecords<SectionData>().ToList();
            sectionData.AddRange(records);
        }

        Debug.Log($"Loaded {sectionData.Count} section data records.");
    }

    public List<GameObject> GetLoadedSections(string sectionFolderPath)
    {
        var loadedSections = new List<GameObject>();

        foreach (var section in sectionData)
        {
            var fullPath = $"{sectionFolderPath}/{section.SectionName}";
            Debug.Log($"Loading section prefab from path: {fullPath}");
            var sectionPrefab = Resources.Load<GameObject>(fullPath);
            if (sectionPrefab != null)
            {
                var sectionInstance = sectionPrefab;
                var sectionTypeComponent = sectionInstance.GetComponent<SectionType>();
                if (sectionTypeComponent != null)
                {
                    sectionTypeComponent.sectionID = section.SectionID;
                    sectionTypeComponent.sectionName = section.SectionName;
                    sectionTypeComponent.sectionType = section.SectionType;
                    sectionTypeComponent.sectionDistance = section.SectionDistance;
                }
                loadedSections.Add(sectionInstance);
                Debug.Log($"Successfully loaded section prefab: {fullPath}");
            }
            else
            {
                Debug.LogWarning($"Section prefab not found: {fullPath}");
            }
        }

        return loadedSections;
    }
}
