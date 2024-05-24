using System;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

[Serializable]
public class GameData
{
    public int coin;
    public bool tutorialActive;
    public int HighScore;
    
}


public class JsonData : MonoBehaviour
{
    private GameData gameData;
    private GameManager gameManager;
    private string GetFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }
    
    private void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        
        //데이터 초기화 함수(밑에 주석을 제거하고 실행하면 됨)
        //ResetGameData();
        
        
        LoadGameData();
    }

    public void SaveGameData()
    {
        gameData.coin=gameManager.TotalCoins;
        gameData.tutorialActive = gameManager.isTutorialActive;
        gameData.HighScore = gameManager.HighScore;
        string filePath= GetFilePath("gameData.json");
        string jsonData = JsonConvert.SerializeObject(gameData);
        File.WriteAllText(filePath,jsonData);
#if UNITY_EDITOR
        Debug.Log("Data Saved: "+jsonData);    
#endif
    }

    public void LoadGameData()
    {
        string filePath=GetFilePath("gameData.json");
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            gameData = JsonConvert.DeserializeObject<GameData>(jsonData);
#if UNITY_EDITOR
            Debug.Log("Data Loaded: "+jsonData);
#endif
            gameManager.TotalCoins = gameData.coin;
            gameManager.isTutorialActive = gameData.tutorialActive;
            gameManager.HighScore = gameData.HighScore;
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("No Save Data");
#endif
            gameData = new GameData { coin = 1000, tutorialActive = true, HighScore = 0};
        }
    }

    private void ResetGameData()
    {
        gameData = new GameData { coin = 0, tutorialActive = true, HighScore = 0};
        gameManager.TotalCoins = gameData.coin;
        gameManager.isTutorialActive = gameData.tutorialActive;
        gameManager.HighScore = gameData.HighScore;
        SaveGameData();
#if UNITY_EDITOR
        Debug.Log("Game Data Reset");
#endif
    }
}
