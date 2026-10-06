using UnityEngine;

[CreateAssetMenu(fileName = "ModelQuestion", menuName = "Scriptable Objects/Questions/ModelQuestion")]
public class ModelQuestion : Questions
{
    public object correctAnswer;

    public bool checkAnswer(object answer)
    {
        return answer == correctAnswer;
    }
}
