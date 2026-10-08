using UnityEngine;

[CreateAssetMenu(fileName = "ModelQuestion", menuName = "Scriptable Objects/Questions/ModelQuestion")]
public class ModelQuestion : Questions
{
    public GameObject correctAnswer;

    public bool checkAnswer(GameObject answer)
    {
        return answer == correctAnswer;
    }
}
