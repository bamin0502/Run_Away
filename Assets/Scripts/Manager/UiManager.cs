using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    [Header("UI Elements")] 
    [SerializeField] public GameObject PausePanel;
    [SerializeField] public GameObject GameOverPanel;
    
    [Header("Pause Panel Ui Button")]
    [SerializeField] public Button homeButton;
    [SerializeField] public Button resumeButton;
    [SerializeField] public Button quitButton;
    
    // [Header("Game Over Panel Ui Button")]
    // [SerializeField] private Button restartButton;
    // [SerializeField] private Button quitButton2;

    // [Header("Game Over Panel Ui Text")]
    // [SerializeField] public TextMeshProUGUI distanceText;

    [Header("Game UI")]
    [SerializeField] public TextMeshProUGUI coinText;
    
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeUI();
    }

    public void Awake()
    {
        //InitializeUI();
        
        PausePanel = GameObject.FindGameObjectWithTag("Pause").GetComponent<GameObject>();
        homeButton = GameObject.FindGameObjectWithTag("HomeButton").GetComponent<Button>();
        resumeButton = GameObject.FindGameObjectWithTag("ResumeButton").GetComponent<Button>();
        quitButton = GameObject.FindGameObjectWithTag("QuitButton").GetComponent<Button>();
    }

    public void Start()
    {
        homeButton.onClick.AddListener(OnHomeButtonClick);
        resumeButton.onClick.AddListener(OnResumeButtonClick);
        quitButton.onClick.AddListener(OnQuitButtonClick);
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
        if (!PausePanel) return;

        PausePanel.SetActive(true);
        FadeIn(PausePanel);
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
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
        Debug.Log("Home");
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
        Debug.Log("Quit");
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
    
    
}
