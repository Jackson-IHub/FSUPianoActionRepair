using TMPro;
using UnityEngine;

public class QuestionPanel : MonoBehaviour
{
    [SerializeField] private GameManager Manager;

    public TextMeshProUGUI question, title; 

    private void Update()
    {
        //change to event system later
        title.text = Manager.currentQuestion.name;
        question.text = Manager.currentQuestion.prompt;
    }

}
