using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerAni playerAni;

    public float jumpForce = 3f;
    public float slideForce = -10f;
    [SerializeField] private float[] lanes = new float[] { -3.8f, 0, 3.8f };
    [SerializeField] private int currentLaneIndex = 1;

    public Defines.SwipeDirection swipeDirection;

    [SerializeField] private float slideDuration = 1.0f;
    private float slideTimer;

    [SerializeField] private float laneChangeSpeed = 5f;
    private Vector3 targetPosition;
    private Vector3 lastPosition;
    private int lastLaneIndex;

    private bool isJumping;
    private bool isSliding;
    private bool isCollidingFront;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerAni = GetComponent<PlayerAni>();
    }

    private void Start()
    {
        swipeDirection = Defines.SwipeDirection.RUN;
        var position = rb.position;
        targetPosition = position;
        lastPosition = position;
        lastLaneIndex = currentLaneIndex;
        playerAni.SetRunAnimation();
        isCollidingFront = false;
    }

    private void Update()
    {
        if (swipeDirection == Defines.SwipeDirection.SLIDE)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                swipeDirection = Defines.SwipeDirection.RUN;
                isSliding = false;
                playerAni.SetRunAnimation();
            }
        }
        

        Vector3 newPosition = Vector3.Lerp(rb.position, targetPosition, laneChangeSpeed * Time.deltaTime);
        newPosition.y = rb.position.y;
        rb.MovePosition(newPosition);

        if (pendingMovement != Vector2.zero)
        {
            UpdateMovement(pendingMovement);
            pendingMovement = Vector2.zero;
        }

    }

    private void FixedUpdate()
    {
        if (pendingMovement != Vector2.zero)
        {
            UpdateMovement(pendingMovement);
            pendingMovement = Vector2.zero;
        }

        if (isJumping)
        {
            var mass = rb.mass;
            rb.AddForce(Physics.gravity * (mass * mass), ForceMode.Force);
        }
    }

    private Vector2 pendingMovement;

    public void SetPendingMovement(Vector2 movement)
    {
        pendingMovement = movement;
    }

    private void UpdateMovement(Vector2 swipeDir)
    {
        if (GameManager.Instance.isGameover || isCollidingFront) return;
        var horizontal = swipeDir.x;
        var vertical = swipeDir.y;

        if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
        {
            if (horizontal > 0.7f)
            {
                TryMoveToLane(currentLaneIndex + 1);
            }
            else if (horizontal < -0.7f)
            {
                TryMoveToLane(currentLaneIndex - 1);
            }
        }
        else
        {
            if (vertical > 0.7f && !isJumping) // 점프 중 다시 점프 불가
            {
                PerformJump();
            }
            else if (vertical < -0.7f)
            {
                PerformSlide();
            }
        }
    }

    private void PerformJump()
    {
        var velocity = rb.velocity;
        velocity = new Vector3(velocity.x, 0, velocity.z);
        rb.velocity = velocity;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        swipeDirection = Defines.SwipeDirection.JUMP;
        isJumping = true;
        isSliding = false;
        playerAni.SetJumpAnimation();
    }

    private void PerformSlide()
    {
        if (isJumping)
        {
            var velocity = rb.velocity;
            velocity = new Vector3(velocity.x, 0, velocity.z);
            rb.velocity = velocity;
            rb.AddForce(Vector3.up * slideForce, ForceMode.Impulse);
            isJumping = false;
        }

        if (IsObstacleInPath(rb.position + rb.transform.forward * 1.0f)) // 슬라이드 전방 장애물 검사
        {
#if UNITY_EDITOR
            Debug.Log("Obstacle detected during slide"); 
#endif
            
            Die(); // 장애물이 있으면 슬라이드 중 죽음
            return;
        }

        swipeDirection = Defines.SwipeDirection.SLIDE;
        slideTimer = slideDuration;
        isSliding = true;
        playerAni.SetSlideAnimation();
    }

    private void TryMoveToLane(int newLaneIndex)
    {
        if (isCollidingFront) return; // 충돌 중일 때는 레인 변경을 막음

        lastPosition = rb.position;
        lastLaneIndex = currentLaneIndex;
        int clampedLaneIndex = Mathf.Clamp(newLaneIndex, 0, lanes.Length - 1);

        Vector3 potentialTargetPosition = rb.position;
        potentialTargetPosition.x = lanes[clampedLaneIndex];

        if (IsObstacleInPath(potentialTargetPosition))
        {
            targetPosition = lastPosition;
            currentLaneIndex = lastLaneIndex;
            return;
        }

        currentLaneIndex = clampedLaneIndex;
        swipeDirection = Defines.SwipeDirection.RUN;
        targetPosition = potentialTargetPosition;
    }

    private bool IsObstacleInPath(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        if (Physics.Raycast(transform.position, direction, out var hit, direction.magnitude))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                return true;
            }
        }
        return false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            isJumping = false;
            if (!isSliding) playerAni.SetRunAnimation();
        }

        if (other.collider.CompareTag("Obstacle"))
        {
            var collisionDirection = other.contacts[0].point - transform.position;
            var forward = transform.forward;
            forward.y = 0;
            var angle = Vector3.Angle(forward, collisionDirection);

            if (angle < 75) // 전면 충돌
            {
#if UNITY_EDITOR
                Debug.Log("Frontal Obstacle Hit");
#endif
                isCollidingFront = true;
                GameManager.Instance.GameOver();
                Die();
            }
            else // 측면 충돌
            {
#if UNITY_EDITOR
                Debug.Log("Side Obstacle Hit");
#endif
                
                targetPosition = lastPosition;
                rb.position = lastPosition;
                currentLaneIndex = lastLaneIndex;
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            isJumping = true;
        }
        if (other.collider.CompareTag("Obstacle"))
        {
            isCollidingFront = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            // 아이템 효과 발동 예정
            other.gameObject.SetActive(false);
        }
    }

    private void Die()
    {
        swipeDirection = Defines.SwipeDirection.DEAD;
        playerAni.SetDeathAnimation();
        // 추가로 죽음 처리 로직 필요시 여기에 추가
    }
}