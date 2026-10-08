using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Put this on the screw (or any socket) together with a TRIGGER collider that defines the snap radius.
/// When a held SnapTool's tip collider overlaps that trigger, a transparent ghost of the tool is shown
/// at the snapped pose. Releasing the grip while the ghost is showing snaps the tool into place.
/// Grabbing the tool again detaches it.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CustomSocket : MonoBehaviour
{
    [Header("Snap")]
    [Tooltip("Where the tool's tip ends up. Defaults to this transform. Its Z axis should point the same way as the tool's tip Z axis.")]
    [SerializeField] Transform snapPoint;
    [Tooltip("Empty = accept any tool. Otherwise must match SnapTool.toolType.")]
    [SerializeField] string acceptedToolType = "";
    [Tooltip("If off, only the position is snapped and the tool keeps its rotation.")]
    [SerializeField] bool alignRotation = true;

    [Header("Ghost preview")]
    [Tooltip("A transparent material. If empty, a basic fallback tinted with Ghost Color is used.")]
    [SerializeField] Material ghostMaterial;
    [SerializeField] Color ghostColor = new Color(0.3f, 0.8f, 1f, 0.35f);

    [Header("Events")]
    public UnityEvent<SocketTool> toolAttached;
    public UnityEvent<SocketTool> toolDetached;

    readonly List<SocketTool> candidates = new List<SocketTool>();
    SocketTool hovered;   // tool whose ghost is currently shown
    SocketTool attached;  // tool that is snapped (or about to be)
    bool snapped;       // true once the attach pose has actually been applied
    RigidbodyConstraints savedConstraints;
    GameObject ghostRoot;
    Material runtimeGhostMaterial;

    public Transform SnapPoint => snapPoint != null ? snapPoint : transform;
    public SocketTool AttachedTool => attached;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Awake()
    {
        if (!GetComponent<Collider>().isTrigger)
            Debug.LogWarning($"{name}: TipSocket's collider should be a trigger.", this);
    }

    void OnDisable()
    {
        SetHover(null);
        Detach();
        candidates.Clear();
    }

    void OnDestroy()
    {
        if (runtimeGhostMaterial != null) Destroy(runtimeGhostMaterial);
    }

    // ---------------------------------------------------------------- detection

    bool Accepts(SocketTool tool) =>
        string.IsNullOrEmpty(acceptedToolType) || tool.ToolType == acceptedToolType;

    void OnTriggerEnter(Collider other)
    {
        var tool = other.GetComponentInParent<SocketTool>();
        // Only the tool's tip collider counts, not its handle or blade.
        if (tool == null || tool.TipCollider != other || !Accepts(tool)) return;
        if (!candidates.Contains(tool)) candidates.Add(tool);
    }

    void OnTriggerExit(Collider other)
    {
        var tool = other.GetComponentInParent<SocketTool>();
        if (tool == null || tool.TipCollider != other) return;
        candidates.Remove(tool);
        if (hovered == tool) SetHover(null);
    }

    void Update()
    {
        candidates.RemoveAll(t => t == null);

        if (attached != null) return; // socket is occupied

        // Pick the closest held tool whose tip overlaps the socket.
        SocketTool best = null;
        float bestDist = float.MaxValue;
        foreach (var t in candidates)
        {
            if (!t.IsHeld || t.CurrentSocket != null) continue;
            float d = (t.SnapPoint.position - SnapPoint.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = t; }
        }

        SetHover(best);

        if (hovered != null && ghostRoot != null)
        {
            GetSnapPose(hovered, out var pos, out var rot);
            ghostRoot.transform.SetPositionAndRotation(pos, rot);
        }
    }

    void LateUpdate()
    {
        // Keep the tool locked to the socket (in case the screw itself moves).
        if (snapped && attached != null) ApplyPose(attached);
    }

    // ---------------------------------------------------------------- hover / ghost

    void SetHover(SocketTool tool)
    {
        if (tool == hovered) return;

        if (hovered != null) hovered.Released -= OnToolReleased;
        hovered = tool;
        DestroyGhost();

        if (hovered != null)
        {
            hovered.Released += OnToolReleased;
            BuildGhost(hovered);
        }
    }

    void BuildGhost(SocketTool tool)
    {
        var mat = GetGhostMaterial();
        ghostRoot = new GameObject($"{tool.name}_SnapGhost");
        ghostRoot.transform.localScale = tool.transform.lossyScale;

        foreach (var mf in tool.GhostMeshes)
        {
            if (mf == null || mf.sharedMesh == null) continue;

            var child = new GameObject(mf.name);
            child.transform.SetParent(ghostRoot.transform, false);

            // Pose of this mesh relative to the tool root, so the ghost matches the tool's shape.
            Matrix4x4 rel = tool.transform.worldToLocalMatrix * mf.transform.localToWorldMatrix;
            child.transform.localPosition = rel.GetColumn(3);
            child.transform.localRotation = rel.rotation;
            child.transform.localScale = rel.lossyScale;

            child.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            var mr = child.AddComponent<MeshRenderer>();
            var mats = new Material[mf.sharedMesh.subMeshCount];
            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            mr.sharedMaterials = mats;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        GetSnapPose(tool, out var pos, out var rot);
        ghostRoot.transform.SetPositionAndRotation(pos, rot);
    }

    void DestroyGhost()
    {
        if (ghostRoot != null) Destroy(ghostRoot);
        ghostRoot = null;
    }

    Material GetGhostMaterial()
    {
        if (ghostMaterial != null) return ghostMaterial;
        if (runtimeGhostMaterial == null)
        {
            // Fallback only. For best results assign a proper transparent URP/HDRP/Built-in material.
            var shader = Shader.Find("Sprites/Default");
            runtimeGhostMaterial = new Material(shader) { color = ghostColor };
        }
        return runtimeGhostMaterial;
    }

    // ---------------------------------------------------------------- attach / detach

    void OnToolReleased(SocketTool tool)
    {
        if (tool != hovered) return;
        SetHover(null);
        if (tool.CurrentSocket != null) return; // another socket claimed it first

        attached = tool;
        tool.CurrentSocket = this;
        StartCoroutine(AttachRoutine(tool));
    }

    IEnumerator AttachRoutine(SocketTool tool)
    {
        // Wait a frame so XRGrabInteractable finishes its own release handling
        // (restoring kinematic state, applying throw velocity) before we override it.
        yield return null;

        if (tool == null || tool.IsHeld)
        {
            // Player re-grabbed it before we finished.
            if (attached == tool) attached = null;
            if (tool != null && tool.CurrentSocket == this) tool.CurrentSocket = null;
            yield break;
        }

        var body = tool.Body;
        if (!body.isKinematic)
        {
            body.linearVelocity = Vector3.zero; // Unity 6. On older versions use body.velocity.
            body.angularVelocity = Vector3.zero;
        }

        // Freeze rather than parent/set kinematic: XRGrabInteractable remembers kinematic state and
        // the original parent when grabbed, and would restore the wrong ones when released again.
        savedConstraints = body.constraints;
        body.constraints = RigidbodyConstraints.FreezeAll;

        ApplyPose(tool);
        snapped = true;
        tool.Grabbed += OnAttachedToolGrabbed;
        toolAttached.Invoke(tool);
    }

    void OnAttachedToolGrabbed(SocketTool tool)
    {
        if (tool == attached) Detach();
    }

    void Detach()
    {
        var tool = attached;
        if (tool == null) return;

        tool.Grabbed -= OnAttachedToolGrabbed;
        if (snapped && tool.Body != null) tool.Body.constraints = savedConstraints;
        if (tool.CurrentSocket == this) tool.CurrentSocket = null;

        attached = null;
        bool wasSnapped = snapped;
        snapped = false;
        if (wasSnapped) toolDetached.Invoke(tool);
    }

    // ---------------------------------------------------------------- pose math

    /// <summary>Tool root pose such that tool.SnapPoint coincides with this socket's snap point.</summary>
    void GetSnapPose(SocketTool tool, out Vector3 pos, out Quaternion rot)
    {
        Transform root = tool.transform;
        Transform tip = tool.SnapPoint;

        Quaternion tipLocalRot = Quaternion.Inverse(root.rotation) * tip.rotation;
        Vector3 tipLocalPos = Vector3.Scale(root.InverseTransformPoint(tip.position), root.lossyScale);

        rot = alignRotation ? SnapPoint.rotation * Quaternion.Inverse(tipLocalRot) : root.rotation;
        pos = SnapPoint.position - rot * tipLocalPos;
    }

    void ApplyPose(SocketTool tool)
    {
        GetSnapPose(tool, out var pos, out var rot);
        tool.transform.SetPositionAndRotation(pos, rot);
        tool.Body.position = pos;
        tool.Body.rotation = rot;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(SnapPoint.position, 0.005f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(SnapPoint.position, SnapPoint.forward * 0.05f);
    }
}