using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class UiManager : MonoBehaviour
{
    [Header("UI Elements")] 
    [SerializeField] public GameObject PausePanel;
    [SerializeField] public GameObject GameOverPanel;
    [SerializeField] public GameObject GamePanel;
    [SerializeField] public GameObject GameMenuPanel;
    
    [Header("Pause Panel Ui Button")]
    [SerializeField] public Button homeButton;
    [SerializeField] public Button resumeButton;
    [SerializeField] public Button quitButton;
    // [SerializeField] public ToggleGroup soundToggleGroup;
    // [SerializeField] public Toggle BgmToggle;
    // [SerializeField] public Toggle SfxToggle;
    // [SerializeField] public AudioMixer audioMixer;
    // public int BgmToggleIndex;
    // public int SfxToggleIndex;
    // private readonly string BgmParameter="BGM";
    // private readonly string SfxParameter="SFX";
    
    // [Header("Game Over Panel Ui Button")]
    // [SerializeField] private Button restartButton;
    // [SerializeField] private Button quitButton2;

    // [Header("Game Over Panel Ui Text")]
    // [SerializeField] public TextMeshProUGUI distanceText;

    [Header("Game UI")]
    [SerializeField] public TextMeshProUGUI coinText;
    [SerializeField] public Button optionButton;
    
    [Header("Game Panel Ui Text")]
    [SerializeField] public Button startButton;
    
    
    public void Awake()
    {
        //InitializeUI();
    }

    public void Start()
    {
        homeButton.onClick.AddListener(OnHomeButtonClick);
        resumeButton.onClick.AddListener(OnResumeButtonClick);
        quitButton.onClick.AddListener(OnQuitButtonClick);
        startButton.onClick.AddListener(StartGame);
        optionButton.onClick.AddListener(ShowPausePanel);
    }

    private void InitializeUI()
    {
        if (PausePanel != null)
        {
            PausePanel.SetActive(false);
        }
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(false);
        }

        if (homeButton != null)
        {
            homeButton.onClick.AddListener(OnHomeButtonClick);
        }
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeButtonClick);
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClick);
        }
        // if (restartButton != null)
        // {
        //     restartButton.onClick.AddListener(OnRestartButtonClick);
        // }
        // if (quitButton2 != null)
        // {
        //     quitButton2.onClick.AddListener(OnQuitButtonClick);
        // }
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

    // public void UpdateDistanceText(float distance)
    // {
    //     if (distanceText != null)
    //         distanceText.text = "Distance: " + distance.ToString("F2") + " meters";
    // }

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
        //진행하던걸 중지하고 다시 처음부터?
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
        
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

    private void OnRestartButtonClick()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UpdateCoinText(int i)
    {
        coinText.text = i.ToString();
    }

    private void StartGame()
    {
        GameMenuPanel.SetActive(false);
        GameManager.Instance.isPlaying = true;
        GameManager.Instance.MenuCamera.enabled = false;
        GameManager.Instance.InGameCamera.enabled = true;
        GamePanel.SetActive(true);
    }
    
}
