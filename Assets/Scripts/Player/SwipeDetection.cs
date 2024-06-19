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
        inputManager = GetComponent<InputManager>();
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
        if(gameManager.isGameover || !gameManager.isPlaying) return;
        var swipeVector = endPos - startPos;
        var distance = Mathf.Clamp(swipeVector.magnitude, 0f, minSwipeDistancePixels);
        //var distance = swipeVector.magnitude;
        if (distance >= minDistance)
        {
            swipeVector.Normalize();
            playerMovement.SetPendingMovement(swipeVector);
        }
    }
}