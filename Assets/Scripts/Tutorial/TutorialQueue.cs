using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TutorialQueue : MonoBehaviour
{
    private Queue<ITutorial> queue = new Queue<ITutorial>();
    private ITutorial current = null;

    public bool IsEmpty => queue.Count == 0;

    private bool CanNext => queue.Count != 0 && current == null;

    public void Add(ITutorial tutorial)
    {
        queue.Enqueue(tutorial);
        if (CanNext) Next();
    }

    public void Next()
    {
        if (IsEmpty) current = null;
        else
        {
            current = queue.Dequeue();
            current.Begin();
        }
    }
}

public interface ITutorial
{
    void Begin();
    void Complete();
}
