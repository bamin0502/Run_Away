using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;

public class GameData
{
    public int coin;
    public bool tutorialActive;
    public int HighScore;
}

public class JsonData : MonoBehaviour
{
    private GameData gameData=new GameData();
    
    private GameManager gameManager;
    private bool isSaving;
    private const string localFileName = "localGameData.json";

    private void Awake()
    {
        SignIn();
    }

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager")?.GetComponent<GameManager>();
        
        if (gameManager == null)
        {
#if UNITY_EDITOR
            Debug.Log("GameManager not found!");
#endif
        }
    }

    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(OnAuthentication);
    }
    
    void OnAuthentication(SignInStatus result)
    {
        if (result == SignInStatus.Success)
        {
            LoadGameData();
        }
        else
        {
            LoadLocalGameData();
        }
    }

    public void SaveGameData()
    {
        if (isSaving) return;

        if (gameManager == null || gameData == null)
        {
            Debug.Log("GameManager or GameData not initialized!");
            return;
        }

        gameData.coin = gameManager.TotalCoins;
        gameData.tutorialActive = gameManager.isTutorialActive;
        gameData.HighScore = gameManager.HighScore;

        string jsonData = JsonConvert.SerializeObject(gameData);

        if (PlayGamesPlatform.Instance.IsAuthenticated())
        {
            isSaving = true;
            OpenSavedGame("gameData", (status, game) => 
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
                    SaveGame(game, data);
                }
                else
                {
                    Debug.LogError("Failed to open saved game");
                    isSaving = false;
                }
            });
        }
        else
        {
            SaveLocalGameData(jsonData);
#if UNITY_EDITOR
            Debug.Log("Data saved locally: " + jsonData);    
#endif
        }
    }

    private void SaveLocalGameData(string jsonData)
    {
        try
        {
            File.WriteAllText(Path.Combine(Application.persistentDataPath, localFileName), jsonData);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save local game data: " + e.Message);
        }
    }

    public void LoadGameData()
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
        {
            OpenSavedGame("gameData", (status, game) => 
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    LoadGame(game);
                }
                else
                {
                    Debug.LogError("Failed to open saved game");
                    LoadLocalGameData();
                }
            });
        }
    }

    private void LoadLocalGameData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, localFileName);
        if (File.Exists(filePath))
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);
                gameData = JsonConvert.DeserializeObject<GameData>(jsonData);
                ApplyGameDataToManager();
#if UNITY_EDITOR
                Debug.Log("Data loaded from local file: " + jsonData);
#endif
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.Log("Failed to load local game data: " + e.Message);   
#endif
                
                InitializeDefaultGameData();
            }
        }
        else
        {
            InitializeDefaultGameData();
        }
    }

    private void InitializeDefaultGameData()
    {
        gameData = new GameData { coin = 0, tutorialActive = true, HighScore = 0 };
        ApplyGameDataToManager();
#if UNITY_EDITOR
        Debug.Log("No saved data, initialized default game data");
#endif
    }

    private void ApplyGameDataToManager()
    {
        if (gameManager)
        {
            gameManager.TotalCoins = gameData.coin;
            gameManager.isTutorialActive = gameData.tutorialActive;
            gameManager.HighScore = gameData.HighScore;
        }
    }

    private void OpenSavedGame(string filename, Action<SavedGameRequestStatus, ISavedGameMetadata> callback)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, callback);
    }

    private void SaveGame(ISavedGameMetadata game, byte[] savedData)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder().Build();
        savedGameClient.CommitUpdate(game, update, savedData, (status, metadata) =>
        {
            if (status == SavedGameRequestStatus.Success)
            {
#if UNITY_EDITOR
                Debug.Log("Game data successfully saved to Google Play Games Services");
#endif
                
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("Failed to save game data");   
#endif
                
            }
            isSaving = false;
        });
    }

    private void LoadGame(ISavedGameMetadata game)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, (status, data) =>
        {
            if (status == SavedGameRequestStatus.Success)
            {
                string jsonData = System.Text.Encoding.UTF8.GetString(data);
                gameData = JsonConvert.DeserializeObject<GameData>(jsonData);
                ApplyGameDataToManager();
#if UNITY_EDITOR
                Debug.Log("Data loaded: " + jsonData);
#endif
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("Failed to load game data");
#endif
                
                LoadLocalGameData();
            }
        });
    }

    private void ResetGameData()
    {
        gameData = new GameData { coin = 0, tutorialActive = true, HighScore = 0 };
        ApplyGameDataToManager();
        SaveGameData();
#if UNITY_EDITOR
        Debug.Log("Game data reset");
#endif
    }
}
