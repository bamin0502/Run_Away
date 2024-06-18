using System;
using System.Collections;
using Cinemachine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UniRx;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

public class GameManager : MonoBehaviour
{
    [Header("불러올 스크립트")]
    private UiManager uiManager;
    private PlayerMovement playerMovement;
    private SoundManager soundManager;
    private GameDatas gameDatas;

    [Header("게임 속도 관련 필드")]
    public float stageSpeed = 5f;
    public float distanceTravelled = 0;

    [Header("게임 상태 관련 필드")]
    public bool isGameover;
    public bool isPaused;
    public bool isPlaying = false;

    [Header("튜토리얼 관련 진행여부 필드")]
    public bool isTutorialActive = true;

    [Header("피버모드 관련 필드")]
    public bool isFeverMode = false;
    private readonly int coinsForFever = 100;
    private bool feverActivatedByItem = false;
    private int coinFeverCount;

    [Header("코인 관련 필드")]
    public int CurrentGameCoins;
    public int TotalCoins = 1000;

    [Header("게임 점수 관련 필드")]
    public int CurrentScore;
    public int scorePerDistance = 10;
    public int HighScore;

    [Header("부활 관련 무적 시간")]
    public float invincibilityDuration = 2f;

    [Header("비활성화 시킬 오브젝트")]
    [SerializeField] public GameObject disableObject;
    [Header("시작전에 비출 카메라")]
    [SerializeField] public CinemachineVirtualCamera MenuCamera;
    [Header("시작후에 비출 카메라")]
    [SerializeField] public CinemachineVirtualCamera InGameCamera;

    [Header("점프 관련 필드")]
    private float initialJumpPower;
    private float maxJumpPower = 20f;

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

    public void Awake()
    {
        uiManager = GameObject.FindGameObjectWithTag("UiManager").GetComponent<UiManager>();
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        soundManager = GameObject.FindGameObjectWithTag("Sound").GetComponent<SoundManager>();
        gameDatas = GameObject.FindGameObjectWithTag("Data").GetComponent<GameDatas>();

        if (playerMovement != null)
        {
            initialJumpPower = playerMovement.jumpForce;
        }

        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        SignIn();
    }

    private void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(OnAuthentication);
    }

    private void OnAuthentication(SignInStatus result)
    {
        if (result == SignInStatus.Success)
        {
#if UNITY_EDITOR
            Debug.Log("Signed in!");
#endif
            uiManager.ShowLoadingImage();
            gameDatas.LoadData();
            gameDatas.OnDataLoaded += OnDataLoaded; // 데이터 로드 완료시 호출
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("Failed to sign in: " + result);
#endif
            uiManager.ShowLoadingImage();
            gameDatas.LoadFromLocal();
            OnDataLoaded(); // 로컬 데이터 로드 완료시 호출
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if(uiManager.GameMenuPanel.activeSelf)
        {
            return;
        }
        
        if (pauseStatus || !uiManager.PausePanel.activeSelf || !uiManager.GameOverPanel.activeSelf)
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
        
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
#if UNITY_EDITOR && UNITY_ANDROID
            Debug.Log("사용자가 구글 플레이에 로그인되어 있습니다.");
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("사용자가 구글 플레이에 로그인되어 있지 않습니다.");
#endif
        }

        if (gameDatas != null)
        {
            ApplyLoadedData();
#if UNITY_EDITOR
            Debug.Log("데이터 로드 완료" + JsonUtility.ToJson(gameDatas.dataSettings)); 
#endif
        }

        if (InGameCamera != null)
        {
            InGameCamera.enabled = false;
        }

        CurrentScore = 0;
        uiManager.UpdateScoreText(CurrentScore);
        soundManager.PlayBgm(0);
    }
    
    private void ApplyLoadedData()
    {
        if (gameDatas != null)
        {
            DataSettings loadedData = gameDatas.dataSettings;
            TotalCoins = loadedData.gold;
            HighScore = loadedData.highScore;
            isTutorialActive = loadedData.isTutorial;

            uiManager.UpdateAllCoinText(TotalCoins);
            uiManager.UpdateHighScoreText(HighScore);
        }
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
        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            uiManager.UpdateHighScoreText(HighScore);
        }

        if (CurrentGameCoins >= 100)
        {
            PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement__100,1, success => { }); 
        }
        if(CurrentGameCoins >= 300)
        {
            PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement__300,1, success => { }); 
        }
        
        
        
        SaveGameData();
