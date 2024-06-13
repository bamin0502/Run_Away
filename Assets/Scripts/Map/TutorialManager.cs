using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject modalPrefab;
    public Canvas canvas;
    public TutorialStep[] tutorialSteps;

    private int currentStep = 0;
    private bool tutorialActive = true;
    private GameManager gameManager;
    private ModalWindow currentModal;
    private UiManager uiManager;

    private Action onComplete;

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager")?.GetComponent<GameManager>();
        uiManager = GameObject.FindGameObjectWithTag("UiManager")?.GetComponent<UiManager>();
    }

    public void StartTutorial(Action onCompleteAction)
    {
        onComplete = onCompleteAction;
        tutorialActive = true;
        currentStep = 0;
        ShowNextStep();
    }

    private void ShowNextStep()
    {
        if (currentModal != null)
        {
            Destroy(currentModal.gameObject);
        }

        if (currentStep >= tutorialSteps.Length)
        {
            EndTutorial();
            return;
        }

        TutorialStep step = tutorialSteps[currentStep];

        currentModal = ModalWindow.Create(modalPrefab, canvas);
        currentModal.SetHeader(step.title);
        currentModal.SetBody(step.content);
        currentModal.SetButton(step.buttonText, OnModalButtonClick);
        currentModal.SetImage(step.imagePath);
        currentModal.Show();

#if UNITY_EDITOR
        Debug.Log("Showing step: " + currentStep);
#endif
    }

    private void OnModalButtonClick()
    {
#if UNITY_EDITOR
        Debug.Log("Button clicked, current step: " + currentStep);
#endif
        currentStep++;
        ShowNextStep();
    }

    private void EndTutorial()
    {
        tutorialActive = false;
        if (gameManager != null)
        {
            gameManager.isTutorialActive = false;
            gameManager.SaveGameData();
            gameManager.ResumeGame();
        }

        onComplete?.Invoke();

#if UNITY_EDITOR
        Debug.Log("Tutorial ended");
#endif
    }
}
