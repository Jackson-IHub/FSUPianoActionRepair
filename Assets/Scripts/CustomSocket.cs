using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Put this on the screw (or any socket) together with a TRIGGER collider that defines the snap radius.
/// When a held SnapTool's tip collider overlaps that trigger, a transparent ghost of the tool is shown
/// at the snapped pose. Releasing the grip while the ghost is showing smoothly moves the tool from
/// where it was released into the snapped pose. Grabbing the tool again detaches it (even mid-transition).
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

    [Header("Snap transition")]
    [Tooltip("Seconds it takes the tool to glide from where it was released into the socket. 0 = instant.")]
    [SerializeField, Min(0f)] float snapDuration = 0.2f;
    [Tooltip("Easing over the transition (X = normalized time 0..1, Y = progress 0..1).")]
    [SerializeField] AnimationCurve snapCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Ghost preview")]
    [Tooltip("A transparent material. If empty, a basic fallback tinted with Ghost Color is used.")]
    [SerializeField] Material ghostMaterial;
    [SerializeField] Color ghostColor = new Color(0.3f, 0.8f, 1f, 0.35f);

    [Header("Events")]
    public UnityEvent<SocketTool> toolAttached;
    public UnityEvent<SocketTool> toolDetached;

    readonly List<SocketTool> candidates = new List<SocketTool>();
    SocketTool hovered;   // tool whose ghost is currently shown
    SocketTool attached;  // tool that is snapped (or gliding into place)
    bool snapped;         // true once the transition has finished and the tool is locked to the socket
    bool constraintsSaved;
    RigidbodyConstraints savedConstraints;
    Coroutine attachRoutine;
    GameObject ghostRoot;
    Material runtimeGhostMaterial;

    public Transform SnapPoint => snapPoint != null ? snapPoint : transform;
    public SocketTool AttachedTool => attached;
    /// <summary>True while the tool is still gliding into the socket.</summary>
    public bool IsTransitioning => attached != null && !snapped;

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
        // While transitioning, the coroutine drives the pose instead.
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

        // Mirror the tool's hierarchy (local TRS per node) so every mesh ends up with exactly
        // the same pose relative to the root as the original, without decomposing matrices.
        var map = new Dictionary<Transform, Transform> { { tool.transform, ghostRoot.transform } };

        Transform GetOrCreate(Transform src)
        {
            if (map.TryGetValue(src, out var existing)) return existing;

            var go = new GameObject(src.name);
            go.transform.SetParent(GetOrCreate(src.parent), false);
            go.transform.localPosition = src.localPosition;
            go.transform.localRotation = src.localRotation;
            go.transform.localScale = src.localScale;
            map[src] = go.transform;
            return go.transform;
        }

        foreach (var mf in tool.GhostMeshes)
        {
            if (mf == null || mf.sharedMesh == null) continue;
            if (!mf.transform.IsChildOf(tool.transform)) continue; // must live under the tool root

            var node = GetOrCreate(mf.transform);

            node.gameObject.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            var mr = node.gameObject.AddComponent<MeshRenderer>();
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
        attachRoutine = StartCoroutine(AttachRoutine(tool));
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
            attachRoutine = null;
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
        constraintsSaved = true;
        body.constraints = RigidbodyConstraints.FreezeAll;

        // Subscribe now so grabbing the tool mid-transition cancels the snap.
        tool.Grabbed += OnAttachedToolGrabbed;

        // Glide from the release pose to the socket pose. The target is re-evaluated every frame
        // so the tool still ends up exactly right if the socket (screw) is moving.
        if (snapDuration > 0f)
        {
            Vector3 startPos = tool.transform.position;
            Quaternion startRot = tool.transform.rotation;
            float elapsed = 0f;

            while (elapsed < snapDuration)
            {
                if (attached != tool) { attachRoutine = null; yield break; } // detached mid-glide

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / snapDuration);
                float k = snapCurve != null ? snapCurve.Evaluate(t) : t;

                GetSnapPose(tool, out var targetPos, out var targetRot);
                SetPose(tool, Vector3.LerpUnclamped(startPos, targetPos, k),
                              Quaternion.SlerpUnclamped(startRot, targetRot, k));
                yield return null;
            }

            if (attached != tool) { attachRoutine = null; yield break; }
        }

        ApplyPose(tool);
        snapped = true;
        attachRoutine = null;
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

        if (attachRoutine != null)
        {
            StopCoroutine(attachRoutine);
            attachRoutine = null;
        }

        tool.Grabbed -= OnAttachedToolGrabbed;
        if (constraintsSaved && tool.Body != null) tool.Body.constraints = savedConstraints;
        constraintsSaved = false;
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
        SetPose(tool, pos, rot);
    }

    void SetPose(SocketTool tool, Vector3 pos, Quaternion rot)
    {
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