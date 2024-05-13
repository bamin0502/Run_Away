using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAni : MonoBehaviour
{
    private Animator ani;
    private SwipeDetection swipeDetection;
    
    [Tooltip("애니메이션 관련 해쉬코드")]
    private static readonly int IsDead = Animator.StringToHash("isDead");
    private static readonly int IsRun = Animator.StringToHash("isRun");
    private static readonly int IsJump = Animator.StringToHash("isJump");
    private static readonly int IsSlide = Animator.StringToHash("isSlide");

    private void Awake()
    {
        ani = GetComponent<Animator>();
        swipeDetection = GetComponent<SwipeDetection>();
    }

    private void Update()
    {
        if (GameManager.Instance.isGameover) return;
        
        if (swipeDetection.isGrounded)
        {
            ani.SetBool(IsRun, true);
            ani.SetBool(IsJump, false);
        }
        else
        {
            ani.SetBool(IsRun, false);
            ani.SetBool(IsJump, true);
        }
        
        if(swipeDetection.swipeDirection == Defines.SwipeDirection.DEAD)
        {
            ani.SetTrigger(IsDead);
            ani.SetBool(IsRun, false);
            ani.SetBool(IsJump, false);
        }
        
        if(swipeDetection.swipeDirection == Defines.SwipeDirection.SLIDE)
        {
            ani.SetTrigger(IsSlide);
        }
        
    

    }
}
