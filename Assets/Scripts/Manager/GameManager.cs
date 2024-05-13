using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public float stageSpeed = 5f;
    public bool isGameover = false;
    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void GameOver()
    {
        Debug.Log("Game Over");
        isGameover = true;
    }
}
