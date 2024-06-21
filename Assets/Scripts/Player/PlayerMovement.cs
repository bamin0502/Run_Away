using System;
using UniRx;
using UnityEngine;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement Fields")] 
    internal Rigidbody rb;
    private PlayerAni playerAni;
    private BoxCollider boxCollider;
    private GameManager gameManager;
    private SoundManager soundManager;

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
    private bool isDead;
    public bool isGrounded { get; internal set; }

    private Vector3 originalColliderCenter;
    private Vector3 originalColliderSize;

    public bool isInvincible { get; private set; }
    private float maxJumpPower = 15f;
    private Vector2 pendingMovement;

    public MMF_Player Player;

    private CompositeDisposable disposables = new CompositeDisposable();

    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        playerAni = GetComponent<PlayerAni>();
        boxCollider = GetComponent<BoxCollider>();
        soundManager = GameObject.FindGameObjectWithTag("Sound").GetComponent<SoundManager>();
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        originalColliderCenter = boxCollider.center;
        originalColliderSize = boxCollider.size;
    }

    private void OnDestroy()
    {
        disposables.Dispose();
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
        isDead = false;
        isGrounded = true;
    }

    private void Update()
    {
        HandleMovement();

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
        if (isJumping)
        {
            rb.AddForce(Physics.gravity * rb.mass, ForceMode.Force);
        }
    }

    private void LateUpdate()
    {
        if (rb.position == targetPosition)
        {
            swipeDirection = Defines.SwipeDirection.RUN;
        }
    }

    public void SetPendingMovement(Vector2 movement)
    {
        pendingMovement = movement;
    }

    private void HandleMovement()
    {
        if (pendingMovement == Vector2.zero) return;
        UpdateMovement(pendingMovement);
        pendingMovement = Vector2.zero;
    }

    private void UpdateMovement(Vector2 swipeDir)
    {
        if (gameManager.isGameover || isCollidingFront) return;
#if UNITY_EDITOR
        Debug.Log($"Swipe Direction: {swipeDir}"); // 스와이프 방향 로그
#endif
        // 민감도를 설정하여 작은 변화를 무시
        if (Mathf.Abs(swipeDir.x) > 0.1f || Mathf.Abs(swipeDir.y) > 0.1f)
        {
            // 주 방향 결정
            if (Mathf.Abs(swipeDir.x) > Mathf.Abs(swipeDir.y))
            {
                // 좌우 방향
                if (swipeDir.x > 0.1f)
                {
                    TryMoveToLane(currentLaneIndex + 1); // 오른쪽
                }
                else if (swipeDir.x < -0.1f)
                {
                    TryMoveToLane(currentLaneIndex - 1); // 왼쪽
                }
            }
            else
            {
                // 상하 방향
                if (swipeDir.y > 0.1f && isGrounded)
                {
                    PerformJump(); // 점프
                }
                else if (swipeDir.y < -0.1f)
                {
                    PerformSlide(); // 슬라이드
                }
            }
        }
    }

    private void PerformJump()
    {
        if (!isGrounded) return; // isGrounded 상태 체크
#if UNITY_EDITOR
        Debug.Log("Perform Jump"); // 점프 실행 로그
#endif
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        swipeDirection = Defines.SwipeDirection.JUMP;
        isJumping = true;
        isGrounded = false; // 점프 상태로 변경
        isSliding = false;
        playerAni.SetJumpAnimation();

        boxCollider.center = originalColliderCenter;
        boxCollider.size = originalColliderSize;
    }

    private void PerformSlide()
    {
        // 이미 슬라이드 중인지 확인
        if (isSliding) return;

        // 현재 점프 중인지 확인하고 전환 처리
        if (isJumping)
        {
            // 플레이어를 즉시 땅으로 이동
            rb.position = new Vector3(rb.position.x, 0, rb.position.z);
            rb.velocity = Vector3.zero; // 상승 움직임을 즉시 멈추기 위해 속도 초기화

            // WalkBy 판정 추가
            if (Physics.Raycast(rb.position, Vector3.down, out RaycastHit hit, 1.5f))
            {
                if (hit.collider.CompareTag("WalkBy") || hit.collider.CompareTag("Obstacle"))
                {
                    rb.position = new Vector3(rb.position.x, hit.point.y, rb.position.z);
                    isGrounded = true;
                    isJumping = false;
                }
                else
                {
                    isGrounded = true;
                    isJumping = false;
                }
            }
            else
            {
                isGrounded = true;
                isJumping = false;
            }
        }

        // 슬라이드 실행
#if UNITY_EDITOR
        Debug.Log("Perform Slide"); // 슬라이드 실행 로그
#endif
        swipeDirection = Defines.SwipeDirection.SLIDE;
        slideTimer = slideDuration;
        isSliding = true;
        playerAni.SetSlideAnimation();
        boxCollider.center = new Vector3(0, 0.45f, 0.15f);
        boxCollider.size = new Vector3(0.6f, 0.6f, 0.75f);
    }

    private void TryMoveToLane(int newLaneIndex)
    {
        if (isCollidingFront) return;
#if UNITY_EDITOR
        Debug.Log("Move to Lane: " + newLaneIndex); // 레인 이동 로그
#endif
        
        int clampedLaneIndex = Mathf.Clamp(newLaneIndex, 0, lanes.Length - 1);

        if (clampedLaneIndex == currentLaneIndex) return;

        Vector3 potentialTargetPosition = rb.position;
        potentialTargetPosition.x = lanes[clampedLaneIndex];

        if (IsObstacleInPath(potentialTargetPosition))
        {
            return;
        }

        lastPosition = rb.position;
        lastLaneIndex = currentLaneIndex;

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

    private bool IsObstacleInPath(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        if (Physics.Raycast(transform.position, direction, out var hit, direction.magnitude))
        {
            if (hit.collider.CompareTag("Obstacle") || hit.collider.CompareTag("Wall"))
            {
                // 장애물에 부딪히면 이동을 멈춤
                targetPosition = lastPosition;
                return true;
            }
        }
        return false;
    }

    public void Die()
    {
        if (isDead) return;

        swipeDirection = Defines.SwipeDirection.DEAD;
        playerAni.SetDeathAnimation();
        soundManager.PlaySfx(1);
        Player.PlayFeedbacks();
        isDead = true;
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

        isDead = false;

        SetInvincible(true);
        Observable.Timer(TimeSpan.FromSeconds(2.0f))
            .Subscribe(_ =>
            {
                SetInvincible(false);
                CheckAndMoveFromObstacle();
            })
            .AddTo(disposables);
    }

    private Vector3 GetSafeRevivePosition(Vector3 startPosition)
    {
        Vector3 revivePosition = startPosition;
        for (int i = 0; i < 10; i++)
        {
            if (!IsObstacleInPath(revivePosition))
            {
                return revivePosition;
            }

            revivePosition.y += 0.5f;
        }

        return startPosition;
    }

    private void CheckAndMoveFromObstacle()
    {
        if (IsObstacleInPath(rb.position))
        {
            Vector3 newPosition = GetSafePosition(rb.position);
            rb.position = newPosition;
            targetPosition = newPosition;
        }
    }

    private Vector3 GetSafePosition(Vector3 currentPosition)
    {
        var transform1 = rb.transform;
        var forward1 = transform1.forward;
        Vector3 forward = forward1;
        Vector3 backward = -forward1;
        var right1 = transform1.right;
        Vector3 left = right1 * -1;
        Vector3 right = right1;

        Vector3[] directions = { forward, backward, left, right };
        foreach (var direction in directions)
        {
            Vector3 newPosition = currentPosition + direction * 3.8f;
            if (!IsObstacleInPath(newPosition))
            {
                return newPosition;
            }
        }

        return currentPosition;
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
}
