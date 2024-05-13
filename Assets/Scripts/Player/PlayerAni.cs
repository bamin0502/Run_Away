using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAni : MonoBehaviour
{
    private Animator ani;
    private SwipeDetection swipeDetection;
    
    [Tooltip("애니메이션 관련 해쉬코드")]
    private static readonly int IsDead = Animator.StringToHash("isDead");
    private static readonly int IsRun = Animator.StringToHash("isRun");
    private static readonly int IsJump = Animator.StringToHash("isJump");
    private static readonly int IsSlide = Animator.StringToHash("isSlide");
    
    void Awake()
    {
        ani = GetComponent<Animator>();
        swipeDetection = GetComponent<SwipeDetection>();
    }

    void Update()
    {
        if (swipeDetection.isGrounded)
        {
            switch (swipeDetection.swipeDirection)
            {
                case Defines.SwipeDirection.UP:
                    ani.Play(IsJump);
                    break;
                case Defines.SwipeDirection.DOWN:
                    ani.Play(IsSlide);
                    break;
                case Defines.SwipeDirection.LEFT:
                case Defines.SwipeDirection.RIGHT:
                    ani.SetBool(IsRun, true);
                    break;
                case Defines.SwipeDirection.ERROR:
                    ani.SetTrigger(IsDead);
                    break;
                case Defines.SwipeDirection.NONE:
                    ani.SetBool(IsRun, true);
                    break;
                default:
                    ani.SetBool(IsRun, true);
                    break;
            }
        }
        
    }
}
