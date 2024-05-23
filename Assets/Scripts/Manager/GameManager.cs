using System;
using Cinemachine;
using UnityEngine;
using TMPro;
using UniRx;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float stageSpeed = 5f;
    public bool isGameover;
    public bool isPaused;
    public bool isPlaying = false;
    public float distanceTravelled = 0;
    
    public bool isTutorialActive = true;
    
    public bool isFeverMode = false;
    
    public int HighScore = 0;


    [Header("코인 관련 필드")] 
    public int CurrentGameCoins = 0;
    public int TotalCoins  = 0;
    
    [Header("게임 점수 관련 필드")]
    public int CurrentScore = 0;
    public int scorePerDistance = 10;
    
    
    private UiManager uiManager;
    private JsonData jsonData;
    [Header("비활성화 시킬 오브젝트")]
    [SerializeField] public GameObject disableObject;
    [Header("시작전에 비출 카메라")] 
    [SerializeField] public CinemachineVirtualCamera MenuCamera;
    [Header("시작후에 비출 카메라")] 
    [SerializeField] public CinemachineVirtualCamera InGameCamera;

    [Header("아이템 이펙트 관련")]
    private IDisposable currentJumpEffect;
    private IDisposable currentMagnetEffect;
    private IDisposable currentFeverEffect;
    public ParticleSystem jumpEffect;
    public ParticleSystem magnetEffect;
    public ParticleSystem feverEffect;
    private IDisposable blinkSubscription;
    
    private PlayerMovement playerMovement;

    public ReactiveProperty<bool> IsMagnetEffectActive { get; private set; } = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> IsFeverModeActive { get; private set; } = new ReactiveProperty<bool>();
    
    public void Awake()
    {
        uiManager = GameObject.FindGameObjectWithTag("UiManager").GetComponent<UiManager>();
        jsonData = GameObject.FindGameObjectWithTag("UiManager").GetComponent<JsonData>();
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    public void Start()
    {
        disableObject.SetActive(false);
        jsonData.LoadGameData();

        InGameCamera.enabled = false;
        CurrentScore = 0;
        uiManager.UpdateScoreText(CurrentScore);
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
        if(CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            uiManager.UpdateHighScoreText(HighScore);
        }
        
        SaveGameData();
#if UNITY_ANDROID
        Handheld.Vibrate();
#endif
        uiManager.UpdateResultCoinText(CurrentGameCoins);
        uiManager.UpdateResultScoreText(CurrentScore);
        uiManager.ShowGameOverPanel();
    }
    public void OnHomeButtonClick()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
        // 로비로 돌아갈 때 총 코인 업데이트
        uiManager.UpdateAllCoinText(TotalCoins);
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
        CurrentGameCoins++;
        TotalCoins++;
        uiManager.UpdateCoinText(CurrentGameCoins);
    }
    
    public void IncreaseJumpPower(float amount, float duration)
    {
        currentJumpEffect?.Dispose();
        blinkSubscription?.Dispose();
        
        jumpEffect.Play();
        
        blinkSubscription = Observable.Timer(TimeSpan.FromSeconds(duration - 3))
            .SelectMany(_ => Observable.Interval(TimeSpan.FromSeconds(0.1f)).TakeWhile(t => t < 10))
            .Subscribe(t =>
            {
                if (jumpEffect.isPlaying)
                {
                    jumpEffect.Stop();
                }
                else
                {
                    jumpEffect.Play();
                }
            });

        playerMovement.AdjustJumpPower(amount);
        currentJumpEffect = Observable.Timer(TimeSpan.FromSeconds(duration))
            .Subscribe(_ => ResetJumpPower(amount));
    }

    private void ResetJumpPower(float amount)
    {
        playerMovement.AdjustJumpPower(-amount);
    }

    public void ActivateMagnetEffect(float duration)
    {
        currentMagnetEffect?.Dispose();
        blinkSubscription?.Dispose();
        
        IsMagnetEffectActive.Value = true;
        magnetEffect.Play();
        
        blinkSubscription = Observable.Timer(TimeSpan.FromSeconds(duration - 3))
            .SelectMany(_ => Observable.Interval(TimeSpan.FromSeconds(0.1f)).TakeWhile(t => t < 10))
            .Subscribe(t =>
            {
                if (magnetEffect.isPlaying)
                {
                    magnetEffect.Stop();
                }
                else
                {
                    magnetEffect.Play();
                }
            });
        currentMagnetEffect = Observable.Timer(TimeSpan.FromSeconds(duration))
            .Subscribe(_ => IsMagnetEffectActive.Value = false);
    }
    
    public void ActivateFeverMode(float duration)
    {
        currentFeverEffect?.Dispose();
        blinkSubscription?.Dispose();
        
        IsFeverModeActive.Value = true;
        feverEffect.Play();
        
        blinkSubscription = Observable.Timer(TimeSpan.FromSeconds(duration - 3))
            .SelectMany(_ => Observable.Interval(TimeSpan.FromSeconds(0.1f)).TakeWhile(t => t < 10))
            .Subscribe(t =>
            {
                if (feverEffect.isPlaying)
                {
                    feverEffect.Stop();
                }
                else
                {
                    feverEffect.Play();
                }
            });
        currentFeverEffect = Observable.Timer(TimeSpan.FromSeconds(duration))
            .Subscribe(_ => IsFeverModeActive.Value = false);
    }
    
    private void Update()
    {
        if (isPlaying)
        {
            distanceTravelled += stageSpeed * Time.deltaTime;
            CurrentScore = (int) distanceTravelled * scorePerDistance;
            uiManager.UpdateScoreText(CurrentScore);
        }
    }


}
