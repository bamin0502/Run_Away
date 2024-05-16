using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerAni playerAni;

    [Header("Swipe Movement")]
    public float jumpForce = 3f;
    public float slideForce = -10f; // 빠르게 하강하는 힘
    [SerializeField] private float[] lanes = new float[] { -3.8f, 0, 3.8f };
    [SerializeField] private int currentLaneIndex = 1;

    public Defines.SwipeDirection swipeDirection;

    [SerializeField] private float slideDuration = 1.0f;
    private float slideTimer;

    [Header("Smooth Movement")]
    [SerializeField] private float laneChangeSpeed = 5f;
    private Vector3 targetPosition;

    private bool isJumping;
    private bool isSliding;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerAni = GetComponent<PlayerAni>();
    }

    private void Start()
    {
        swipeDirection = Defines.SwipeDirection.RUN;
        targetPosition = rb.position;
        playerAni.SetRunAnimation();
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

        // Smoothly move to target position
        Vector3 newPosition = Vector3.Lerp(rb.position, targetPosition, laneChangeSpeed * Time.deltaTime);
        newPosition.y = rb.position.y;  // Preserve the Y position during horizontal movement
        rb.MovePosition(newPosition);
    }

    private void FixedUpdate()
    {
        if (pendingMovement != Vector2.zero)
        {
            UpdateMovement(pendingMovement);
            pendingMovement = Vector2.zero;
        }

        // Apply gravity manually if jumping
        if (isJumping)
        {
            rb.AddForce(Physics.gravity * (rb.mass * rb.mass), ForceMode.Force);
        }
    }

    private Vector2 pendingMovement;

    public void SetPendingMovement(Vector2 movement)
    {
        pendingMovement = movement;
    }

    private void UpdateMovement(Vector2 swipeDir)
    {
        if (GameManager.Instance.isGameover) return;
        var horizontal = swipeDir.x;
        var vertical = swipeDir.y;

        if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
        {
            if (horizontal > 0.7f)
            {
                currentLaneIndex = Mathf.Clamp(currentLaneIndex + 1, 0, lanes.Length - 1);
                swipeDirection = Defines.SwipeDirection.RUN;
            }
            else if (horizontal < -0.7f)
            {
                currentLaneIndex = Mathf.Clamp(currentLaneIndex - 1, 0, lanes.Length - 1);
                swipeDirection = Defines.SwipeDirection.RUN;
            }

            // Set target position for horizontal movement only
            targetPosition = rb.position;
            targetPosition.x = lanes[currentLaneIndex];
        }
        else
        {
            if (vertical > 0.7f && !isJumping && !isSliding)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);  // Reset vertical velocity
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                swipeDirection = Defines.SwipeDirection.JUMP;
                isJumping = true;
                playerAni.SetJumpAnimation();
            }
            else if (vertical < -0.7f)
            {
                if (isJumping)
                {
                    // Apply downward force to quickly land
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);  // Reset vertical velocity
                    rb.AddForce(Vector3.up * slideForce, ForceMode.Impulse);
                    isJumping = false;
                }
                swipeDirection = Defines.SwipeDirection.SLIDE;
                slideTimer = slideDuration;
                isSliding = true;
                playerAni.SetSlideAnimation();
            }
        }
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
            Debug.Log("Obstacle Hit");
            GameManager.Instance.GameOver();
            Die();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            isJumping = true;
        }
    }

    private void Die()
    {
        swipeDirection = Defines.SwipeDirection.DEAD;
        playerAni.SetDeathAnimation();
    }
}
