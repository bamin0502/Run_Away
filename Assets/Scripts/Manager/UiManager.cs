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

    // [Header("Game Over Panel Ui Text")]
    // [SerializeField] public TextMeshProUGUI distanceText;

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
        FadeIn(GameOverPanel);
        Time.timeScale = 0;
    }

    private void FadeIn(GameObject obj)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup)
            canvasGroup.DOFade(1, 0.5f);
    }

    private void FadeOut(GameObject obj)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.DOFade(0, 0.5f);
    }

    public void OnHomeButtonClick()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
        
    }

    public void OnResumeButtonClick()
    {
        Time.timeScale = 1;
        FadeOut(PausePanel);
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
    
    public void UpdateCoinText(int i)
    {
        coinText.text = i.ToString();
    }

    public void StartGame()
    {
        gameManager.isPlaying = true;
        gameManager.MenuCamera.enabled = false;
        gameManager.InGameCamera.enabled = true;
        GamePanel.SetActive(true);
    }
    
    public void HighScoreTextUpdate(int i)
    {
        HighScoreText.text = i.ToString();
    }
    
    public void AllCoinTextUpdate(int i)
    {
        AllCoinText.text = i.ToString();
    }

    
}
