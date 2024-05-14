using System.Collections;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using UnityEngine;

public class Int32DefaultConverter:Int32Converter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }
        return base.ConvertFromString(text, row, memberMapData);
    }
}

public class ObstacleData
{
    [Name("Obstacle_ID")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ObstacleID { get; set; }
    [Name("Obstacle_name(korean)")]
    public string ObstacleNameKorean { get; set; }
    [Name("Obstacle_name(english)")]
    public string ObstacleNameEnglish { get; set; }
    [Name("Obstacle_coin")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ObstacleCoin { get; set; }
    [Name("Obstacle_type")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ObstacleType { get; set; }
    [Name("Obstacle_speed")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ObstacleSpeed { get; set; }    
}
