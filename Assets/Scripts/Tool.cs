using UnityEngine;

public class Tool : MonoBehaviour
{
    [SerializeField] private float returnDuration = 0.5f;
    [SerializeField] private AnimationCurve returnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField] private Rigidbody rb;

    [SerializeField] private ToolType.toolType toolType;

    private Vector3 returnPosition;
    private Quaternion returnRotation;

    // Pose captured at the moment of release; the return lerps from here.
    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool isReturning = false;
    private float returnTimer = 0f;
    private bool wasKinematic;

    void Start()
    {
        returnPosition = transform.position;
        returnRotation = transform.rotation;
    }

    void Update()
    {
        if (!isReturning)
            return;

        returnTimer += Time.deltaTime;
        float t = returnDuration > 0f ? Mathf.Clamp01(returnTimer / returnDuration) : 1f;
        float eased = returnCurve.Evaluate(t);

        transform.SetPositionAndRotation(
            Vector3.LerpUnclamped(startPosition, returnPosition, eased),
            Quaternion.SlerpUnclamped(startRotation, returnRotation, eased));

        if (t >= 1f)
            FinishReturn();
    }

    public void OnGrabbed()
    {
        if (isReturning)
            FinishReturn(snap: false); // grabbed mid-return: stop and hand control to the hand
    }

    public void OnReleased()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        returnTimer = 0f;
        isReturning = true;

        // Stop physics from fighting the animation while it plays.
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }
    }

    private void FinishReturn(bool snap = true)
    {
        isReturning = false;

        if (snap)
            transform.SetPositionAndRotation(returnPosition, returnRotation);

        if (rb != null)
            rb.isKinematic = wasKinematic;
    }

    public bool IsAtReturnPosition()
    {
        return Vector3.Distance(transform.position, returnPosition) < 0.01f;
    }

    public ToolType.toolType getToolType()
    {
        return toolType;
    }
}