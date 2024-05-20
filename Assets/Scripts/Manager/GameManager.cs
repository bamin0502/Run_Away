using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public float stageSpeed = 5f;
    public bool isGameover;
    public bool isPaused;
    public bool isPlaying = false;
    public float distanceTravelled = 0;

    [SerializeField] public TextMeshProUGUI distanceText;

    public bool isTutorialActive = true;
    
    public bool isFeverMode = false;
    public int CoinCount = 0;
    

    public override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeGame();
    }

    private void Update()
    {
        if (!isGameover && !isPaused && isPlaying)
        {
            distanceTravelled += stageSpeed * Time.deltaTime;

            distanceText.text = "Speed: " + stageSpeed.ToString("F0") + "m/s\n" + "Distance: " + distanceTravelled.ToString("F0") + "m";
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UiManager.Instance.ShowPausePanel();
        }
    }

    public void GameOver()
    {
#if UNITY_EDITOR
        Debug.Log("Game Over");
#endif
        isGameover = true;
#if UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
    }

    private void InitializeGame()
    {
        isGameover = false;
        isPaused = false;
        isPlaying = true;
        distanceTravelled = 0;
        stageSpeed = 5f; // 초기 속도 설정
    }
    
    public void AddCoin()
    {
        // 코인 획득 시 처리
        UiManager.Instance.UpdateCoinText(++CoinCount); 
    }
}