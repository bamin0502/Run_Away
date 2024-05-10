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

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        switch (playerStateType)
        {
            case PlayerStateType.Run:
                Debug.Log("Run");
                break;
            case PlayerStateType.Jump:
                Debug.Log("Jump");
                break;
            case PlayerStateType.Slide:
                Debug.Log("Slide");
                break;
            case PlayerStateType.Dead:
                Debug.Log("Dead");
                break;
            default:
                throw new System.ArgumentOutOfRangeException();
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
                throw new System.ArgumentOutOfRangeException();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
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
                throw new System.ArgumentOutOfRangeException();
        }
    }
}
