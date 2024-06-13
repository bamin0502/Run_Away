using System;
using System.IO;
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine;

public class DataSettings
{
    public int gold = 0;
    public int highScore = 0;
    public bool isTutorial = true;
}

public class GameDatas : MonoBehaviour
{
    public DataSettings dataSettings = new DataSettings();
    private string fileName = "saveData.dat";
    private string localFilePath;

    public event Action OnDataLoaded; 

    private void Awake()
    {
        localFilePath = Path.Combine(Application.persistentDataPath, fileName);
    }

    public void SaveData()
    {
        SaveToLocal();
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            SaveToCloud();
        }
    }

    private void SaveToLocal()
    {
        var json = JsonUtility.ToJson(dataSettings);
        File.WriteAllText(localFilePath, json);
    }

    private void SaveToCloud()
    {
        OpenSaveGame();
    }

    private void OpenSaveGame()
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood, OnSavedGameOpened);
    }

    private void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (status == SavedGameRequestStatus.Success)
        {
            var update = new SavedGameMetadataUpdate.Builder().Build();
            var json = JsonUtility.ToJson(dataSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            savedGameClient.CommitUpdate(game, update, bytes, OnSavedGameWritten);
        }
        else
        {

        }
    }

    private void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        if (status == SavedGameRequestStatus.Success)
        {

        }
        else
        {

        }
    }

    public void LoadData()
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            LoadFromCloud();
        }
        else
        {
            LoadFromLocal();
        }
    }

    public void LoadFromLocal()
    {

        if (File.Exists(localFilePath))
        {
            var json = File.ReadAllText(localFilePath);
            dataSettings = JsonUtility.FromJson<DataSettings>(json);

            OnDataLoaded?.Invoke(); 
        }
        else
        {

            SaveToLocal();
        }
    }

    private void LoadFromCloud()
    {
        OpenLoadGame();
    }

    private void OpenLoadGame()
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood, LoadGameData);
    }

    private void LoadGameData(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (status == SavedGameRequestStatus.Success)
        {
            savedGameClient.ReadBinaryData(data, OnSavedGameDataRead);
        }
        else
        {
            LoadFromLocal();  // 로컬 데이터로 대체
        }
    }

    private void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] loadedData)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            string data = Encoding.UTF8.GetString(loadedData);
            if (string.IsNullOrEmpty(data))
            {

                SaveToCloud();
            }
            else
            {

                dataSettings = JsonUtility.FromJson<DataSettings>(data);
                SaveToLocal();  // 로컬 데이터를 클라우드 데이터로 업데이트
                OnDataLoaded?.Invoke(); 
            }
        }
        else
        {
            LoadFromLocal();  // 로컬 데이터로 대체
        }
    }

    public void DeleteData()
    {
        DeleteGameData();
    }

    private void DeleteGameData()
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood, DeleteSaveGame);
    }

    private void DeleteSaveGame(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (status == SavedGameRequestStatus.Success)
        {
            savedGameClient.Delete(data);
            File.Delete(localFilePath);  // 로컬 데이터도 삭제
        }
        else
        {

        }
    }
}
