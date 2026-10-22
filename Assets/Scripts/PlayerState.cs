using UnityEngine;

public class PlayerState : MonoBehaviour
{
    private int heldToolCount = 0;

    public bool IsHoldingTool()
    {
        return heldToolCount > 0;
    }

    public void IncrementHeldToolCount()
    {
        heldToolCount++;
    }

    public void DecrementHeldToolCount()
    {
        heldToolCount = Mathf.Max(0, heldToolCount - 1);
    }
}
