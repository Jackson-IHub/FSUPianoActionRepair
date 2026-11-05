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
            title.text = Manager.currentQuestion.name;
            question.text = "  INCORRECT: PLEASE TRY AGAIN" + Manager.currentQuestion.prompt + "  INCORRECT: PLEASE TRY AGAIN";
        }
        else
        {
            title.text = Manager.currentQuestion.name;
            question.text = Manager.currentQuestion.prompt;
        }
    }

}
