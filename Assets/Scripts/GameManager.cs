
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Questions> questions;
    [SerializeField] private GameObject MeasurementSliderPanel;
    [SerializeField] private MeasurementSlider measurementSlider;
    [SerializeField] private GameObject measurementButton;

    [SerializeField] private Questions finalPlaceholderQuestion;

    public bool currentQuestionAnsweredIncorrectly = false;
    public Questions currentQuestion;

    [SerializeField] private ToolSocketManager toolSocketManager;
    [SerializeField] private ToolSocketManager caliperSocketManager;

    [SerializeField] private ParticleSystem confettiParticles;

    const float DETACH_TOOLS_AFTER = 3.0f;  // Seconds

    private int currentStep = 0;
    private int currentPhase = 0;

    private float selectedMeasurement;

    public UnityEvent OnCheckAnswer;
    private void OnEnable()
    {
        Thorb.OnToolInsertedIntoThorb += HandleToolInserted;
    }

    private void OnDisable()
    {
        Thorb.OnToolInsertedIntoThorb -= HandleToolInserted;
    }

    private void Awake()
    {
        StartGame();
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
            currentQuestion = finalPlaceholderQuestion;
            Debug.Log("All questions completed!");
            return;
        }
        else 
        {
            currentStep = nextStep;
            currentPhase = nextPhase;
            currentQuestion = next;
        }
            

        Debug.Log($"Moved to Step: {currentStep} | Phase: {currentPhase}");
        if (currentQuestion.type == QuestionTypes.questionTypes.measurementQuestion)
        {
            MeasurementQuestion question = (MeasurementQuestion)currentQuestion;
            MeasurementSliderPanel.SetActive(true);
            measurementButton.SetActive(true);
            measurementSlider.setBounds(question.minimumBounds, question.maximumBounds, question.sliderIncrement);
        }
        else 
        {
            MeasurementSliderPanel.SetActive(false);
            measurementButton.SetActive(false);
        }
        currentQuestionAnsweredIncorrectly = false;
        
    }

    private IEnumerator DetachTools()
    {
        yield return new WaitForSeconds(DETACH_TOOLS_AFTER);

        // Move all tools back to toolbox
        toolSocketManager.DetachTools();
        caliperSocketManager.DetachTools();

    }

    private void HandleToolInserted(Thorb thorb, SocketTool socketTool)
    {
        CheckAnswer(thorb, socketTool);
    }

    public void CheckAnswer(Thorb thorb = null, SocketTool socketTool = null)
    {


        if (currentQuestion == null) return;

        if (currentQuestion.type == QuestionTypes.questionTypes.toolQuestion ||
            currentQuestion.type == QuestionTypes.questionTypes.actionLocationQuestion)
        {
            if (thorb == null || socketTool == null)
            {
                Debug.LogWarning("Missing Thorb or inserted tool.");
                currentQuestionAnsweredIncorrectly = true;
                OnCheckAnswer?.Invoke();
                return;
            }

            Tool tool = socketTool.GetComponent<Tool>();

            if (tool == null)
            {
                Debug.LogError($"No Tool component found on {socketTool.name}.");
                currentQuestionAnsweredIncorrectly = true;
                OnCheckAnswer?.Invoke();
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

                // Display confetti
                confettiParticles.Play();

                // Move tools back
                StartCoroutine(DetachTools());

                NextQuestion();
                currentQuestionAnsweredIncorrectly = false;
            }
            else
            {
                Debug.Log($"Wrong answer! Tool: {actualToolType}, Location: {actualLocationID}");
                currentQuestionAnsweredIncorrectly = true;
            }
        }
        else if (currentQuestion.type == QuestionTypes.questionTypes.measurementQuestion)
        {
            MeasurementQuestion question = (MeasurementQuestion)currentQuestion;

            if (Mathf.Approximately(question.correctAnswer, selectedMeasurement))
                NextQuestion();
            else
                Debug.Log($"Wrong Measurement!: {selectedMeasurement}");
                currentQuestionAnsweredIncorrectly = true;
        }

        OnCheckAnswer?.Invoke();
    }

    public void CheckAnswerWrapper()
    {
        CheckAnswer();
        Debug.Log("Im doing something");
    }
    // Call this from your measurement system when a reading is submitted.
    public void SetSelectedMeasurement()
    {
        selectedMeasurement = measurementSlider.getValue();
    }

    private void Start()
    {
        StartGame();   
    }
}
