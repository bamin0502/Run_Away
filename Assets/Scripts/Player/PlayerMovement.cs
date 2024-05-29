using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement Fields")] internal Rigidbody rb;
    private PlayerAni playerAni;
    private BoxCollider boxCollider;
    private GameManager gameManager;

    [Header("Player Movement Parameters")]
    public float jumpForce = 10f;
    public float slideForce = -10f;
    
    [SerializeField] private float[] lanes = new float[] { -3.8f, 0, 3.8f };
    [SerializeField] internal int currentLaneIndex = 1;

    [Header("Player Movement States")]
    public Defines.SwipeDirection swipeDirection;

    [SerializeField] private float slideDuration = 1.0f;
    private float slideTimer;

    [SerializeField] private float laneChangeSpeed = 5f;
    public Vector3 targetPosition { get; internal set; }
    public Vector3 lastPosition { get; private set; }
    public int lastLaneIndex { get; private set; }

    public bool isJumping { get; internal set; }
    public bool isSliding { get; private set; }
    public bool isCollidingFront { get; internal set; }

    private Vector3 originalColliderCenter;
    private Vector3 originalColliderSize;
    
    public bool isInvincible { get; private set; }
    private float maxJumpPower = 15f;
    private Vector2 pendingMovement;
    
    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        playerAni = GetComponent<PlayerAni>();
        boxCollider = GetComponent<BoxCollider>();
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        originalColliderCenter = boxCollider.center;
        originalColliderSize = boxCollider.size;
    }

    private void Start()
    {
        swipeDirection = Defines.SwipeDirection.IDLE;
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

                boxCollider.center = originalColliderCenter;
                boxCollider.size = originalColliderSize;
            }
        }

        Vector3 newPosition = Vector3.Lerp(rb.position, targetPosition, laneChangeSpeed * Time.deltaTime);
        newPosition.y = rb.position.y;
        rb.MovePosition(newPosition);
    }

    private void FixedUpdate()
    {
        if (pendingMovement != Vector2.zero)
        {
            UpdateMovement(pendingMovement);
        }

        if (isJumping)
        {
            rb.AddForce(Physics.gravity * rb.mass, ForceMode.Force);
        }
    }
    
    public void SetPendingMovement(Vector2 movement)
    {
        pendingMovement = movement;
    }

    private void UpdateMovement(Vector2 swipeDir)
    {
        if (gameManager.isGameover || isCollidingFront) return;
        var horizontal = swipeDir.x;
        var vertical = swipeDir.y;

        if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
        {
            if (horizontal > 0.5f)
            {
                TryMoveToLane(currentLaneIndex + 1);
                pendingMovement = Vector2.zero;
            }
            else if (horizontal < -0.5f)
            {
                TryMoveToLane(currentLaneIndex - 1);
                pendingMovement = Vector2.zero;
            }
        }
        else
        {
            if (vertical > 0.5f)
            {
                PerformJump();
                //SoundManager.instance.PlaySfx(6);
            }
            else if (vertical < -0.5f)
            {
                PerformSlide();
                pendingMovement = Vector2.zero;
                //SoundManager.instance.PlaySfx(9);
            }
        }
    }

    private void PerformJump()
    {
        if (isJumping) return;

        var velocity = rb.velocity;
        velocity = new Vector3(velocity.x, 0, velocity.z);
        rb.velocity = velocity;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        pendingMovement = Vector2.zero;
        swipeDirection = Defines.SwipeDirection.JUMP;
        isJumping = true;
        isSliding = false;
        playerAni.SetJumpAnimation();

        boxCollider.center = originalColliderCenter;
        boxCollider.size = originalColliderSize;
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

        swipeDirection = Defines.SwipeDirection.SLIDE;
        slideTimer = slideDuration;
        isSliding = true;
        playerAni.SetSlideAnimation();
        boxCollider.center = new Vector3(0, 0.45f, 0.15f);
        boxCollider.size = new Vector3(0.6f, 0.6f, 0.75f);
    }

    public void TryMoveToLane(int newLaneIndex)
    {
        if (isCollidingFront) return;

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
        
        if (isSliding)
        {
            isSliding = false;
            playerAni.SetRunAnimation();
            boxCollider.center = originalColliderCenter;
            boxCollider.size = originalColliderSize;
        }
    }

    public bool IsObstacleInPath(Vector3 targetPos)
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

    public void Die()
    {
        swipeDirection = Defines.SwipeDirection.DEAD;
        playerAni.SetDeathAnimation();
        SoundManager.instance.PlaySfx(1);
        // 추가로 죽음 처리 로직 필요시 여기에 추가
        //deadParticle.Play();
    }
    
    public void AdjustJumpPower(float amount)
    {
        jumpForce += amount;
    }

    public void ResetJumpPower(float originalJumpForce)
    {
        jumpForce = originalJumpForce;
    }
    
    public void Revive()
    {
        Vector3 safePosition = GetSafeRevivePosition(rb.position);
        rb.position = safePosition;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isJumping = false;
        isSliding = false;
        isCollidingFront = false;
        swipeDirection = Defines.SwipeDirection.RUN;
        playerAni.SetRunAnimation();

        SetInvincible(true);
        StartCoroutine(RemoveInvincibilityAfter(2.0f)); 
    }
    
    private Vector3 GetSafeRevivePosition(Vector3 startPosition)
    {
        Vector3 revivePosition = startPosition;
        for (int i = 0; i < 10; i++)
        {
            if (IsObstacleInPath(revivePosition))
            {
                return revivePosition;
            }

            revivePosition.y += 0.5f;
        }

        return startPosition;
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
        if (invincible)
        {
            playerAni.SetInvincibleAnimation();
        }
        else
        {
            playerAni.StopInvincibleAnimation();
        }
    }

    private IEnumerator RemoveInvincibilityAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetInvincible(false);
    }
}
