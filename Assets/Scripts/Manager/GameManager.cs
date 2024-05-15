using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public float stageSpeed = 5f;
    public bool isGameover;
    public bool isPaused;
    public bool isPlaying=false;
    public float distanceTravelled = 0;
    public float speedIncreaseMilestone = 10;
    public float speedMultiplier = 1.1f;
    
    public TextMeshProUGUI distanceText;
    void Start()
    {
        
    }

    private void Update()
    {
        if (!isGameover || !isPaused || isPlaying)
        {
            distanceTravelled += stageSpeed * Time.deltaTime;

            if (distanceTravelled > speedIncreaseMilestone)

            {
                stageSpeed *= speedMultiplier;
                speedIncreaseMilestone *= speedMultiplier;
            }
            distanceText.text = "Distance: " + distanceTravelled.ToString("F2") + " meters";
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UiManager.Instance.ShowPausePanel();
        }
        

    }

    public void GameOver()
    {
        Debug.Log("Game Over");
        isGameover = true;
        Handheld.Vibrate();
    }
    
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
