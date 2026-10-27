
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Questions> questions;

    private int currentStep = 0;
    private int currentPhase = 0;

    public Questions currentQuestion;
    private float selectedMeasurement;

    private void OnEnable()
    {
        Thorb.OnToolInsertedIntoThorb += HandleToolInserted;
    }

    private void OnDisable()
    {
        Thorb.OnToolInsertedIntoThorb -= HandleToolInserted;
    }

    public void StartGame()
    {
        currentStep = 1;
        currentPhase = 1;
        currentQuestion = FindQuestion(currentStep, currentPhase);

        if (currentQuestion != null)
            Debug.Log("Game Started: Step 1 | Phase 1");
        else
            Debug.LogError("Could not find the first question.");
    }

    private Questions FindQuestion(int step, int phase)
    {
        return questions.Find(q => q.step == step && q.phase == phase);
    }

    public void NextQuestion()
    {
        int nextStep = currentStep;
        int nextPhase = currentPhase + 1;

        if (nextPhase > 3)
        {
            nextPhase = 1;
            nextStep++;
        }

        Questions next = FindQuestion(nextStep, nextPhase);

        if (next == null)
        {
            currentQuestion = null;
            Debug.Log("All questions completed!");
            return;
        }

        currentStep = nextStep;
        currentPhase = nextPhase;
        currentQuestion = next;

        Debug.Log($"Moved to Step: {currentStep} | Phase: {currentPhase}");
    }

    private void HandleToolInserted(Thorb thorb, SocketTool socketTool)
    {
        CheckAnswer(thorb, socketTool);
    }

    public void CheckAnswer(Thorb thorb = null, SocketTool socketTool = null)
    {
        Debug.Log(socketTool.name);
        if (currentQuestion == null) return;

        if (currentQuestion.type == QuestionTypes.questionTypes.toolQuestion ||
            currentQuestion.type == QuestionTypes.questionTypes.actionLocationQuestion)
        {
            if (thorb == null || socketTool == null)
            {
                Debug.LogWarning("Missing Thorb or inserted tool.");
                return;
            }

            Tool tool = socketTool.GetComponent<Tool>();

            if (tool == null)
            {
                Debug.LogError($"No Tool component found on {socketTool.name}.");
                return;
            }

            ToolType.toolType actualToolType = tool.getToolType();
            int actualLocationID = thorb.ID;

            bool correct;

            if (currentQuestion.type == QuestionTypes.questionTypes.toolQuestion)
            {
                ToolQuestion question = (ToolQuestion)currentQuestion;

                correct = question.correctTool == actualToolType &&
                          question.correctArea == actualLocationID;
            }
            else
            {
                ModelQuestion question = (ModelQuestion)currentQuestion;

                correct = question.correctTool == actualToolType &&
                          question.correctArea == actualLocationID;
            }

            if (correct)
            {
                Debug.Log("Correct tool and location!");
                NextQuestion();
            }
            else
            {
                Debug.Log($"Wrong answer! Tool: {actualToolType}, Location: {actualLocationID}");
            }
        }
        else if (currentQuestion.type == QuestionTypes.questionTypes.measurementQuestion)
        {
            MeasurementQuestion question = (MeasurementQuestion)currentQuestion;

            if (Mathf.Approximately(question.correctAnswer, selectedMeasurement))
                NextQuestion();
            else
                Debug.Log("Wrong Measurement!");
        }
    }

    // Call this from your measurement system when a reading is submitted.
    public void SetSelectedMeasurement(float measurement)
    {
        selectedMeasurement = measurement;
    }

    private void Start()
    {
        StartGame();   
    }
}
