using UnityEngine;
using System.Collections;
using System;

public class ScrollTutorial : MonoBehaviour, ITutorial
{
    [SerializeField] private TutorialQueue _tutorialQueue = null;
    [SerializeField] private Transform _hand = null;
    [SerializeField] private Vector2 _offset = Vector2.zero;
    [SerializeField] private float _speed = 0f;
    private int _tableSize = 0;
    private Vector3 _startPosition = Vector3.zero;
    private readonly string _key = "ScrollTutorial";

    private bool IsComplete
    {
        get
        {
            if (!PlayerPrefs.HasKey(_key))
                PlayerPrefs.SetInt(_key, 0);
            return PlayerPrefs.GetInt(_key) != 0;
        }
        set
        {
            PlayerPrefs.SetInt(_key, value ? 1 : 0);
        }
    }

    private void Start()
    {
        if (IsComplete) Destroy(gameObject);
        else
        {
            _tutorialQueue.Add(this);
            GameLogic.Instance.OnTableInstantiated += SetTable;
            GameLogic.Instance.OnUnitInstantiated += SetStartPosition;
        }
    }

    private void SetTable(TableInfo tableInfo) => _tableSize = tableInfo.fieldSize + 8;

    private void SetStartPosition(Unit unit)
    {
        Point point = GameLogic.Instance.GetFirstPoint(unit);
        _startPosition = new Vector3(point.X - (_tableSize - 1) / 2f, point.Y - (_tableSize - 1) / 2f - 1f);
        unit.onClick += (u) => Complete();
    }

    private void OnDestroy()
    {
        GameLogic.Instance.OnTableInstantiated -= SetTable;
        GameLogic.Instance.OnUnitInstantiated -= SetStartPosition;
    }

    public void Begin()
    {
        StartCoroutine(MoveHand());
    }

    private IEnumerator MoveHand()
    {
        yield return new WaitWhile(() => _startPosition == Vector3.zero);
        _hand.position = _startPosition;
        while (true)
        {
            float start = _hand.localPosition.x;
            float end = _offset.y;
            float timer = 0f;
            float duration = Mathf.Abs(end - start) / _speed;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                Vector3 pos = _hand.localPosition;
                pos.x = Mathf.Lerp(start, end, timer / duration);
                _hand.localPosition = pos;
                yield return null;
            }

            start = _hand.localPosition.x;
            end = _offset.x;
            timer = 0f;
            duration = Mathf.Abs(end - start) / _speed;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                Vector3 pos = _hand.localPosition;
                pos.x = Mathf.Lerp(start, end, timer / duration);
                _hand.localPosition = pos;
                yield return null;
            }
        }
    }

    public void Complete()
    {
        IsComplete = true;
        _tutorialQueue.Next();
        Destroy(gameObject);
    }
}
