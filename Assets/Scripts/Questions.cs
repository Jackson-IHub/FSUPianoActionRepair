using UnityEngine;

[CreateAssetMenu(fileName = "Questions", menuName = "Scriptable Objects/Questions")]
public abstract class Questions : ScriptableObject
{
    public int phase;
    public int step;
    [TextArea(3,10)] 
    public string prompt;
    public QuestionTypes.questionTypes type;
}
