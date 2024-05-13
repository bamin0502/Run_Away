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
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        switch (playerStateType)
        {
            case PlayerStateType.Run:
                break;
            case PlayerStateType.Jump:
            case PlayerStateType.Slide:
                animator.SetBool(Run, true);
                break;
            case PlayerStateType.Dead:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
