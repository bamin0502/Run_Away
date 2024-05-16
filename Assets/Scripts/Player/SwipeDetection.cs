using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class SwipeDetection : MonoBehaviour
{
    private InputManager inputManager;
    private PlayerMovement playerMovement;

    [SerializeField] private float minDistance = 0.2f;
    private Vector2 startPos;
    private Vector2 endPos;
    private float minSwipeDistancePixels;
    private float minSwipeDistanceInch = 0.25f;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
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
        Debug.Log("Start");
        startPos = pos;
    }

    private void SwipeEnd(Vector2 pos, float time)
    {
        endPos = pos;
        Debug.Log("End");
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        var swipeVector = endPos - startPos;
        var distance = Mathf.Clamp(swipeVector.magnitude, 0f, minSwipeDistancePixels);
        if (distance >= minDistance)
        {
            swipeVector.Normalize();
            playerMovement.SetPendingMovement(swipeVector);
        }
    }
}