using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Questions> questions;

    private bool gameStarted = false;

    private int currentStep = 0;
    private int currentPhase = 0;
    private Questions currentQuestion;
    private QuestionTypes currentType;
    public object currentAnswer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            if (currentStep != currentQuestion.step || currentPhase != currentQuestion.phase)
            {

            }
        }
    }
    
    public void changeQuestion()
    {

    }
    
    public void startGame()
    {
        ++currentStep;
        gameStarted = true;
    }
    public void checkAnswer()
    {
    if (currentQuestion.type == QuestionTypes.questionTypes.toolQuestion)
    {
        //do something
    }
    if (currentQuestion.type == QuestionTypes.questionTypes.measurementQuestion)
    {
        //do something
    }
    if (currentQuestion.type == QuestionTypes.questionTypes.actionLocationQuestion)
    {
        //do something
    }
    }
}
