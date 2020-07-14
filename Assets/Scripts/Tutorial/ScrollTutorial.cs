using UnityEngine;
using System.Collections;
using System;

public class ScrollTutorial : MonoBehaviour, ITutorial
{
    [SerializeField] private GameObject _tutorialLabel = null;
    [SerializeField] private GameObject _tableGoals = null;
    [SerializeField] private TutorialQueue _tutorialQueue = null;
    [SerializeField] private Transform _hand = null;
    [SerializeField] private Vector2 _offset = Vector2.zero;
    [SerializeField] private float _speed = 0f;
    [SerializeField] private GameObject[] _destroyable = null;
    private int _tableSize = 0;
    private Vector3 _startPosition = Vector3.zero;
    private readonly string _key = "ScrollTutorial";
    private Coroutine _move = null;

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

    private void OnEnable()
    {
        if (IsComplete)
        {
            foreach (var d in _destroyable) Destroy(d);
            Destroy(gameObject);
        }
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
        GameLogic.Instance.OnUnitInstantiated -= ContinueDestroying;
    }

    public void Begin()
    {
        _tableGoals.transform.localScale = Vector3.zero;
        _move = StartCoroutine(MoveHand());
    }

    private IEnumerator MoveHand()
    {
        yield return new WaitWhile(() => _startPosition == Vector3.zero);
        yield return new WaitForSeconds(0.5f);

        Vector3 startPos = _hand.position;
        float timer = 0f;
        float duration = 0.4f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            _hand.localPosition = Vector3.Lerp(startPos, _startPosition, timer / duration);
            yield return null;
        }

        _hand.position = _startPosition;
        while (true)
        {
            float start = _hand.localPosition.x;
            float end = _offset.y;
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
        StopCoroutine(_move);

        GameLogic.Instance.OnTableInstantiated -= SetTable;
        GameLogic.Instance.OnUnitInstantiated -= SetStartPosition;

        StartCoroutine(Completing());
    }

    private bool _continueDestroying = false;

    private IEnumerator Completing()
    {
        foreach (var d in _destroyable) Destroy(d);

        GameLogic.Instance.OnUnitInstantiated += ContinueDestroying;

        yield return new WaitUntil(() => _continueDestroying);

        GameLogic.Instance.OnUnitInstantiated -= ContinueDestroying;

        float timer = 0f;
        float duration = 0.5f;
        while(timer < duration)
        {
            timer += Time.deltaTime;
            _tableGoals.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer / duration);
            yield return null;
        }
        
        Destroy(gameObject);
    }

    private void ContinueDestroying(Unit unit) => _continueDestroying = true;
}
