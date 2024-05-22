using System;
using Cinemachine;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float stageSpeed = 5f;
    public bool isGameover;
    public bool isPaused;
    public bool isPlaying = false;
    public float distanceTravelled = 0;
    
    public bool isTutorialActive = true;
    
    public bool isFeverMode = false;
    public int CoinCount = 0;
    public int HighScore = 0;
    
    private UiManager uiManager;
    private JsonData jsonData;
    [Header("비활성화 시킬 오브젝트")]
    [SerializeField] public GameObject disableObject;
    [Header("시작전에 비출 카메라")] 
    [SerializeField] public CinemachineVirtualCamera MenuCamera;
    [Header("시작후에 비출 카메라")] 
    [SerializeField] public CinemachineVirtualCamera InGameCamera;
    
    
    public void Awake()
    {
        uiManager = GameObject.FindGameObjectWithTag("UiManager").GetComponent<UiManager>();
        jsonData = GameObject.FindGameObjectWithTag("UiManager").GetComponent<JsonData>();
    }

    public void Start()
    {
        disableObject.SetActive(false);
        jsonData.LoadGameData();

        InGameCamera.enabled = false;
        
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }
    
    public void GameOver()
    {
#if UNITY_EDITOR
        Debug.Log("Game Over");
#endif
        isGameover = true;
        isPlaying = false;
        SaveGameData();
#if UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    public void SaveGameData()
    {
        jsonData.SaveGameData();
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
    }
    
    public void AddCoin()
    {
        // 코인 획득 시 처리
        uiManager.UpdateCoinText(CoinCount++);
        
    }
}