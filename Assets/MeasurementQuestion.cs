using UnityEngine;

[CreateAssetMenu(fileName = "MeasurementQuestion", menuName = "Scriptable Objects/Questions/MeasurementQuestion")]
public class MeasurementQuestion : Questions
{
    public float correctAnswer;

    public bool checkAnswer(float answer)
    {
        return answer == correctAnswer;
    }
}
