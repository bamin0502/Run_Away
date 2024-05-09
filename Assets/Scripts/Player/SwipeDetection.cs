using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwipeDetection : MonoBehaviour
{
    [Tooltip("최소 스와이프 거리"), Header("Swipe Settings")]
    [SerializeField] private float minSwipeDistance = 0.2f;  // 최소 스와이프 거리 설정
    [SerializeField, Range(0f, 1f)] private float directionThreshold = 0.7f;
    
    private InputManager inputManager;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private float startTime;
    private float endTime;
    
    private Rigidbody rb;
    private Vector3 pendingMove;
    private bool shouldMove;
    
    private void Awake()
    {
        inputManager = InputManager.Instance;
        rb = GetComponent<Rigidbody>();
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
    
    private void SwipeStart(Vector2 position, float time)
    {
        startPosition = position;
        startTime = time;
    }
    private void SwipeEnd(Vector2 position, float time)
    {
        endPosition = position;
        endTime = time;
        
        DetectSwipe();
    }
    
    private void DetectSwipe()
    {
        if (Vector3.Distance(startPosition, endPosition) >= minSwipeDistance && (endTime - startTime) < 0.5f)
        {
            Vector2 direction = endPosition - startPosition;
            Vector2 normalizedDirection = direction.normalized;
            
            if (Vector2.Dot(Vector2.up, normalizedDirection) > directionThreshold)
            {
                pendingMove = Vector3.up;
                Debug.Log("Up");
                shouldMove = true;
            }
            else if (Vector2.Dot(Vector2.down, normalizedDirection) > directionThreshold)
            {
                pendingMove = Vector3.down;
                Debug.Log("Down");
                shouldMove = true;
            }
            else if (Vector2.Dot(Vector2.right, normalizedDirection) > directionThreshold)
            {
                pendingMove = Vector3.right;
                Debug.Log("Right");
                shouldMove = true;
            }
            else if (Vector2.Dot(Vector2.left, normalizedDirection) > directionThreshold)
            {
                pendingMove = Vector3.left;
                Debug.Log("Left");
                shouldMove = true;
            }
        }
    }
    
    private void FixedUpdate()
    {
        if (shouldMove)
        {
            rb.MovePosition(rb.position + pendingMove);
            shouldMove = false;
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPosition, endPosition);
    }
    
    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 100, 100), $"Start: {startPosition}\nEnd: {endPosition}");
    }
    
    
}
