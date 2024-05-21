using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAni : MonoBehaviour
{
    private Animator ani;
    private GameManager gameManager;
    [Tooltip("애니메이션 관련 해쉬코드")] 
    private static readonly int IsDead = Animator.StringToHash("isDead");
    private static readonly int IsRun = Animator.StringToHash("isRun");
    private static readonly int IsJump = Animator.StringToHash("isJump");
    private static readonly int IsSlide = Animator.StringToHash("isSlide");

    private bool deathTrigger;
    private UiManager uiManager;
    private void Awake()
    {
        ani = GetComponent<Animator>();
        uiManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<UiManager>();
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
    }

    private void Start()
    {
        deathTrigger = false;
    }
    
    private void Update()
    {
        if (!gameManager.isPlaying)
        {
            ani.SetBool(IsRun, false);
        }
        
        if (gameManager.isGameover && !deathTrigger)
        {
            SetDeathAnimation();
            deathTrigger = true;
        }
    }

    public void SetRunAnimation()
    {
        ani.SetBool(IsRun, true);
        ani.SetBool(IsJump, false);
        ani.SetBool(IsSlide, false);
    }

    public void SetJumpAnimation()
    {
        ani.SetBool(IsRun, false);
        ani.SetBool(IsJump, true);
        ani.SetBool(IsSlide, false);
    }

    public void SetSlideAnimation()
    {
        ani.SetBool(IsRun, false);
        ani.SetBool(IsJump, false);
        ani.SetBool(IsSlide, true);
    }

    public void SetDeathAnimation()
    {
        ani.SetTrigger(IsDead);
        ani.SetBool(IsRun, false);
        ani.SetBool(IsJump, false);
        ani.SetBool(IsSlide, false);
    }

    public void EndResult()
    {
        uiManager.ShowGameOverPanel();
    }
}