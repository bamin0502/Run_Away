using System;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;

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
    private bool isSaving;
    
    
    
    private void Awake()
    {
        PlayGamesPlatform.Activate();
        gameManager = GameObject.FindGameObjectWithTag("Manager")?.GetComponent<GameManager>();
        
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }
        SignInToGPGS();
    }

    private void SignInToGPGS()
    {
        Social.localUser.Authenticate(success =>
        {
            if (success)
            {
                LoadGameData();
            }
            else
            {
                InitializeDefaultGameData();
            }
        });
    }

    public void SaveGameData()
    {
        if (isSaving) return;

        if (gameManager == null || gameData == null)
        {
            Debug.LogError("GameManager or GameData not initialized!");
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
#if UNITY_EDITOR
        Debug.Log("Data saved: " + jsonData);    
#endif
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
                    InitializeDefaultGameData();
                }
            });
        }
        else
        {
            InitializeDefaultGameData();
        }
    }

    private void InitializeDefaultGameData()
    {
        gameData = new GameData { coin = 1000, tutorialActive = true, HighScore = 0 };
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
                Debug.Log("Game data successfully saved to Google Play Games Services");
            }
            else
            {
                Debug.LogError("Failed to save game data");
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
                Debug.LogError("Failed to load game data");
                InitializeDefaultGameData();
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
