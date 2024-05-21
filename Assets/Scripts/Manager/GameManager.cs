using System;
using Cinemachine;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public float stageSpeed = 5f;
    public bool isGameover;
    public bool isPaused;
    public bool isPlaying = false;
    public float distanceTravelled = 0;
    
    public bool isTutorialActive = true;
    
    public bool isFeverMode = false;
    public int CoinCount = 0;
    
    private UiManager uiManager;

    [Header("비활성화 시킬 오브젝트")]
    [SerializeField] public GameObject disableObject;
    //게임 시작전에 비출 카메라
    [SerializeField] public CinemachineVirtualCamera MenuCamera;
    //게임 시작후에 비출 카메라
    [SerializeField] public CinemachineVirtualCamera InGameCamera;
    
    
    public override void Awake()
    {
        base.Awake();
        uiManager = GetComponent<UiManager>();
    }

    public void Start()
    {
        disableObject.SetActive(false);
        MenuCamera.enabled = false;
        InGameCamera.enabled = true;
    }

    private void Update()
    {
        
    }

    public void GameOver()
    {
#if UNITY_EDITOR
        Debug.Log("Game Over");
#endif
        isGameover = true;
        isPlaying = false;
#if UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    public void RestartGame()
    {
        
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
    }
    
    public void AddCoin()
    {
        // 코인 획득 시 처리
        uiManager.UpdateCoinText(++CoinCount); 
    }
}