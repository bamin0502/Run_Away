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

    private bool deathTrigger;

    private void Awake()
    {
        ani = GetComponent<Animator>();
        swipeDetection = GetComponent<SwipeDetection>();
    }

    private void Start()
    {
        deathTrigger = false;
        ani.SetBool(IsRun, true);
    }
    
    private void Update()
    {
        if (GameManager.Instance.isGameover && !deathTrigger)
        {
            ani.SetTrigger(IsDead);
            ani.SetBool(IsRun, false);
            ani.SetBool(IsJump, false);
            ani.SetBool(IsSlide, false);
            deathTrigger = true;
            return;
        }

        if (deathTrigger) return;

        if (swipeDetection.isGrounded)
        {
            ani.SetBool(IsJump, false);
            ani.SetBool(IsRun, true);
        }
        else
        {
            ani.SetBool(IsJump, true);
            ani.SetBool(IsRun, false);
        }

        ani.SetBool(IsSlide, swipeDetection.swipeDirection == Defines.SwipeDirection.SLIDE);
    }

    public void EndResult()
    {
        UiManager.Instance.ShowGameOverPanel();
    }
}