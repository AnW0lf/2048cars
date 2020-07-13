using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WindowQueue : MonoBehaviour, IWindowQueue
{
    [SerializeField] private BoxCollider2D _blockRaycast = null;
    private Queue<IQueuedWindow> windows = new Queue<IQueuedWindow>();
    private IQueuedWindow current = null;

    public bool IsEmpty => windows.Count == 0 && current == null;

    public void Add(IQueuedWindow window)
    {
        windows.Enqueue(window);
        Next();
    }

    public void Open()
    {
        _blockRaycast.enabled = true;
        current = windows.Dequeue();
        current.Open();
    }

    public void Close()
    {
        _blockRaycast.enabled = false;
        current = null;
        Next();
    }

    public void Next()
    {
        if (current == null && windows.Count != 0)
            Open();
    }
}

public interface IWindowQueue
{
    void Add(IQueuedWindow window);
    void Open();
    void Close();
    void Next();
}
