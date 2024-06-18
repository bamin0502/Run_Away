using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    [Header("Game Manager"), Tooltip("게임 매니저 오브젝트")]
    private GameManager gameManager;
    [Header("Tutorial Manager"), Tooltip("튜토리얼 매니저 오브젝트")]
    private TutorialManager tutorialManager;
    
    [Header("Sound Manager")]
    private SoundManager soundManager;
    

    [Header("UI Elements"), Tooltip("패널 관련 오브젝트들")]
    [SerializeField] public GameObject PausePanel;
    [SerializeField] public GameObject GameOverPanel;
    [SerializeField] public GameObject GamePanel;
    [SerializeField] public GameObject GameMenuPanel;
    [SerializeField] public GameObject RevivePanel;
    [SerializeField] public GameObject QuitPanel;
    [SerializeField] public GameObject TextPanel;

    [Header("Pause Panel Ui Button"), Tooltip("일시정지 패널 버튼들")]
    [SerializeField] public Button homeButton;
    [SerializeField] public Button resumeButton;
    [SerializeField] public Button quitButton;

    [Header("Game Over Panel Ui Button"), Tooltip("게임오버 패널 버튼들")]
    [SerializeField] private Button ReviveButton;
    [SerializeField] private Button LobbyButton;

    [Header("Game Over Panel Ui Text"), Tooltip("게임오버 패널 텍스트들")]
    [SerializeField] public TextMeshProUGUI scoreText;
    [SerializeField] public TextMeshProUGUI resultCoinText;

    [Header("Game UI"), Tooltip("게임 패널 텍스트들")]
    [SerializeField] public TextMeshProUGUI coinText;
    [SerializeField] public Button optionButton;
    [SerializeField] public TextMeshProUGUI GameScoreText;
    [SerializeField] public TextMeshProUGUI HighGameScoreText;
    [SerializeField] public Image FeverGauge;

    [Header("Game Panel Ui Text"), Tooltip("게임 결과 패널 텍스트들")]
    [SerializeField] public Button startButton;
    [SerializeField] public TextMeshProUGUI HighScoreText;
    [SerializeField] public TextMeshProUGUI AllCoinText;
    [SerializeField] public Button LeaderBoardButton;
    [SerializeField] public Button AchievementsButton;
    [SerializeField] public Image LoadingImage;
    [SerializeField] public TextMeshProUGUI LoadingText;

    [Header("Revive Panel Ui"), Tooltip("부활 패널 UI들")]
    [SerializeField] public Button ReviveCheckButton;
    [SerializeField] public Button BackButton;
    [SerializeField] public TextMeshProUGUI ReviveCoinText;
    [SerializeField] public Button AdsReviveCheckButton;
 
    [Header("Quit Panel Ui"), Tooltip("종료 패널 UI들")]
    [SerializeField] public Button QuitCheckButton;
    [SerializeField] public Button QuitBackButton;

    [Header("Text Panel"), Tooltip("텍스트 패널 UI들")]
    [SerializeField] public TextMeshProUGUI FeverTextPanel;
    [SerializeField] public GameObject FeverTextObject;

    private Tween feverTextTween;
    private Tween loadingTween;

    public void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        tutorialManager = GameObject.FindGameObjectWithTag("Tutorial").GetComponent<TutorialManager>();
        soundManager = GameObject.FindGameObjectWithTag("Sound").GetComponent<SoundManager>();
        startButton.interactable = false; 
    }

    public void Start()
    {
        homeButton.onClick.AddListener(OnHomeButtonClick);
        resumeButton.onClick.AddListener(OnResumeButtonClick);
        quitButton.onClick.AddListener(OnQuitButtonClick);
        startButton.onClick.AddListener(OnStartButtonClick);
        optionButton.onClick.AddListener(ShowPausePanel);
        ReviveButton.onClick.AddListener(Revive);
        LobbyButton.onClick.AddListener(() => gameManager.AdsLoadHomeScene());
        BackButton.onClick.AddListener(HideRevivePanel);
        ReviveCheckButton.onClick.AddListener(OnReviveButtonClick);
        QuitCheckButton.onClick.AddListener(OnQuitButtonClick);
        QuitBackButton.onClick.AddListener(() => QuitPanel.SetActive(false));
        LeaderBoardButton.onClick.AddListener(ShowLeaderBoard);
        AchievementsButton.onClick.AddListener(ShowAchievements);
        AdsReviveCheckButton.onClick.AddListener(() => gameManager.OnReviveButtonClicked());
        
        UpdateAllCoinText(gameManager.TotalCoins);
        UpdateHighScoreText(gameManager.HighScore);
        UpdateReviveCoinText(gameManager.TotalCoins);

        FeverGauge.fillAmount = 0;
        FeverTextObject.SetActive(false);

        RevivePanel.SetActive(false);
        PausePanel.SetActive(false);
        GameOverPanel.SetActive(false);
        GamePanel.SetActive(false);
        QuitPanel.SetActive(false);
        TextPanel.SetActive(false);
    }

    private void OnStartButtonClick()
    {
#if UNITY_EDITOR
        Debug.Log("Start");
#endif
        startButton.interactable = false; 
        if (gameManager.isTutorialActive)
        {
            tutorialManager.StartTutorial(StartGame);
        }
        else
        {
            StartGame();
        }
    }

    public void ShowPausePanel()
    {
        PausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ShowQuitPanel()
    {
        QuitPanel.SetActive(true);
    }

    public void ShowGameOverPanel()
    {
        if (!GameOverPanel) return;

        GameOverPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void OnHomeButtonClick()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }
    
    public void OnResumeButtonClick()
    {
        Time.timeScale = 1;
        PausePanel.SetActive(false);
        gameManager.isPlaying = true;
        Debug.Log("Resume");
    }

    public void OnQuitButtonClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Quit");
#endif
    }

    private void StartGame()
    {
        gameManager.isPlaying = true;
        gameManager.MenuCamera.enabled = false;
        gameManager.InGameCamera.enabled = true;
        GamePanel.SetActive(true);
        GameMenuPanel.SetActive(false);
        GC.Collect();
        soundManager.PlayBgm(1);
    }

    public void UpdateCoinText(int coin)
    {
        coinText.text = coin.ToString("");
    }

    public void UpdateResultCoinText(int coin)
    {
        resultCoinText.text = "COIN:" + coin.ToString("");
    }

    public void UpdateAllCoinText(int coin)
    {
        AllCoinText.text = coin.ToString();
    }

    private void Revive()
    {
        RevivePanel.SetActive(true);
        UpdateReviveButtonState(gameManager.TotalCoins);
        UpdateReviveCoinText(gameManager.TotalCoins);
    }

    public void HideRevivePanel()
    {
        RevivePanel.SetActive(false);
    }

    public void UpdateScoreText(int currentScore)
    {
        GameScoreText.text = currentScore.ToString("");

        if (currentScore > gameManager.HighScore)
        {
            HighGameScoreText.text = currentScore.ToString("");
        }
    }

    public void UpdateResultScoreText(int currentScore)
    {
        scoreText.text = "SCORE: " + currentScore.ToString("");
    }

    public void UpdateHighScoreText(int highScore)
    {
        HighScoreText.text = highScore.ToString("");
        HighGameScoreText.text = highScore.ToString("");
    }

    private void UpdateReviveCoinText(int coin)
    {
        ReviveCoinText.text = coin.ToString();
    }

    private void UpdateReviveButtonState(int coin)
    {
        ReviveCheckButton.interactable = coin >= 300;
    }

    private void OnReviveButtonClick()
    {
        gameManager.RevivePlayer();
    }

    public void UpdateFeverGauge(float value)
    {
        FeverGauge.fillAmount = value;

        if (FeverGauge.fillAmount >= 1)
        {
            gameManager.ActivateFeverMode(10f);
            ShowFeverText();
        }
    }

    public void ResetFeverGuage()
    {
        FeverGauge.fillAmount = 0;
    }

    public void ShowFeverText()
    {
        TextPanel.SetActive(true);
        FeverTextObject.SetActive(true);
        FeverTextPanel.text = "FEVER TIME!!!";

        feverTextTween = FeverTextPanel.rectTransform.DOLocalMoveY(20, 0.5f)
            .SetRelative()
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void HideFeverText()
    {
        TextPanel.SetActive(false);
        FeverTextObject.SetActive(false);
        feverTextTween.Kill();
    }

    public void HidePausePanel()
    {
        PausePanel.SetActive(false);
        Time.timeScale = 1; 
        
    }

    public void HideQuitPanel()
    {
        QuitPanel.SetActive(false);
    }

    public void EnableStartButton()
    {
        startButton.interactable = true; 
    }
    
    public void ShowLoadingImage()
    {
        if (LoadingImage != null)
        {
            LoadingImage.gameObject.SetActive(true);
            LoadingText.gameObject.SetActive(true);
            StartLoadingAnimation();
        }
    }

    private void StartLoadingAnimation()
    {
        loadingTween = LoadingImage.DOFillAmount(1, 1f)
            .SetLoops(-1, LoopType.Restart)
            .OnComplete(() =>
            {
                LoadingImage.gameObject.SetActive(false);
                LoadingText.gameObject.SetActive(false);
            });
    }

    public void HideLoadingImage()
    {
        if (loadingTween != null)
        {
            loadingTween.Kill();
        }
        if (LoadingImage != null)
        {
            LoadingImage.fillAmount = 0;
            LoadingImage.gameObject.SetActive(false);
            LoadingText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (loadingTween != null)
        {
            loadingTween.Kill();
        }
    }

    private void ShowAchievements()
    {
        gameManager.ShowAchievements();
    }

    private void ShowLeaderBoard()
    {
        gameManager.ShowLeaderBoard();
    }
    
    
}
