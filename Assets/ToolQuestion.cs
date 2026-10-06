using UnityEngine;

[CreateAssetMenu(fileName = "ToolQuestion", menuName = "Scriptable Objects/Questions/ToolQuestion")]
public class ToolQuestion : Questions
{
    public object correctAnswer;

    public bool checkAnswer(object answer)
    {
        return answer == correctAnswer;
    }
}
