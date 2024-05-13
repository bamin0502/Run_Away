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
        switch (swipeDetection.swipeDirection) {
            case Defines.SwipeDirection.UP:
                ani.SetBool(IsJump, true);
                ani.SetBool(IsRun, false);
                ani.SetBool(IsSlide, false);
                break;
            case Defines.SwipeDirection.DOWN:
                ani.SetBool(IsSlide, true);
                ani.SetBool(IsJump, false);
                ani.SetBool(IsRun, false);
                break;
            case Defines.SwipeDirection.LEFT:
            case Defines.SwipeDirection.RIGHT:
            case Defines.SwipeDirection.NONE:
                ani.SetBool(IsRun, true);
                ani.SetBool(IsJump, false);
                ani.SetBool(IsSlide, false);
                break;
            case Defines.SwipeDirection.ERROR:
                ani.SetTrigger(IsDead);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        // swipeDetection.swipeDirection = Defines.SwipeDirection.NONE;
    }
}
