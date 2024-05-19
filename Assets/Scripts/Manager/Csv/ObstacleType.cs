using UnityEngine;

public class ObstacleType : MonoBehaviour
{
    [SerializeField]private int obstacleID;
    [SerializeField]private string obstacleNameEnglish;
    [SerializeField]private int obstacleCoin;
    [SerializeField]private int obstacleType;
    [SerializeField]private int obstacleSpeed;
    [SerializeField]private int obstacleSection;

    public int ObstacleID
    {
        get => obstacleID;
        set => obstacleID = value;
    }
    
    public string ObstacleNameEnglish
    {
        get => obstacleNameEnglish;
        set => obstacleNameEnglish = value;
    }
    
    public int ObstacleCoin
    {
        get => obstacleCoin;
        set => obstacleCoin = value;
    }
    
    public int ObstacleTypeNum
    {
        get => obstacleType;
        set => obstacleType = value;
    }
    
    public int ObstacleSpeed
    {
        get => obstacleSpeed;
        set => obstacleSpeed = value;
    }
    
    public int ObstacleSection
    {
        get => obstacleSection;
        set => obstacleSection = value;
    }
    
}
