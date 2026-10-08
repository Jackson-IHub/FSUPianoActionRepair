using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRI 3.x. For XRI 2.x delete this line.

/// <summary>
/// Put this on the tool (the same GameObject as its XRGrabInteractable + Rigidbody).
/// It holds the "tip" trigger collider and the transform that gets aligned to a TipSocket,
/// and re-broadcasts grab/release events so sockets don't need to touch XRI directly.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class SocketTool : MonoBehaviour
{
    [Header("Tip")]
    [Tooltip("Trigger collider at the tip of the tool. The socket detects this collider.")]
    [SerializeField] Collider tipCollider;

    [Tooltip("Transform at the tip. When snapped, this transform's position AND rotation " +
             "will match the socket's snap point. Point its blue (Z) axis along the shaft, out of the tip.")]
    [SerializeField] Transform snapPoint;

    [Header("Filtering")]
    [Tooltip("Only sockets whose 'Accepted Tool Type' is empty or equal to this will accept the tool.")]
    [SerializeField] string toolType = "";

    [Header("Ghost preview")]
    [Tooltip("Meshes copied to build the transparent preview. Leave empty to use every MeshFilter under this tool.")]
    [SerializeField] MeshFilter[] ghostMeshes;

    public Collider TipCollider => tipCollider;
    public Transform SnapPoint => snapPoint != null ? snapPoint : (tipCollider != null ? tipCollider.transform : transform);
    public string ToolType => toolType;
    public MeshFilter[] GhostMeshes => ghostMeshes;
    public Rigidbody Body { get; private set; }
    public XRGrabInteractable Interactable { get; private set; }
    public bool IsHeld => Interactable != null && Interactable.isSelected;

    /// <summary>The socket this tool is currently snapped into (null if none).</summary>
    public CustomSocket CurrentSocket { get; set; }

    public event Action<SocketTool> Grabbed;
    public event Action<SocketTool> Released;

    void Awake()
    {
        Interactable = GetComponent<XRGrabInteractable>();
        Body = GetComponent<Rigidbody>();

        if (tipCollider == null)
            Debug.LogError($"{name}: SnapTool has no Tip Collider assigned.", this);
        else if (!tipCollider.isTrigger)
            Debug.LogWarning($"{name}: Tip Collider should be a trigger so it doesn't push the screw around.", this);

        if (ghostMeshes == null || ghostMeshes.Length == 0)
            ghostMeshes = GetComponentsInChildren<MeshFilter>();
    }

    void OnEnable()
    {
        Interactable.selectEntered.AddListener(OnSelectEntered);
        Interactable.selectExited.AddListener(OnSelectExited);
    }

    void OnDisable()
    {
        Interactable.selectEntered.RemoveListener(OnSelectEntered);
        Interactable.selectExited.RemoveListener(OnSelectExited);
    }

    void OnSelectEntered(SelectEnterEventArgs args) => Grabbed?.Invoke(this);
    void OnSelectExited(SelectExitEventArgs args) => Released?.Invoke(this);
}