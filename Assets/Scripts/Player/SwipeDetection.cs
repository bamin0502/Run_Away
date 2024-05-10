using System;
using System.Collections;
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
        
        private Vector2 startPos;
        private float startTime;

        private Vector2 endPos;
        private float endTime;
        
        private Rigidbody rb;
        private Vector3 direction = Vector3.zero;
        
        public float swipeMove = 3.8f;
        public float jumpForce = 3f;
        
        
        private float minSwipeDistancePixels;
        public float minSwipeDistanceInch = 0.25f;

        private Vector3 dir=Vector3.zero;
        
        [SerializeField]
        private bool isGrounded;
        
        
        private void Awake()
        {
            inputManager = GetComponent<InputManager>();
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            minSwipeDistancePixels = minSwipeDistanceInch * Screen.dpi;
            groundCheck=GameObject.FindGameObjectWithTag("Ground").transform;
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
            if (groundCheck.CompareTag("Ground"))
            {
                isGrounded = true;
            }
        }
        private void OnCollisionExit(Collision other)
        {
            if (groundCheck.CompareTag("Ground"))
            {
                isGrounded = false;
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

        private void Update()
        {
            
        }

        private void DetectSwipe()
        {
            var swipeVector = endPos - startPos;
            var distance = Mathf.Clamp(swipeVector.magnitude, 0f, minSwipeDistancePixels);
            if (distance >= minDistance)
            {
                swipeVector.Normalize();
                UpdateMovement(swipeVector);
            }
        }

        private void UpdateMovement(Vector2 direction)
        {
            var horizontal = direction.x;
            var vertical = direction.y;

            if (Mathf.Abs(horizontal) > Mathf.Abs(vertical)) {
                if (horizontal > dirThreshold) {
                    rb.MovePosition(rb.position + Vector3.right * swipeMove);
                } else if (horizontal < -dirThreshold) {
                    rb.MovePosition(rb.position + Vector3.left * swipeMove);
                }
            } else {
                if (vertical > dirThreshold && isGrounded) {
                    rb.MovePosition(rb.position + Vector3.up * jumpForce);
                }
                else
                {
                    rb.MovePosition(rb.position +new Vector3(0,0.5f,0));
                }
            }
        }
    }