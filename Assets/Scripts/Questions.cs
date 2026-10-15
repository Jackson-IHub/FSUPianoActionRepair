using UnityEngine;

[CreateAssetMenu(fileName = "Questions", menuName = "Scriptable Objects/Questions")]
public abstract class Questions : ScriptableObject
{
    public int phase;
    public int step;
    public string prompt;
    public QuestionTypes.questionTypes type;
}
