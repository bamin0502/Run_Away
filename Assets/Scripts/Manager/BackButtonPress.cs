using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BackButtonPress : MonoBehaviour
{
    private static BackButtonPress instance;
    
    public delegate void BackButtonPressAction();
    public static event BackButtonPressAction OnBackButtonPress;
    
    private BackButton backButton;
    private UiManager uiManager;
    private IntroUI introUI;
    
    void Awake()
    {
        backButton = new BackButton();
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        backButton.Enable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        backButton.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        backButton.Back.Back.performed += OnBackAction;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "StartScene")
        {
            introUI = GameObject.FindGameObjectWithTag("Intro").GetComponent<IntroUI>();
        }
        if(scene.name == "GameScene")
        {
            uiManager = GameObject.FindGameObjectWithTag("UiManager").GetComponent<UiManager>();
        }
    }
    private void OnBackAction(InputAction.CallbackContext context)
    {
        OnBackButtonPress?.Invoke();
#if UNITY_EDITOR
        Debug.Log("Back button pressed");
#endif
        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            if (introUI == null) return;

            if (introUI.QuitPanel.activeSelf)
            {
                introUI.QuitPanel.SetActive(false);
            }
            else
            {
                introUI.QuitPanel.SetActive(true);
            }
        }

        if (SceneManager.GetActiveScene().name == "GameScene")
        {
#if UNITY_EDITOR
            Debug.Log("GameScene");
#endif
            if (uiManager == null) return;

            if (uiManager.GameOverPanel.activeSelf)
            {
                return;
            }

            if (uiManager.GameMenuPanel.activeSelf)
            {
                if (uiManager.QuitPanel.activeSelf)
                {
                    uiManager.HideQuitPanel();
                }
                else
                {
                    uiManager.ShowQuitPanel();
                }

                return;
            }

            if (uiManager.GamePanel.activeSelf)
            {
                if (uiManager.PausePanel.activeSelf)
                {
                    uiManager.HidePausePanel();
                }
                else if (!uiManager.QuitPanel.activeSelf)
                {
                    uiManager.ShowPausePanel();
                }
            }
            else if (uiManager.QuitPanel.activeSelf)
            {
                uiManager.HideQuitPanel();
            }
            
        }
    }
}

