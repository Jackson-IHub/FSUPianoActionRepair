using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class QuestionPanel : MonoBehaviour
{
    [SerializeField] private GameManager Manager;

    public TextMeshProUGUI question, title;

    private void Start()
    {
        Manager.OnCheckAnswer.AddListener(UpdateQuestionPanel);
        UpdateQuestionPanel();
    }

    private void UpdateQuestionPanel()
    {
        if (Manager.currentQuestionAnsweredIncorrectly)
        {
            title.text = Manager.currentQuestion.name + "  INCORRECT: PLEASE TRY AGAIN";
            question.text = Manager.currentQuestion.prompt;
        }
        else
        {
            title.text = Manager.currentQuestion.name;
            question.text = Manager.currentQuestion.prompt;
        }
    }

}
