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
        var isRunning = swipeDetection.swipeDirection == Defines.SwipeDirection.RUN;
        var isJumping = swipeDetection.swipeDirection == Defines.SwipeDirection.JUMP;
        var isSliding = swipeDetection.swipeDirection == Defines.SwipeDirection.SLIDE;

        ani.SetBool(IsRun, isRunning);
        ani.SetBool(IsJump, isJumping);
        ani.SetBool(IsSlide, isSliding);
    }
}
