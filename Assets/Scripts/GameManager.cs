using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Note: Removed NUnit and UnityEditor to prevent build compilation errors.

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Questions> questions;
    [SerializeField] private GameObject player;

    // Connect this to your hand script that tracks the grabbed object
    [SerializeField] private GetGrabbedObject toolGetter;

    private bool gameStarted = false;
    private int currentStep = 0;
    private int currentPhase = 0;

    private Questions currentQuestion;
    public object currentAnswer;

    // References updated via the toolGetter
    private GameObject selectedTool;
    private ToolType.toolType selectedToolType; // Assumes your ToolType enum has a 'None' or 'Unknown' default option
    private GameObject selectedThorb;
    private GameObject selectedToolLocation;
    private float selectedMeasurement;

    void Start()
    {
        // Initialize game state or call startGame() here if needed
    }

    void Update()
    {
        // 1. Cleanly update what the player is holding using your toolGetter script
        if (toolGetter != null && toolGetter.holdingTool)
        {
            // Safely fetch the current grabbed object from your hand tracker
            selectedTool = toolGetter.grabbedObject;

            if (selectedTool != null && selectedTool.GetComponent<Tool>() != null)
            {
                selectedToolType = selectedTool.GetComponent<Tool>().getToolType();
            }
        }
        else
        {
            selectedTool = null;
            selectedToolType = ToolType.toolType.None; // Set to your enum's default fallback value
        }

        // 2. Process what needs evaluation based on the question type
        if (currentQuestion == null) return;

        if (currentQuestion.type == QuestionTypes.questionTypes.toolQuestion)
        {
            if (selectedTool != null)
            {
                // Optional: Extract location logic from the held tool if necessary
                // selectedToolLocation = selectedTool.GetComponent<Tooltip>()?.gameObject;
            }
        }
        else if (currentQuestion.type == QuestionTypes.questionTypes.actionLocationQuestion)
        {
            // If they need to grab a specific object (Thorb), check if the grabbed tool is it
            selectedThorb = selectedTool;
        }
        else if (currentQuestion.type == QuestionTypes.questionTypes.measurementQuestion)
        {
            // UI reading logic goes here
        }
    }

    public void nextQuestion()
    {
        // Adjusted logic to advance steps normally, or handle phase thresholds cleanly
        ++currentStep;

        // Search your list for the question matching the new step/phase
        foreach (Questions question in questions)
        {
            if (currentPhase == question.phase && currentStep == question.step)
            {
                currentQuestion = question;
                break; // Found it, stop searching
            }
        }

        if (currentQuestion != null)
        {
            Debug.Log("Moved to Phase: " + currentQuestion.phase + " | Step: " + currentQuestion.step);
        }
    }

    public void startGame()
    {
        currentStep = 1;
        currentPhase = 1;
        gameStarted = true;

        // Load your first question matching phase 1, step 1
        foreach (Questions question in questions)
        {
            if (question.phase == 1 && question.step == 1)
            {
                currentQuestion = question;
                break;
            }
        }
        Debug.Log("Game Started");
    }

    public void checkAnswer()
    {
        if (currentQuestion == null) return;

        if (currentQuestion.type == QuestionTypes.questionTypes.toolQuestion)
        {
            ToolQuestion currentToolQuestion = (ToolQuestion)currentQuestion;

            if (currentToolQuestion.correctAnswer == selectedTool && currentToolQuestion.correctArea == selectedToolLocation)
            {
                nextQuestion();
            }
            else
            {
                Debug.Log("Wrong Tool or Location!");
            }
        }
        else if (currentQuestion.type == QuestionTypes.questionTypes.measurementQuestion)
        {
            MeasurementQuestion currentMeasurementQuestion = (MeasurementQuestion)currentQuestion;

            if (Mathf.Approximately(currentMeasurementQuestion.correctAnswer, selectedMeasurement))
            {
                nextQuestion();
            }
            else
            {
                Debug.Log("Wrong Measurement!");
            }
        }
        else if (currentQuestion.type == QuestionTypes.questionTypes.actionLocationQuestion)
        {
            ModelQuestion currentModelQuestion = (ModelQuestion)currentQuestion;

            if (currentModelQuestion.correctAnswer == selectedThorb)
            {
                nextQuestion();
            }
            else
            {
                Debug.Log("Wrong Object Selected!");
            }
        }
    }
}
