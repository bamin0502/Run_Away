using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class SwipeDetection : MonoBehaviour
{
    private InputManager inputManager;
    private PlayerMovement playerMovement;
    private GameManager gameManager;
    [SerializeField] private float minDistance = 0.1f;
    private Vector2 startPos;
    private Vector2 endPos;
    private float minSwipeDistancePixels;
    private float minSwipeDistanceInch = 0.1f;

    private void Awake()
    {
        inputManager = InputManager.Instance;
        playerMovement = GetComponent<PlayerMovement>();
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
    }

    private void Start()
    {
        minSwipeDistancePixels = minSwipeDistanceInch * Screen.dpi;
    }

    private void OnEnable()
    {
        inputManager.OnStartTouch += SwipeStart;
        inputManager.OnEndTouch += SwipeEnd;
    }

    private void OnDisable()
    {
        inputManager.OnStartTouch -= SwipeStart;
        inputManager.OnEndTouch -= SwipeEnd;
    }

    private void SwipeStart(Vector2 pos, float time)
    {
#if UNITY_EDITOR
        Debug.Log("Start");
#endif
        
        startPos = pos;
    }

    private void SwipeEnd(Vector2 pos, float time)
    {
        endPos = pos;
#if UNITY_EDITOR
        Debug.Log("End");
#endif
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        if (gameManager.isGameover || !gameManager.isPlaying || gameManager.isPaused) return;
        
        Vector2 swipeVector = endPos - startPos;
        float distance = swipeVector.magnitude;

        if (distance >= minSwipeDistancePixels)
        {
            playerMovement.SetPendingMovement(swipeVector.normalized);
        }
    }
}