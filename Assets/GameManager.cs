using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.XR.CoreUtils;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Questions> questions;

    [SerializeField] private GameObject player;  

    private bool gameStarted = false;

    private int currentStep = 0;
    private int currentPhase = 0;
    private Questions currentQuestion;
    private QuestionTypes currentType;
    public object currentAnswer;

    private GameObject selectedTool;
    private GameObject selectedThorb;
    private GameObject selectedToolLocation;
    private float selectedMeasurement;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentQuestion.type == QuestionTypes.questionTypes.toolQuestion)
        {
            selectedTool = player.GetComponent<XRGrabInteractable>()?.gameObject;
            if (selectedTool != null)
            {
                //Script to read the tool's location input
                //selectedToolLocation = selectedTool.GetComponent<tooltip>()?.gameobject
            }
        }
        else if (currentQuestion.type == QuestionTypes.questionTypes.actionLocationQuestion)
        {
            selectedThorb = player.GetComponent<XRGrabInteractable>()?.gameObject;
        }
        else if (currentQuestion.type == QuestionTypes.questionTypes.measurementQuestion)
        {
            //UI reading in the measurement selection
        }
    }
    
    public void nextQuestion()
    {
        if (currentQuestion.phase <= 3)
        {
            currentPhase = 1;
            ++currentStep;
        }
        else
        {
            ++currentPhase;
        }
            foreach (Questions question in questions)
            {
                if (currentPhase == question.phase && currentStep == question.step)
            {
               currentQuestion = question;
            }
            }

        Debug.Log("Phase" + currentQuestion.phase);
        Debug.Log("Step" + currentQuestion.step);

    }
    
    public void startGame()
    {
        ++currentStep;
        gameStarted = true;
        Debug.Log("Game Started");
    }
    public void checkAnswer()
    {
    if (currentQuestion.type == QuestionTypes.questionTypes.toolQuestion)
    {
        ToolQuestion currentToolQuestion = (ToolQuestion)currentQuestion;

        if (currentToolQuestion.correctAnswer == selectedTool && currentToolQuestion.correctArea == selectedToolLocation)
            {
                nextQuestion();
            }
        else
        {
            //Wrong answer check
        }
    }
    if (currentQuestion.type == QuestionTypes.questionTypes.measurementQuestion)
    {
        
        MeasurementQuestion currentMeasurementQuestion = (MeasurementQuestion)currentQuestion;

        if (currentMeasurementQuestion.correctAnswer == selectedMeasurement)
            {
                nextQuestion();
            }
        else
        {
            //wrong answer check
        }
    }
    if (currentQuestion.type == QuestionTypes.questionTypes.actionLocationQuestion)
    {
        ModelQuestion currentModelQuestion = (ModelQuestion)currentQuestion;

            if (currentModelQuestion.correctAnswer == selectedThorb)
            {
                nextQuestion();
            }
            else
            {
                //wrong answer check
            }
        }
    }
}
