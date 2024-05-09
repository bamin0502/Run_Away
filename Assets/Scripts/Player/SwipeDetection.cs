using System;
using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(Rigidbody))]
public class SwipeDetection : MonoBehaviour
    {
        private InputManager inputManager;

        [SerializeField] private float minDistance = 0.2f;
        [SerializeField, Range(0f, 1f)] private float dirThreshold = 0.7f;
        
        private Vector2 startPos;
        private float startTime;

        private Vector2 endPos;
        private float endTime;
        
        private Rigidbody rb;
        private Vector3 direction = Vector3.zero;
        
        public float swipeMove = 5f;
        public float jumpForce = 5f;
        public float groundDistance = 0.5f;
        
        private float minSwipeDistancePixels;
        public float minSwipeDistanceInch = 0.25f;

        private Vector3 dir=Vector3.zero;
        
        
        private void Awake()
        {
            inputManager = GetComponent<InputManager>();
            rb = GetComponent<Rigidbody>();
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

        private void FixedUpdate()
        {
            
        }

        #region Input Methods
        public void SwipeStart(Vector2 pos, float time)
        {
            Debug.Log("Start");
            startPos = pos;
            
            
            // TODO : Check Second Touch
            
        }
        
        public void SwipeEnd(Vector2 pos, float time)
        {
            endPos = pos;
            Debug.Log("End");
            DetectSwipe();
        }

        private void DetectSwipe()
        {
            Vector2 swipeVector = endPos - startPos;
            float distance = swipeVector.magnitude;
            if (distance >= minDistance)
            {
                swipeVector.Normalize();
                UpdateMovement(swipeVector);
            }
        }

        private void UpdateMovement(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal movement
                if (direction.x > 0)
                {
                    // 오른쪽으로 스와이프
                    dir = new Vector3(5, 0.5f, 0); // x, y, z
                }
                else
                {
                    // 왼쪽으로 스와이프
                    dir = new Vector3(-5, 0.5f, 0); // x, y, z
                }
            }
            else
            {
                // Vertical movement
                if (direction.y > 0)
                {
                    // 위로 스와이프
                    dir = new Vector3(0, 2, 0); // x, y, z
                }
                else
                {
                    // 아래로 스와이프
                    dir = new Vector3(0, 0.5f, 0); // x, y, z
                }
            }
            
            rb.MovePosition(rb.position+dir);
        }
        
        #endregion
        
        
        

    }