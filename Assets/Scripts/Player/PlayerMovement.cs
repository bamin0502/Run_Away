using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.5f;
    public bool isGrounded;

    [Header("Swipe Movement")]
    public float jumpForce = 3f;
    [SerializeField] private float[] lanes = new float[] { -3.8f, 0, 3.8f };
    [SerializeField] private int currentLaneIndex = 1;

    public Defines.SwipeDirection swipeDirection;

    [SerializeField] private float slideDuration = 1.0f;
    private float slideTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        swipeDirection = Defines.SwipeDirection.RUN;
    }

    private void Update()
    {
        if (!groundCheck)
        {
            groundCheck = GameObject.FindGameObjectWithTag("Ground").transform;
        }

        if (swipeDirection == Defines.SwipeDirection.SLIDE)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                swipeDirection = Defines.SwipeDirection.RUN;
            }
        }
    }

    private void FixedUpdate()
    {
        if (pendingMovement != Vector2.zero)
        {
            UpdateMovement(pendingMovement);
            pendingMovement = Vector2.zero;
        }
    }

    private Vector2 pendingMovement;

    public void SetPendingMovement(Vector2 movement)
    {
        pendingMovement = movement;
    }

    public void UpdateMovement(Vector2 swipeDir)
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
            var newPos = rb.position;
            newPos.x = lanes[currentLaneIndex];
            rb.MovePosition(newPos);
        }
        else
        {
            if (vertical > 0.7f && isGrounded)
            {
                rb.MovePosition(rb.position + Vector3.up * jumpForce);
                swipeDirection = Defines.SwipeDirection.JUMP;
            }
            else if (vertical < -0.7f)
            {
                if (swipeDirection == Defines.SwipeDirection.JUMP)
                {
                    swipeDirection = Defines.SwipeDirection.SLIDE;
                    slideTimer = slideDuration;
                }
                else if (isGrounded)
                {
                    swipeDirection = Defines.SwipeDirection.SLIDE;
                    slideTimer = slideDuration;
                }
            }
            else if (isGrounded)
            {
                swipeDirection = Defines.SwipeDirection.RUN;
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!GameManager.Instance.isGameover && groundCheck != null && other.collider.CompareTag("Ground"))
        {
            isGrounded = true;
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
        if (groundCheck != null && other.collider.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private void Die()
    {
        swipeDirection = Defines.SwipeDirection.DEAD;
    }
}
