using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwipeDetection : MonoBehaviour
    {
        private InputManager inputManager;

        [SerializeField] private float minDistance = 0.2f;
        [SerializeField, Range(0f, 1f)] private float dirThreshold = 0.7f;
        
        [Header("Ground Check"),Tooltip("Ground Check Position")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.5f;
        [SerializeField] private bool isGrounded;
        
        [Header("Swipe Detection"),Tooltip("Start Position of Swipe")]
        private Vector2 startPos;
        private float startTime;
        
        [Tooltip("End Position of Swipe"),Header("Swipe Detection")]
        private Vector2 endPos;
        private float endTime;
        
        [Header("Swipe Movement"),Tooltip("Swipe Move Speed")]
        public float jumpForce = 3f;
        private float[] lanes=new float[] {-3.8f,0,3.8f};
        private int currentLaneIndex = 1;
        
        private Rigidbody rb;
        private Vector3 direction = Vector3.zero;
        private Vector2 pendingMovement;
        
        private float minSwipeDistancePixels;
        private float minSwipeDistanceInch = 0.25f;
        public Defines.SwipeDirection swipeDirection; 
        private void Awake()
        {
            inputManager = GetComponent<InputManager>();
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            minSwipeDistancePixels = minSwipeDistanceInch * Screen.dpi;
        }

        private void Update()
        {
            if (!groundCheck)
            {
                groundCheck=GameObject.FindGameObjectWithTag("Ground").transform;
                if (!groundCheck)
                {
                    Debug.LogWarning("Ground Check Not Found");
                    return;
                }
            }
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

        private void OnCollisionEnter(Collision other)
        {
            if (groundCheck !=null && groundCheck.CompareTag("Ground"))
            {
                isGrounded = true;
            }
            
        }
        private void OnCollisionExit(Collision other)
        {
            if (groundCheck!=null && groundCheck.CompareTag("Ground"))
            {
                isGrounded = false;
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

        private void SwipeStart(Vector2 pos, float time)
        {
            Debug.Log("Start");
            startPos = pos;
            
            
            // TODO : Check Second Touch
            
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
                pendingMovement = swipeVector;
            }
        }

        private void UpdateMovement(Vector2 swipeDir)
        {
            Debug.Log($"Attempting to move in direction: {swipeDir}");
            var horizontal = swipeDir.x;
            var vertical = swipeDir.y;

            if (Mathf.Abs(horizontal) > Mathf.Abs(vertical)) {
                if (horizontal > dirThreshold) {
                    swipeDirection = Defines.SwipeDirection.RIGHT;
                    currentLaneIndex = Mathf.Clamp(currentLaneIndex + 1, 0, lanes.Length - 1);
                } else if (horizontal < -dirThreshold) {
                    swipeDirection = Defines.SwipeDirection.LEFT;
                    currentLaneIndex = Mathf.Clamp(currentLaneIndex - 1, 0, lanes.Length - 1);
                }
                var newPos = rb.position;
                newPos.x = lanes[currentLaneIndex];
                rb.MovePosition(newPos);
                Debug.Log($"New Position: {newPos}");
            } else {
                if (vertical > dirThreshold && isGrounded) {
                    swipeDirection = Defines.SwipeDirection.UP;
                    rb.MovePosition(rb.position + Vector3.up * jumpForce);
                }
                else if(!isGrounded && vertical < dirThreshold)
                {
                    Debug.Log("Not Grounded");
                    swipeDirection = Defines.SwipeDirection.DOWN;
                    ImmediateGround();
                }
                else if(vertical< dirThreshold && isGrounded)
                {
                    Debug.Log("Grounded");
                    swipeDirection = Defines.SwipeDirection.DOWN;
                }
                else
                {
                    swipeDirection = Defines.SwipeDirection.NONE;
                }
            }
        }

        private void ImmediateGround()
        {
            var position = rb.position;
            Vector3 groundPosition= new Vector3(position.x,0,position.z);
            rb.MovePosition(groundPosition);
            isGrounded = true;
        }
    }