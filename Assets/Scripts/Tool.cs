using UnityEngine;
using UnityEngine.Events;

public class Tool : MonoBehaviour
{
    [SerializeField] private float returnDuration = 0.5f;

    [SerializeField] private Rigidbody rb;
    
    private Vector3 returnPosition;
    private Quaternion returnRotation;

    private bool isGrabbed = false;
    private float returnTimer = 0;

    void Start()
    {
        returnPosition = transform.position;
        returnRotation = transform.rotation;
    }

    void Update()
    {
        returnTimer = Mathf.MoveTowards(returnTimer, returnDuration, Time.deltaTime);
        if (isGrabbed)
        {
            returnTimer = 0;
        }
        else if (returnTimer < returnDuration)
        {
            float t = returnTimer / returnDuration;
            transform.position = Vector3.Lerp(transform.position, returnPosition, t);
            transform.rotation = Quaternion.Lerp(transform.rotation, returnRotation, t);
        }
    }

    public void OnGrabbed()
    {
        isGrabbed = true;
    }

    public void OnReleased()
    {
        isGrabbed = false;
    }
}