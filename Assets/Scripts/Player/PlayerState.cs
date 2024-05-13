using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : StateMachineBehaviour
{
    private static readonly int IsJump = Animator.StringToHash("isJump");
    private static readonly int IsSlide = Animator.StringToHash("isSlide");
    private static readonly int IsRun = Animator.StringToHash("isRun");

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("Jump"))
        {
            animator.SetBool(IsJump, false);
        }
        else if (stateInfo.IsName("Slide"))
        {
            animator.SetBool(IsSlide, false);
        }

    }
    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        
    }
}