#if UNITY_ANDROID
        Handheld.Vibrate();
#endif
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
        if (gameDatas != null)
        {
#if UNITY_EDITOR
            Debug.Log("게임 데이터 저장 중...");
#endif
            gameDatas.dataSettings.gold = TotalCoins;
            gameDatas.dataSettings.highScore = HighScore;
            gameDatas.dataSettings.isTutorial = isTutorialActive;
            
            gameDatas.SaveData();
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("GameDatas가 초기화되지 않았습니다!"); 
#endif
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        
        var tileManager = GameObject.FindGameObjectWithTag("TileManager").GetComponent<Tile>();
        tileManager.ResumeTileMovement();
    }

    public void AddCoin()
    {
        CurrentGameCoins++;
        TotalCoins++;
        uiManager.UpdateCoinText(CurrentGameCoins);
        soundManager.PlaySfx(0);
        
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
        if (playerMovement != null && playerMovement.jumpForce < maxJumpPower)
        {
            currentJumpEffect?.Dispose();
            blinkSubscription?.Dispose();

            jumpEffect.Play();

            blinkSubscription = Observable.Timer(TimeSpan.FromSeconds(duration - 3))
                .SelectMany(_ => Observable.Interval(TimeSpan.FromSeconds(0.1f)).TakeWhile(t => t < 10))
                .Subscribe(t =>
                {
                    if (jumpEffect != null)
                    {
                        if (jumpEffect.isPlaying)
                        {
                            jumpEffect.Stop();
                        }
                        else
                        {
                            jumpEffect.Play();
                        }
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
    }

    private void ResetJumpPower()
    {
        if (playerMovement != null)
        {
            playerMovement.jumpForce = initialJumpPower;
            jumpEffect.Stop();
        }
    }

    public void ActivateMagnetEffect(float duration)
    {
        if (!IsMagnetEffectActive.Value)
        {
            currentMagnetEffect?.Dispose();
            blinkSubscription?.Dispose();

            IsMagnetEffectActive.Value = true;
            magnetEffect.Play();

            blinkSubscription = Observable.Timer(TimeSpan.FromSeconds(duration - 3))
                .SelectMany(_ => Observable.Interval(TimeSpan.FromSeconds(0.1f)).TakeWhile(t => t < 10))
                .Subscribe(t =>
                {
                    if (magnetEffect != null)
                    {
                        if (magnetEffect.isPlaying)
                        {
                            magnetEffect.Stop();
                        }
                        else
                        {
                            magnetEffect.Play();
                        }
                    }
                });

            currentMagnetEffect = Observable.Timer(TimeSpan.FromSeconds(duration))
                .Subscribe(_ =>
                {
                    IsMagnetEffectActive.Value = false;
                    magnetEffect.Stop();
                });
        }
    }

    public void ActivateFeverMode(float duration)
    {
        if (!IsFeverModeActive.Value && !isFeverMode)
        {
            isFeverMode = true;
            currentFeverEffect?.Dispose();
            blinkSubscription?.Dispose();

            IsFeverModeActive.Value = true;
            feverEffect.Play();

            uiManager.ShowFeverText();

            blinkSubscription = Observable.Timer(TimeSpan.FromSeconds(duration - 3))
                .SelectMany(_ => Observable.Interval(TimeSpan.FromSeconds(0.1f)).TakeWhile(t => t < 10))
                .Subscribe(t =>
                {
                    if (feverEffect != null)
                    {
                        if (feverEffect.isPlaying)
                        {
                            feverEffect.Stop();
                        }
                        else
                        {
                            feverEffect.Play();
                        }
                    }
                });

            currentFeverEffect = Observable.Timer(TimeSpan.FromSeconds(duration))
                .ObserveOnMainThread()
                .Subscribe(_ =>
                {
                    uiManager.HideFeverText();
                    IsFeverModeActive.Value = false;
                    isFeverMode = false;
                    coinFeverCount = 0;
                    feverEffect.Stop();
                    uiManager.ResetFeverGuage();
                });
        }
    }

    public void ActivateFeverModeByItem(float duration)
    {
        if (!isFeverMode && !IsFeverModeActive.Value)
        {
            ActivateFeverMode(duration);
        }
    }

    private void Update()
    {
        if (isPlaying)
        {
            distanceTravelled += stageSpeed * Time.deltaTime;
            CurrentScore = (int)distanceTravelled * scorePerDistance;
            uiManager.UpdateScoreText(CurrentScore);
        }

        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            uiManager.UpdateHighScoreText(HighScore);
            
        }
        
        if (!isTutorialActive)
        {
            Social.ReportProgress(GPGSIds.achievement, 100f, success => { });
        }
        
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ActivateCheat();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            IncreaseJumpPower(5f, 10f);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ActivateMagnetEffect(10f);
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ActivateFeverModeByItem(10f);
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ActivateFeverMode(10f);
        }
#endif
    }

    private void CheckAchievements()
    {
        if (TotalCoins >= 100000)
        {
            PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_8, success => { });
            Social.ReportProgress(GPGSIds.achievement_8, 100f, success => { });
        }
        else if (TotalCoins >= 50000)
        {
            PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_7, success => { });
            Social.ReportProgress(GPGSIds.achievement_7, 100f, success => { });
        }
        else if (TotalCoins >= 10000)
        {
            PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_6, success => { });
            Social.ReportProgress(GPGSIds.achievement_6, 100f, success => { });
        }
        else if (TotalCoins >= 5000)
        {
            PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_5, success => { });
            Social.ReportProgress(GPGSIds.achievement_5, 100f, success => { });
        }
        else if (TotalCoins >= 3000)
        {
            PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_4, success => { });
            Social.ReportProgress(GPGSIds.achievement_4, 100f, success => { });
        }
        else if (TotalCoins >= 1000)
        {
            PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_3, success => { });
            Social.ReportProgress(GPGSIds.achievement_3, 100f, success => { });
        }
    }

    public void RevivePlayer()
    {
        if (TotalCoins >= 300)
        {
            TotalCoins -= 300;
            uiManager.UpdateAllCoinText(TotalCoins);

            playerMovement.Revive();

            uiManager.HideRevivePanel();
            if (uiManager != null) uiManager.GameOverPanel.SetActive(false);
            isGameover = false;
            isPlaying = true;
            isFeverMode = false;
            IsFeverModeActive.Value = false;
            
            PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement_2,1, success => { }); 
            
            StartCoroutine(StartInvincibility());
            Time.timeScale = 1;
        }
    }

    private IEnumerator StartInvincibility()
    {
        if (playerMovement != null)
        {
            playerMovement.SetInvincible(true);
            yield return new WaitForSeconds(invincibilityDuration);
            playerMovement.SetInvincible(false);
        }
    }

