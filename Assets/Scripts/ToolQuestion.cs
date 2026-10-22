using UnityEngine;

[CreateAssetMenu(fileName = "ToolQuestion", menuName = "Scriptable Objects/Questions/ToolQuestion")]
public class ToolQuestion : Questions
{
    public ToolType.toolType correctTool;
    public int correctArea;
}
