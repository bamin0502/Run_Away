using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleData
{
    public static readonly string FormatPath = "Obstacle/{0}";
    
    public int id;
    public string Korname;
    public string Engname;
    public int Coin;
    public int Type;
    public int Speed;

    public string GetKorname()
    {
        return Korname;    
    }
    public string GetEngname()
    {
        return Engname;    
    }
    public int GetCoin()
    {
        return Coin;    
    }
    public int GetType()
    {
        return Type;    
    }
    public int GetSpeed()
    {
        return Speed;    
    }
    
}
