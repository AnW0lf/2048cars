using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class DropDownWindow : MonoBehaviour, IQueuedWindow
{
    [SerializeField] protected WindowQueue _windowQueue;
    [SerializeField] protected bool _visible;
    [SerializeField] protected RectTransform _window = null;
    [SerializeField] protected GameObject _background = null;
    [SerializeField] protected float _duration = 0.5f;

    private Coroutine _coroutine = null;
    public bool Visible
    {
        get => _visible;
        set
        {
            if(_visible != value)
            {
                _visible = value;
                if (_coroutine != null) StopCoroutine(_coroutine);
                _coroutine = StartCoroutine(Move());

                _background.SetActive(_visible);
            }
        }
    }

    private IEnumerator Move()
    {
        float start = _window.anchoredPosition.y, end = _visible ? 0f : Screen.height;
        float timer = (1f - Mathf.Abs(start - end) / Screen.height) * _duration;
        while (timer <= _duration)
        {
            timer += Time.deltaTime;
            Vector2 pos = _window.anchoredPosition;
            pos.y = Mathf.Lerp(start, end, timer / _duration);
            _window.anchoredPosition = pos;
            yield return null;
        }
        _coroutine = null;
    }

    protected void Init()
    {
        Vector2 pos = _window.anchoredPosition;
        pos.y = _visible ? 0f : Screen.height;
        _window.anchoredPosition = pos;
    }

    public void Request()
    {
        _windowQueue.Add(this);
    }

    public void Open()
    {
        Visible = true;
    }

    public void Next()
    {
        Visible = false;
        _windowQueue.Close();
    }
}

public interface IQueuedWindow
{
    void Request();
    void Open();
    void Next();
}
