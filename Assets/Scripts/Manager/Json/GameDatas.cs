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
    public static GameDatas Instance { get; private set; }
    public DataSettings dataSettings = new DataSettings();
    private string fileName = "saveData.dat";
    private string localFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            localFilePath = Path.Combine(Application.persistentDataPath, fileName);
        }
        else
        {
            Destroy(gameObject);
        }
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
        Debug.Log("로컬에 데이터 저장 중...");
        var json = JsonUtility.ToJson(dataSettings);
        File.WriteAllText(localFilePath, json);
        Debug.Log("로컬 데이터 저장 완료: " + json);
    }

    private void SaveToCloud()
    {
        Debug.Log("클라우드에 데이터 저장 중...");
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
            Debug.Log("게임 데이터 열기 성공");
            var update = new SavedGameMetadataUpdate.Builder().Build();
            var json = JsonUtility.ToJson(dataSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            Debug.Log("클라우드에 데이터 저장 중: " + json);
            savedGameClient.CommitUpdate(game, update, bytes, OnSavedGameWritten);
        }
        else
        {
            Debug.LogError("게임 데이터 열기 실패: " + status);
        }
    }

    private void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            Debug.Log("게임 데이터 클라우드 저장 성공");
        }
        else
        {
            Debug.LogError("게임 데이터 클라우드 저장 실패: " + status);
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
            Debug.Log("로컬 데이터 로드 완료: " + json);
        }
        else
        {
            Debug.Log("로컬 데이터 없음, 새로운 저장 생성.");
            SaveToLocal();
        }
    }

    private void LoadFromCloud()
    {
        Debug.Log("클라우드에서 데이터 로드 중...");
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
            Debug.Log("게임 데이터 로드 성공");
            savedGameClient.ReadBinaryData(data, OnSavedGameDataRead);
        }
        else
        {
            Debug.LogError("게임 데이터 로드 실패: " + status);
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
                Debug.Log("클라우드 데이터 없음, 새로운 저장 생성.");
                SaveToCloud();
            }
            else
            {
                Debug.Log("클라우드 데이터 로드 완료: " + data);
                dataSettings = JsonUtility.FromJson<DataSettings>(data);
                SaveToLocal();  // 로컬 데이터를 클라우드 데이터로 업데이트
                ApplyLoadedData();
            }
        }
        else
        {
            Debug.LogError("클라우드 데이터 읽기 실패: " + status);
            LoadFromLocal();  // 로컬 데이터로 대체
            ApplyLoadedData();
        }
    }

    public void DeleteData()
    {
        Debug.Log("데이터 삭제 시도 중...");
        DeleteGameData();
    }

    private void DeleteGameData()
    {
        Debug.Log("게임 데이터 삭제를 위해 열기...");
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood, DeleteSaveGame);
    }

    private void DeleteSaveGame(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (status == SavedGameRequestStatus.Success)
        {
            Debug.Log("게임 데이터 삭제 성공");
            savedGameClient.Delete(data);
            File.Delete(localFilePath);  // 로컬 데이터도 삭제
        }
        else
        {
            Debug.LogError("게임 데이터 삭제 실패: " + status);
        }
    }

    private void ApplyLoadedData()
    {
        GameManager.Instance?.ApplyLoadedData();
    }
}
