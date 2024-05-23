using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    private GameManager gameManager;
    private TutorialManager tutorialManager;
    
    [Header("UI Elements")] 
    [SerializeField] public GameObject PausePanel;
    [SerializeField] public GameObject GameOverPanel;
    [SerializeField] public GameObject GamePanel;
    [SerializeField] public GameObject GameMenuPanel;
    
    [Header("Pause Panel Ui Button")]
    [SerializeField] public Button homeButton;
    [SerializeField] public Button resumeButton;
    [SerializeField] public Button quitButton;
    
    [Header("Game Over Panel Ui Button")]
    [SerializeField] private Button ReviveButton;
    [SerializeField] private Button LobbyButton;

    [Header("Game Over Panel Ui Text")]
    [SerializeField] public TextMeshProUGUI scoreText;
    [SerializeField] public TextMeshProUGUI resultCoinText;

    [Header("Game UI")]
    [SerializeField] public TextMeshProUGUI coinText;
    [SerializeField] public Button optionButton;
    
    [Header("Game Panel Ui Text")]
    [SerializeField] public Button startButton;
    [SerializeField] public TextMeshProUGUI HighScoreText;
    [SerializeField] public TextMeshProUGUI AllCoinText;
    
    public void Awake()
    {
        //InitializeUI();
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
        
        UpdateAllCoinText(gameManager.TotalCoins);
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
        Time.timeScale = 0;
    }

    public void ShowGameOverPanel()
    {
        if (GameOverPanel == null) return;

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

    public void StartGame()
    {
        gameManager.isPlaying = true;
        gameManager.MenuCamera.enabled = false;
        gameManager.InGameCamera.enabled = true;
        GamePanel.SetActive(true);
    }
    
    public void UpdateCoinText(int coin)
    {
        coinText.text = coin.ToString();
    }

    public void UpdateResultCoinText(int coin)
    {
        resultCoinText.text = "COIN:" + coin.ToString("00000");
    }

    public void UpdateAllCoinText(int coin)
    {
        AllCoinText.text = coin.ToString();
    }
    
    public void Revive()
    {
        return;
    }
}
