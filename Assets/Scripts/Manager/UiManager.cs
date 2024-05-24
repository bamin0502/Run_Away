using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    [Header("Game Manager"),Tooltip("게임 매니저 오브젝트")]
    private GameManager gameManager;
    [Header("Tutorial Manager"),Tooltip("튜토리얼 매니저 오브젝트")]
    private TutorialManager tutorialManager;
    
    [Header("UI Elements"),Tooltip("패널 관련 오브젝트들")] 
    [SerializeField] public GameObject PausePanel;
    [SerializeField] public GameObject GameOverPanel;
    [SerializeField] public GameObject GamePanel;
    [SerializeField] public GameObject GameMenuPanel;
    [SerializeField] public GameObject RevivePanel;
    
    [Header("Pause Panel Ui Button"),Tooltip("일시정지 패널 버튼들")]
    [SerializeField] public Button homeButton;
    [SerializeField] public Button resumeButton;
    [SerializeField] public Button quitButton;
    
    [Header("Game Over Panel Ui Button"),Tooltip("게임오버 패널 버튼들")]
    [SerializeField] private Button ReviveButton;
    [SerializeField] private Button LobbyButton;

    [Header("Game Over Panel Ui Text"),Tooltip("게임오버 패널 텍스트들")]
    [SerializeField] public TextMeshProUGUI scoreText;
    [SerializeField] public TextMeshProUGUI resultCoinText;

    [Header("Game UI"),Tooltip("게임 패널 텍스트들")]
    [SerializeField] public TextMeshProUGUI coinText;
    [SerializeField] public Button optionButton;
    [SerializeField] public TextMeshProUGUI GameScoreText;
    [SerializeField] public TextMeshProUGUI HighGameScoreText;
    
    [Header("Game Panel Ui Text"),Tooltip("게임 패널 텍스트들")]
    [SerializeField] public Button startButton;
    [SerializeField] public TextMeshProUGUI HighScoreText;
    [SerializeField] public TextMeshProUGUI AllCoinText;
    
    [Header("Revive Panel Ui"),Tooltip("부활 패널 UI들")]
    [SerializeField] public Button ReviveCheckButton;
    [SerializeField] public Button BackButton;
    [SerializeField] public TextMeshProUGUI ReviveCoinText;
    public void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        tutorialManager=GameObject.FindGameObjectWithTag("Tutorial").GetComponent<TutorialManager>();
    }

    public void Start()
    {
        homeButton.onClick.AddListener(OnHomeButtonClick);
        resumeButton.onClick.AddListener(OnResumeButtonClick);
        quitButton.onClick.AddListener(OnQuitButtonClick);
        startButton.onClick.AddListener(OnStartButtonClick);
        optionButton.onClick.AddListener(ShowPausePanel);
        ReviveButton.onClick.AddListener(Revive);
        LobbyButton.onClick.AddListener(OnHomeButtonClick);
        BackButton.onClick.AddListener(HideRevivePanel);
        ReviveCheckButton.onClick.AddListener(OnReviveButtonClick);
        
        UpdateAllCoinText(gameManager.TotalCoins);
        UpdateHighScoreText(gameManager.HighScore);
        UpdateReviveCoinText(gameManager.TotalCoins);
        
        RevivePanel.SetActive(false);
        PausePanel.SetActive(false);
        GameOverPanel.SetActive(false);
        GamePanel.SetActive(false);
    }

    private void OnStartButtonClick()
    {
#if UNITY_EDITOR
        Debug.Log("Start");
#endif

        if (gameManager.isTutorialActive)
        {
            tutorialManager.StartTutorial(StartGame);
            GameMenuPanel.SetActive(false);
        }
        else
        {
            StartGame();
            GameMenuPanel.SetActive(false);
        }
           
    }

    public void ShowPausePanel()
    {
        PausePanel.SetActive(true);
        Time.timeScale = 0;
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
        SoundManager.Instance.PlayBgm(1);
    }
    
    public void UpdateCoinText(int coin)
    {
        coinText.text = coin.ToString("0000");
    }

    public void UpdateResultCoinText(int coin)
    {
        resultCoinText.text = "COIN:" + coin.ToString("0000");
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
        GameScoreText.text = currentScore.ToString("00000");
        
        if(currentScore > gameManager.HighScore)
        {
            HighGameScoreText.text = currentScore.ToString("00000");
        }
    }
    
    public void UpdateResultScoreText(int currentScore)
    {
        scoreText.text = "SCORE: " + currentScore.ToString("00000");
    }

    public void UpdateHighScoreText(int highScore)
    {
        HighScoreText.text = highScore.ToString("00000");
        HighGameScoreText.text = highScore.ToString("00000");
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
    
}
