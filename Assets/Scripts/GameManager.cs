using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Note: Removed NUnit and UnityEditor to prevent build compilation errors.

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Questions> questions;
    [SerializeField] private List<GameObject> thorbs;
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
        int nextStep = currentStep + 1;

        foreach (Questions question in questions)
        {
            if (currentPhase == question.phase &&
                nextStep == question.step)
            {
                currentStep = nextStep;
                currentQuestion = question;

                Debug.Log($"Moved to Phase: {currentQuestion.phase} | Step: {currentQuestion.step}");
                return;
            }
        }

        currentQuestion = null;
        Debug.Log("No more questions found for this phase.");
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

    public void checkAnswer(Thorb activatedThorb = null, SocketTool insertedTool = null)
    {
        if (currentQuestion == null) return;

        if (currentQuestion.type == QuestionTypes.questionTypes.toolQuestion)
        {
            ToolQuestion currentToolQuestion = (ToolQuestion)currentQuestion;

            GameObject toolObject = insertedTool != null
                ? insertedTool.gameObject
                : selectedTool;

            GameObject toolLocation = activatedThorb != null
                ? activatedThorb.gameObject
                : selectedToolLocation;

            if (currentToolQuestion.correctAnswer == toolObject &&
                currentToolQuestion.correctArea == toolLocation)
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

            GameObject selectedObject = activatedThorb != null
                ? activatedThorb.gameObject
                : selectedThorb;

            if (currentModelQuestion.correctAnswer == selectedObject)
            {
                nextQuestion();
            }
            else
            {
                Debug.Log("Wrong Object Selected!");
            }
        }
    }
    private void OnEnable()
    {
        Thorb.OnThorbActivated += HandleThorbActivated;
    }

    private void OnDisable()
    {
        Thorb.OnThorbActivated -= HandleThorbActivated;
    }

    private void HandleThorbActivated(Thorb thorb, SocketTool tool)
    {
        if (thorb == null || tool == null) return;

        Debug.Log($"Thorb activated: {thorb.name}");
        Debug.Log($"Tool inserted: {tool.name}");

        checkAnswer(thorb, tool);
    }
}
