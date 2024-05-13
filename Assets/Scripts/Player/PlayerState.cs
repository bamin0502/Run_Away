using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : StateMachineBehaviour
{
    public enum PlayerStateType
    {
        Run,
        Jump,
        Slide,
        Dead
    }

    public PlayerStateType playerStateType;
    private static readonly int Run = Animator.StringToHash("isRun");
    private static readonly int IsJump = Animator.StringToHash("isJump");
    private static readonly int IsSlide = Animator.StringToHash("isSlide");

    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        switch (playerStateType)
        {
            case PlayerStateType.Run:
                Debug.Log("Run");
                animator.ResetTrigger(IsJump);
                animator.ResetTrigger(IsSlide);
                break;
            case PlayerStateType.Jump:
                Debug.Log("Jump");
                animator.ResetTrigger(IsSlide);
                break;
            case PlayerStateType.Slide:
                Debug.Log("Slide");
                animator.ResetTrigger(IsJump);
                break;
            case PlayerStateType.Dead:
                Debug.Log("Dead");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        switch (playerStateType)
        {
            case PlayerStateType.Run:
                break;
            case PlayerStateType.Jump:
                
                break;
            case PlayerStateType.Slide:
                break;
            case PlayerStateType.Dead:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        switch (playerStateType)
        {
            case PlayerStateType.Run:
                break;
            case PlayerStateType.Jump:
                animator.SetBool(Run,true);
                break;
            case PlayerStateType.Slide:
                animator.SetBool(Run,true);
                break;
            case PlayerStateType.Dead:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
