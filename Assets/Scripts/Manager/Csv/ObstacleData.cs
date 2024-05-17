using CsvHelper.Configuration.Attributes;

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
