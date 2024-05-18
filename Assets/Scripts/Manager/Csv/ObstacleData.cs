using CsvHelper.Configuration.Attributes;

public class ObstacleData
{
    [Name("Obstacle_ID")]
    public int ObstacleID { get; set; }
    
    [Name("Obstacle_name(english)")]
    public string ObstacleNameEnglish { get; set; }
    
    [Name("Obstacle_coin")]
    public int ObstacleCoin { get; set; }
    
    [Name("Obstacle_type")]
    public int ObstacleType { get; set; }
    
    [Name("Obstacle_speed")]
    public int ObstacleSpeed { get; set; }    
    
    [Name("obstacle_section")]
    [TypeConverter(typeof(Int32DefaultConverter))]
    public int ObstacleSection { get; set; }
    

}
