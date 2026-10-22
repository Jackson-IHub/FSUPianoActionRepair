using UnityEngine;
using System;

[RequireComponent(typeof(CustomSocket))]
public class Thorb : MonoBehaviour
{
    public static event Action<Thorb, SocketTool> OnToolInsertedIntoThorb;

    [SerializeField] private int id;

    private CustomSocket socket;

    public int ID => id;

    public bool HasToolInserted =>
        socket != null && socket.AttachedTool != null;

    public SocketTool InsertedTool =>
        socket != null ? socket.AttachedTool : null;

    private void Awake()
    {
        socket = GetComponent<CustomSocket>();
        socket.toolAttached.AddListener(OnToolAttached);
    }

    private void OnDestroy()
    {
        if (socket != null)
            socket.toolAttached.RemoveListener(OnToolAttached);
    }

    private void OnToolAttached(SocketTool tool)
    {
        Debug.Log($"{name}: Tool inserted: {tool.name}");
        OnToolInsertedIntoThorb?.Invoke(this, tool);
    }
}