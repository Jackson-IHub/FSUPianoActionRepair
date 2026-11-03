using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

[CreateAssetMenu(fileName = "MeasurementQuestion", menuName = "Scriptable Objects/Questions/MeasurementQuestion")]
public class MeasurementQuestion : Questions
{
    public float correctAnswer;
    public float minimumBounds;
    public float maximumBounds;
    public float sliderIncrement;
}
