using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class UiManager : Singleton<UiManager>
{
    [Header("UI Elements")] 
    public GameObject PausePanel;
    public GameObject GameOverPanel;
    //public GameObject QuitPanel;
    //public GameObject HomePanel;
    
    [Header("Pause Panel Ui Button")]
    public Button homeButton;
    public Button resumeButton;
    public Button quitButton;
    
    [Header("Game Over Panel Ui Button")]
    public Button restartButton;
    public Button quitButton2;

    [Header("Game Over Panel Ui Text")]
    public TextMeshProUGUI distanceText;
    //public TextMeshProUGUI coinText;
    //public TextMeshProUGUI bestDistanceText;
    
    //[Header("Quit Panel Ui Button")]
    //public Button yesButton;
    //public Button noButton;
    
    //[Header("Home Panel Ui Button")]
    //public Button StartButton;
    
    public override void Awake()
    {
        PausePanel.SetActive(false);
        GameOverPanel.SetActive(false);
        //QuitPanel.SetActive(false);
        //HomePanel.SetActive(false);
    }
    
    public void ShowPausePanel()
    {
        PausePanel.SetActive(true);
        FadeIn(PausePanel);
        Time.timeScale = 0;
    }
    
    public void ShowGameOverPanel()
    {
        GameOverPanel.SetActive(true);
        FadeIn(GameOverPanel);
        Time.timeScale = 0;
    }
    
    public void UpdateDistanceText(float distance)
    {
        distanceText.text = "Distance: " + distance.ToString("F2") + " meters";
    }
    
    public void UpdateCoinText(int coin)
    {
        //coinText.text = "Coin: " + coin;
    }
    
    public void UpdateBestDistanceText(float bestDistance)
    {
        //bestDistanceText.text = "Best Distance: " + bestDistance.ToString("F2") + " meters";
    }

    private void FadeIn(GameObject obj)
    {
        obj.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
    }
    
    private void FadeOut(GameObject obj)
    {
        obj.GetComponent<CanvasGroup>().DOFade(0, 0.5f);
    }
    
    public void OnHomeButtonClick()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
    
    public void OnResumeButtonClick()
    {
        Time.timeScale = 1;
        FadeOut(PausePanel);
        PausePanel.SetActive(false);
    }
    
    public void OnQuitButtonClick()
    {
        Application.Quit();
    }
    public void OnRestartButtonClick()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
    
    public void OnStartButtonClick()
    {
        Time.timeScale = 1;
    }
    
}
