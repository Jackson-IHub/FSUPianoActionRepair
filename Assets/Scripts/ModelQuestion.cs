using UnityEngine;

[CreateAssetMenu(fileName = "ModelQuestion", menuName = "Scriptable Objects/Questions/ModelQuestion")]
public class ModelQuestion : Questions
{
    public ToolType.toolType correctTool;
    public int correctArea;
}
