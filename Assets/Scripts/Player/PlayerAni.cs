using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAni : MonoBehaviour
{
    [Tooltip("플레이어 애니메이터")]
    private Animator ani;
    [Tooltip("게임 매니저")]
    private GameManager gameManager;
    [Tooltip("UI 매니저")]
    private UiManager uiManager;
    
    [SerializeField]private SkinnedMeshRenderer playerRenderer;
    private Color originalColor;
    private bool isFlashing;
    
    [Tooltip("애니메이션 관련 해쉬코드")] 
    private static readonly int IsDead = Animator.StringToHash("isDead");
    private static readonly int IsRun = Animator.StringToHash("isRun");
    private static readonly int IsJump = Animator.StringToHash("isJump");
    private static readonly int IsSlide = Animator.StringToHash("isSlide");

    private bool deathTrigger;
    
    private void Awake()
    {
        playerRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        originalColor = playerRenderer.material.color;
        ani = GetComponent<Animator>();
        uiManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<UiManager>();
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
    }
    public void SetInvincibleAnimation()
    {
        if (!isFlashing)
        {
            StartCoroutine(Flash());
        }
    }
    
    private IEnumerator Flash()
    {
        isFlashing = true;
        while (isFlashing)
        {
            var material = playerRenderer.material;
            material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            material.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void StopInvincibleAnimation()
    {
        isFlashing = false;
        playerRenderer.material.color = originalColor;
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
        ani.SetBool(IsSlide, false);
        ani.SetBool(IsJump, false);
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