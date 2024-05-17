using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;

    private int currentStep = 0;
    private bool tutorialActive = true;
    
    private GameManager gameManager;
    void Awake()
    {
        
    }

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        
        
    }


    private void Update()
    {
        
    }
}
