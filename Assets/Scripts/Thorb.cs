using UnityEngine;
using System;

[RequireComponent(typeof(CustomSocket))]
public class Thorb : MonoBehaviour
{
    public static event Action<Thorb, SocketTool> OnThorbActivated;

    public bool HasToolInserted { get; private set; }

    private CustomSocket socket;

    private void Awake()
    {
        socket = GetComponent<CustomSocket>();

        socket.toolAttached.AddListener(OnToolAttached);
        socket.toolDetached.AddListener(OnToolDetached);
    }

    private void OnDestroy()
    {
        if (socket == null) return;

        socket.toolAttached.RemoveListener(OnToolAttached);
        socket.toolDetached.RemoveListener(OnToolDetached);
    }

    private void OnToolAttached(SocketTool tool)
    {
        HasToolInserted = true;

        Debug.Log($"{name}: Tool inserted: {tool.name}");

        OnThorbActivated?.Invoke(this, tool);
    }

    private void OnToolDetached(SocketTool tool)
    {
        HasToolInserted = false;

        Debug.Log($"{name}: Tool removed.");
    }
}