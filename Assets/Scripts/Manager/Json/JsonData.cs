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
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        
        PlayGamesPlatform.Activate();
        
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
                    Debug.LogError("저장 게임을 열기 실패");
                }
            });
        }
#if UNITY_EDITOR
        Debug.Log("데이터 저장됨: " + jsonData);    
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
                    Debug.LogError("저장 게임을 열기 실패");
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
        gameManager.TotalCoins = gameData.coin;
        gameManager.isTutorialActive = gameData.tutorialActive;
        gameManager.HighScore = gameData.HighScore;
#if UNITY_EDITOR
        Debug.Log("저장 데이터 없음, 기본 게임 데이터 초기화");
#endif
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
                Debug.Log("게임 데이터가 Google Play Games Services에 성공적으로 저장됨");
            }
            else
            {
                Debug.LogError("게임 데이터 저장 실패");
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

                gameManager.TotalCoins = gameData.coin;
                gameManager.isTutorialActive = gameData.tutorialActive;
                gameManager.HighScore = gameData.HighScore;

#if UNITY_EDITOR
                Debug.Log("데이터 로드됨: " + jsonData);
#endif
            }
            else
            {
                Debug.LogError("게임 데이터 로드 실패");
                InitializeDefaultGameData();
            }
        });
    }

    private void ResetGameData()
    {
        gameData = new GameData { coin = 0, tutorialActive = true, HighScore = 0 };
        gameManager.TotalCoins = gameData.coin;
        gameManager.isTutorialActive = gameData.tutorialActive;
        gameManager.HighScore = gameData.HighScore;
        SaveGameData();
#if UNITY_EDITOR
        Debug.Log("게임 데이터 리셋됨");
#endif
    }
}
