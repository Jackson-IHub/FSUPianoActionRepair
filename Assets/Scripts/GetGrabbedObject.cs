using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GetGrabbedObject : MonoBehaviour
{
    private NearFarInteractor interactor;
    public bool holdingTool = false;
    private IXRSelectInteractable grabbedInteractable { get; set; } = null;
    public GameObject grabbedObject { get; set; } = null;
    void Start()
    {
        // Get the interactor component on this hand controller
        interactor = GetComponentInChildren<NearFarInteractor>();
    }

    void Update()
    {
        // Check if the hand is currently holding anything
        if (interactor.isSelectActive)
        {
            holdingTool = true;
            // Get the first interactable object in the selection list
            grabbedInteractable = interactor.interactablesSelected[0];

            // Get the actual GameObject of the grabbed object
            grabbedObject = grabbedInteractable.transform.gameObject;

            Debug.Log("Currently holding: " + grabbedObject.name);
        }
        else
        {
            holdingTool = false;
            grabbedInteractable = null;
            grabbedObject=null;
        }
    }
}
