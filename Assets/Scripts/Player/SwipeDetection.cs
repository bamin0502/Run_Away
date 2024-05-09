using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwipeDetection : MonoBehaviour
{
    [SerializeField] private float minSwipeDistance = 0.2f;
    [SerializeField, Range(0f, 1f)] private float directionThreshold = 0.7f;

    private InputManager inputManager;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private float startTime;
    private float endTime;

    private Rigidbody rb;
    private Vector3 moveAmount;
    
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
        float distance = Vector2.Distance(startPosition, endPosition);
        if (distance >= minSwipeDistance && (endTime - startTime) < 0.5f)
        {
            Vector2 direction = (endPosition - startPosition).normalized;
            UpdateMovement(direction);
        }
    }

    private void UpdateMovement(Vector2 direction)
    {
        float horizontalDot = Vector2.Dot(Vector2.right, direction);
        float verticalDot = Vector2.Dot(Vector2.up, direction);

        if (Mathf.Abs(horizontalDot) > directionThreshold)
        {
            moveAmount.x += (horizontalDot > 0 ? 1 : -1) * 5;  // 오른쪽 또는 왼쪽으로 5단위 이동
        }
        if (Mathf.Abs(verticalDot) > directionThreshold)
        {
            moveAmount.y += (verticalDot > 0 ? 1 : -1) * 0.5f;  // 위 또는 아래로 0.5단위 이동
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveAmount); 
        moveAmount = Vector3.zero; 
    }
}
