using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using TMPro;
using UnityEngine;

public class GpgsManager : MonoBehaviour
{
    private string savedGameFilename = "save.bin";
    private string saveData = "세이브 로드 확인";

    void Awake()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        SignIn();
    }

    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(OnAuthentication);
    }

    void OnAuthentication(SignInStatus result)
    {
        if (result == SignInStatus.Success)
        {
            Debug.Log("Signed in!");
        }
        else
        {
            Debug.Log("Failed to sign in: " + result);
        }
    }

    public void OnSavedGameSelected(SelectUIStatus status, ISavedGameMetadata game)
    {
        if (status == SelectUIStatus.SavedGameSelected)
        {
            // handle selected game save
            OpenSavedGame(game.Filename, OnSavedGameOpenedForLoad);
            Debug.Log("Selected game: " + game.Description);

        }
        else
        {
            // handle cancel or error
            Debug.Log("Select UI error: " + status);
        }
    }

    public void SaveGame()
    {
        OpenSavedGame(savedGameFilename, OnSavedGameOpenedForSave);
    }

    void OnSavedGameOpenedForSave(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(saveData);
            SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                .WithUpdatedDescription("Saved at " + DateTime.Now.ToString())
                .Build();

            PlayGamesPlatform.Instance.SavedGame.CommitUpdate(game, update, data, OnSavedGameCommit);
        }
        else
        {
            Debug.Log("Failed to open saved game");
        }
    }

    void OnSavedGameCommit(SavedGameRequestStatus commitStatus, ISavedGameMetadata updatedGame)
    {
        Debug.Log(commitStatus == SavedGameRequestStatus.Success ? "Game saved successfully" : "Failed to save game");
    }

    public void LoadGame()
    {
        OpenSavedGame(savedGameFilename, OnSavedGameOpenedForLoad);
    }

    void OnSavedGameOpenedForLoad(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, OnSavedGameDataRead);
        }
        else
        {
            Debug.Log("Failed to open saved game");
        }
    }

    void OnSavedGameDataRead(SavedGameRequestStatus readStatus, byte[] data)
    {
        if (readStatus == SavedGameRequestStatus.Success)
        {
            string loadedData = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log("Loaded data: " + loadedData);
        }
        else
        {
            Debug.Log("Failed to read saved game data");
        }
    }

    private void OpenSavedGame(string filename, Action<SavedGameRequestStatus, ISavedGameMetadata> callback)
    {
        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            filename,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            callback);
    }
}
