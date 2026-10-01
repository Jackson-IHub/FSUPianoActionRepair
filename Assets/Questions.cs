using UnityEngine;

[CreateAssetMenu(fileName = "Questions", menuName = "Scriptable Objects/Questions")]
public class Questions : ScriptableObject
{
    public int stage;
    public int step;
    public string questionText;
    public string questionTitle;
    public bool complete = false;
    public float correctAnswer;
}
