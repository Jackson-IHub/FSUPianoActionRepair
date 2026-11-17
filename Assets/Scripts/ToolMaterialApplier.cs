using UnityEngine;

public class ToolMaterialApplier : MonoBehaviour
{
    [SerializeField] private GameManager manager;

    private void Start()
    {
        manager.OnCheckAnswer.AddListener(CheckAnswer);
        CheckAnswer();
    }

    private void CheckAnswer()
    {   
        if (!manager.currentQuestionAnsweredIncorrectly) return;

        Questions currentQuestion = manager.currentQuestion;
        if (currentQuestion.type == QuestionTypes.questionTypes.measurementQuestion) return;

        foreach (Transform child in transform)
        {
            ToolType.toolType correctTool;
            int correctArea;

            if (currentQuestion.type == QuestionTypes.questionTypes.toolQuestion)
            {
                ToolQuestion question = (ToolQuestion)currentQuestion;
                correctTool = question.correctTool;
                correctArea = question.correctArea;
            }
            else
            {
                ModelQuestion question = (ModelQuestion)currentQuestion;
                correctTool = question.correctTool;
                correctArea = question.correctArea;
            }

            var socket = child.GetComponent<CustomSocket>();
            var thorb = child.GetComponent<Thorb>();
               
            Tool toolScript = socket.AttachedTool.GetComponent<Tool>();
            ToolType.toolType actualToolType = toolScript.getToolType();
            int actualArea = thorb.ID;

            if (actualToolType == correctTool && actualArea == correctArea)
            {
                // TODO: Apply pulsing red material to the tool
            }
        }
    }

}