#if UNITY_EDITOR
    private void ActivateCheat()
    {
        int coinsToAdd = 1000;
        AddCheatCoins(coinsToAdd);

        Debug.Log("치트 활성화! 추가된 코인: " + coinsToAdd);
    }

    private void AddCheatCoins(int coinsToAdd)
    {
        TotalCoins += coinsToAdd;
        uiManager.UpdateAllCoinText(TotalCoins);
    }
#endif

    private void OnDataLoaded()
    {
        ApplyLoadedData();
        uiManager.HideLoadingImage();
        if (uiManager != null) uiManager.EnableStartButton(); // 데이터가 로드된 후에 시작 버튼 활성화
        
        PlayGamesPlatform.Instance.ReportScore(gameDatas.dataSettings.highScore, GPGSIds.leaderboard, success => { });
        //PlayGamesPlatform.Instance.ReportScore(gameDatas.dataSettings.gold, GPGSIds.leaderboard_2, success => { });
        CheckAchievements();
    }

    public void ShowAchievements()
    {
        PlayGamesPlatform.Instance.ShowAchievementsUI();
        isPaused = false;
        uiManager.HidePausePanel();
    }

    public void ShowLeaderBoard()
    {
        PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard);
        isPaused = false;
        uiManager.HidePausePanel();
    }
}
