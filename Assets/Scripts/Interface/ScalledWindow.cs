using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class ScalledWindow : MonoBehaviour, IQueuedWindow
{
    [SerializeField] protected WindowQueue _windowQueue;
    [SerializeField] protected bool _visible;
    [SerializeField] protected RectTransform _window = null;
    [SerializeField] protected GameObject _background = null;
    [SerializeField] protected float _duration = 0f;

    private Coroutine _coroutine = null;

    public UnityAction OnOpened { get; set; }
    public UnityAction OnClosed { get; set; }

    public bool Visible
    {
        get => _visible;
        set
        {
            if(_visible != value)
            {
                _visible = value;
                if (_coroutine != null) StopCoroutine(_coroutine);
                _coroutine = StartCoroutine(scalling());
            }
        }
    }

    private IEnumerator scalling()
    {
        if(_visible) _background.SetActive(true);

        Vector3 start = _window.localScale, end = _visible ? Vector3.one : Vector3.zero;
        float timer = (Mathf.Sqrt(3) - Vector3.Distance(start, end)) * _duration;
        start = _visible ? Vector3.zero : Vector3.one;
        while (timer <= _duration)
        {
            timer += Time.deltaTime;
            _window.localScale = Vector3.Lerp(start, end, timer / _duration);
            yield return null;
        }

        if (!_visible) _background.SetActive(false);
        _coroutine = null;

        if (_visible) OnOpened?.Invoke();
        else OnClosed?.Invoke();
    }

    protected void Init()
    {
        _window.localScale = _visible ? Vector3.one : Vector3.zero;
    }

    public void Request()
    {
        _windowQueue.Add(this);
    }

    public void Open()
    {
        Visible = true;
    }

    public void Close()
    {
        Visible = false;
    }

    public void Next()
    {
        _windowQueue.Close();
    }
}

public interface IQueuedWindow
{
    void Request();
    void Open();
    void Close();
    void Next();
}
