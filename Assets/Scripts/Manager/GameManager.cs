using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UniRx;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("불러올 스크립트")]
    private UiManager uiManager;
    private JsonData jsonData;
    private PlayerMovement playerMovement;
    
    [Header("게임 속도 관련 필드")]
    public float stageSpeed = 5f;
    public float distanceTravelled = 0;
    
    [Header("게임 상태 관련 필드")]
    public bool isGameover;
    public bool isPaused;
    public bool isPlaying = false;
    
    // [Header("타일 관련 필드")]
    // public TileManager tileManager;
    
    [Header("튜토리얼 관련 진행여부 필드")]
    public bool isTutorialActive = true;
    
    [Header("피버모드 관련 필드")]
    public bool isFeverMode = false;
    private int coinsForFever = 100;
    private bool feverActivatedByItem = false;
    private int coinFeverCount = 0;
    
    [Header("코인 관련 필드")] 
    public int CurrentGameCoins = 0;
    public int TotalCoins  = 1000;
    
    [Header("게임 점수 관련 필드")]
    public int CurrentScore = 0;
    public int scorePerDistance = 10;
    public int HighScore = 0;
    
    [Header("부활 관련 무적 시간")]
    public float invincibilityDuration = 2f;
    
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
    public ReactiveProperty<bool> IsMagnetEffectActive { get; private set; } = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> IsFeverModeActive { get; private set; } = new ReactiveProperty<bool>();
    private float initialJumpPower;
    private float maxJumpPower = 20f;
    public void Awake()
    {
        uiManager = GameObject.FindGameObjectWithTag("UiManager").GetComponent<UiManager>();
        jsonData = GameObject.FindGameObjectWithTag("UiManager").GetComponent<JsonData>();
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        initialJumpPower = playerMovement.jumpForce;
        //tileManager = GameObject.FindGameObjectWithTag("TileManager").GetComponent<TileManager>();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            isPaused = true;
            uiManager.ShowPausePanel();
        }
        else
        {
            isPaused = false;
        }
    }

    public void Start()
    {
        disableObject.SetActive(false);
        jsonData.LoadGameData();

        InGameCamera.enabled = false;
        CurrentScore = 0;
        uiManager.UpdateScoreText(CurrentScore);
        SoundManager.Instance.PlayBgm(0);
        
        //tileManager.Initialize();
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
        // 2초 동안 기다리는 로직 구현
        StartCoroutine(ShowGameOverPanelAfterDelay(2f));
       
    }
    
    private IEnumerator ShowGameOverPanelAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        uiManager.UpdateResultCoinText(CurrentGameCoins);
        uiManager.UpdateResultScoreText(CurrentScore);
        uiManager.ShowGameOverPanel();
    }
    public void OnHomeButtonClick()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);

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
        CurrentGameCoins++;
        TotalCoins++;
        uiManager.UpdateCoinText(CurrentGameCoins);
        
        if (!isFeverMode)
        {
            coinFeverCount++;
            
            if (coinFeverCount >= coinsForFever)
            {
                uiManager.UpdateFeverGauge(1);
            }
            else
            {
                uiManager.UpdateFeverGauge(coinFeverCount / (float)coinsForFever);
            }
        }
    }
    
    public void IncreaseJumpPower(float amount, float duration)
    {
        if (playerMovement.jumpForce >= maxJumpPower)
        {
            return; 
        }
        
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
        if (playerMovement.jumpForce > initialJumpPower)
        {
            playerMovement.jumpForce = initialJumpPower;
        }

        playerMovement.AdjustJumpPower(amount);
        
        if (playerMovement.jumpForce > maxJumpPower)
        {
            playerMovement.jumpForce = maxJumpPower;
        }

        currentJumpEffect = Observable.Timer(TimeSpan.FromSeconds(duration))
            .Subscribe(_ => ResetJumpPower());
            
    }

    private void ResetJumpPower()
    {
        playerMovement.jumpForce = initialJumpPower;
        jumpEffect.Stop();
    }

    public void ActivateMagnetEffect(float duration)
    {
        if (IsMagnetEffectActive.Value)
        {
            return;
        }
        
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
            .Subscribe(_ =>
            {
                IsMagnetEffectActive.Value = false;
                magnetEffect.Stop();
            });
            
    }

    public void ActivateFeverMode(float duration)
    {
        if (IsFeverModeActive.Value)
        {
            return;
        }

        isFeverMode = true;
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
            .Subscribe(_ =>
            {
                IsFeverModeActive.Value = false;
                isFeverMode = false;
                coinFeverCount = 0;
                feverEffect.Stop();
                uiManager.ResetFeverGuage();
            });
    }
    
    public void ActivateFeverModeByItem(float duration)
    {
        ActivateFeverMode(duration);
    }
    
    private void Update()
    {
        if (isPlaying)
        {
            distanceTravelled += stageSpeed * Time.deltaTime;
            CurrentScore = (int) distanceTravelled * scorePerDistance;
            uiManager.UpdateScoreText(CurrentScore);
            
            //tileManager.UpdateTiles(stageSpeed);
        }
        
        if(CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            uiManager.UpdateHighScoreText(HighScore);
        }
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ActivateCheat();
        } 
#endif
        

    }

    public void RevivePlayer()
    {
        if (TotalCoins >= 300)
        {
            TotalCoins -= 300;
            uiManager.UpdateAllCoinText(TotalCoins);
            
            playerMovement.Revive();
            
            uiManager.HideRevivePanel();
            uiManager.GameOverPanel.SetActive(false);
            isGameover = false;
            isPlaying = true;
            isFeverMode = false;
            IsFeverModeActive.Value = false;
            
            StartCoroutine(StartInvincibility());
            
            Time.timeScale = 1;
        }
    }

    private IEnumerator StartInvincibility()
    {
        playerMovement.SetInvincible(true);
        yield return new WaitForSeconds(invincibilityDuration);
        playerMovement.SetInvincible(false);    
    }
#if UNITY_EDITOR
    private void ActivateCheat()
    {
        int coinsToAdd = 1000;
        AddCheatCoins(coinsToAdd);

        Debug.Log("Cheat activated! Coins added: " + coinsToAdd);
    }

    private void AddCheatCoins(int coinsToAdd)
    {
        TotalCoins += coinsToAdd;
        uiManager.UpdateAllCoinText(TotalCoins);
    }
#endif
    
}
