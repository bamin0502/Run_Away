using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class StringTable : DataTable
{
    private class Data
    {
        public string Id { get; set; }
        public string String { get; set; }
    }

    private Dictionary<string, string> table = new Dictionary<string, string>();

    public override void Load(string path)
    {
        path = string.Format(FormatPath, path);

        table.Clear();

        var textAsset = Resources.Load<TextAsset>(path);
        //var textAsset = Addressables.LoadAssetAsync<TextAsset>(path).WaitForCompletion();
        //Debug.Log(textAsset.text);
        
        using (var reader = new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(textAsset.text)), System.Text.Encoding.UTF8))
        using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csvReader.GetRecords<Data>();
            foreach (var record in records)
            {
                table.Add(record.Id, record.String);
            }
        }
    }

    public string Get(string id)
    {
        return !table.TryGetValue(id, out var value) ? string.Empty : value;
    }

}