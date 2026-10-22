using UnityEngine;

[CreateAssetMenu(fileName = "ToolQuestion", menuName = "Scriptable Objects/Questions/ToolQuestion")]
public class ToolQuestion : Questions
{
    public GameObject correctAnswer;
    public GameObject correctArea;

    public bool checkAnswer(GameObject answer)
    {
        return answer == correctAnswer;
    }
}
