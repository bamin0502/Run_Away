using UnityEngine;

public class MainManager : MonoBehaviour
{
    public Transform playerTransform;
    public Transform tileManagerTransform;

    private GameManager gameManager;
    private TileManager tileManager;
    private SpeedManager speedManager;
    private ItemManager itemManager;

    void Awake()
    {
        gameManager = GetComponent<GameManager>();

        tileManager = gameObject.AddComponent<TileManager>();
        speedManager = gameObject.AddComponent<SpeedManager>();
        itemManager = gameObject.AddComponent<ItemManager>();

        tileManager.playerTransform = playerTransform;
        tileManager.tileManagerTransform = tileManagerTransform;
    }

    private void Start()
    {
        tileManager.InitializeTiles();
    }

    private void Update()
    {
        if (!gameManager.isGameover && !gameManager.isPaused && gameManager.isPlaying)
        {
            float moveSpeed = speedManager.UpdateSpeed(Time.deltaTime);
            tileManager.MoveTiles(moveSpeed);

            float distanceTravelled = moveSpeed * Time.deltaTime;
            itemManager.UpdateDistance(distanceTravelled);
            gameManager.stageSpeed = moveSpeed;
        }
    }
}