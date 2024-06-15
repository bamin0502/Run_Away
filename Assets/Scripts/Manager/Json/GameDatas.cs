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
#if UNITY_EDITOR
        Debug.Log("로컬에 데이터 저장 중...");
#endif
        
        var json = JsonUtility.ToJson(dataSettings);
        File.WriteAllText(localFilePath, json);
#if UNITY_EDITOR
        Debug.Log("로컬 데이터 저장 완료: " + json);
#endif
        
    }

    private void SaveToCloud()
    {
#if UNITY_EDITOR
        Debug.Log("클라우드에 데이터 저장 중...");
#endif
        
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
#if UNITY_EDITOR
            Debug.Log("게임 데이터 열기 성공"); 
#endif
            
            var update = new SavedGameMetadataUpdate.Builder().Build();
            var json = JsonUtility.ToJson(dataSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
#if UNITY_EDITOR
            Debug.Log("클라우드에 데이터 저장 중: " + json); 
#endif
            
            savedGameClient.CommitUpdate(game, update, bytes, OnSavedGameWritten);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("게임 데이터 열기 실패: " + status);
#endif
            
        }
    }

    private void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
#if UNITY_EDITOR
            Debug.Log("게임 데이터 클라우드 저장 성공");
#endif
            
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("게임 데이터 클라우드 저장 실패: " + status); 
#endif
            
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
        Debug.Log("로컬 저장소에서 데이터 로드 중...");
        if (File.Exists(localFilePath))
        {
            var json = File.ReadAllText(localFilePath);
            dataSettings = JsonUtility.FromJson<DataSettings>(json);
#if UNITY_EDITOR
            Debug.Log("로컬 데이터 로드 완료: " + json);
#endif
            
            OnDataLoaded?.Invoke();
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("로컬 데이터 없음, 새로운 저장 생성."); 
#endif
            
            SaveToLocal();
            OnDataLoaded?.Invoke();
        }
    }

    private void LoadFromCloud()
    {
#if UNITY_EDITOR
        Debug.Log("클라우드에서 데이터 로드 중..."); 
#endif
        
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
#if UNITY_EDITOR
            Debug.Log("게임 데이터 로드 성공");
#endif
            
            savedGameClient.ReadBinaryData(data, OnSavedGameDataRead);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("게임 데이터 로드 실패: " + status); 
#endif 
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
#if UNITY_EDITOR
                Debug.Log("클라우드 데이터 없음, 새로운 저장 생성."); 
#endif
               
                SaveToCloud();
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("클라우드 데이터 로드 완료: " + data);
#endif
                
                dataSettings = JsonUtility.FromJson<DataSettings>(data);
                SaveToLocal();  // 로컬 데이터를 클라우드 데이터로 업데이트
                OnDataLoaded?.Invoke();
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("클라우드 데이터 읽기 실패: " + status);
#endif
            
            LoadFromLocal();  // 로컬 데이터로 대체
        }
    }

    public void DeleteData()
    {
        Debug.Log("데이터 삭제 시도 중...");
        DeleteGameData();
    }

    private void DeleteGameData()
    {
#if UNITY_EDITOR
        Debug.Log("게임 데이터 삭제를 위해 열기...");
#endif
        
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood, DeleteSaveGame);
    }

    private void DeleteSaveGame(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (status == SavedGameRequestStatus.Success)
        {
#if UNITY_EDITOR
            Debug.Log("게임 데이터 삭제 성공"); 
#endif
            
            savedGameClient.Delete(data);
            File.Delete(localFilePath);  
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("게임 데이터 삭제 실패: " + status); 
#endif
            
        }
    }
}
