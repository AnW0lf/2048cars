using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 30f;
    [SerializeField] private float _moveSpeed = 15f;
    [SerializeField] private float _pushSpeed = 5f;

    public int Cost
    {
        get => _cost;
        set
        {
            _cost = value;
            onCostChanged?.Invoke(_cost);
        }
    }
    public int Distance { get; set; }

    private Coroutine _move = null;
    private int _cost;

    public UnityAction<int> onCostChanged = null;
    public UnityAction<Unit> onClick = null;

    public void MoveTo(Vector3 pos, MoveMode mode)
    {
        float speed = 0f;
        switch (mode)
        {
            case MoveMode.SCROLL:
                speed = _scrollSpeed;
                break;
            case MoveMode.MOVE:
                speed = _moveSpeed;
                break;
            case MoveMode.PUSH:
                speed = _pushSpeed;
                break;
            default:
                break;
        }
        if (_move != null) StopCoroutine(_move);
        _move = StartCoroutine(Move(pos, speed));
    }

    private IEnumerator Move(Vector3 end, float speed)
    {
        Vector3 start = transform.position;
        float timer = 0f, duration = Vector3.Distance(start, end) / speed;

        while (timer <= duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, timer / duration);
            yield return null;
        }

        _move = null;
        onEndMove?.Invoke(TargetTable.Vector3ToPoint(end));
    }

    private Point min;
    private Point max;

    private Point _current;

    private Table _targetTable;
    public Table TargetTable
    {
        get => _targetTable;
        set
        {
            _targetTable = value;
            Init();
        }
    }

    public UnityAction<Point> onEndScrolling = null, onEndMove = null;

    public void Merge()
    {
        MoneyCounter moneyCounter = FindObjectOfType<MoneyCounter>();
        moneyCounter.ChangeCounter(Player.Instance.Money + 5, Camera.main.WorldToScreenPoint(transform.position));
        Destroy(gameObject);
    }

    private void Init()
    {
        int minCoordinate = (TargetTable.TableSize - TargetTable.FieldSize) / 2;
        int maxCoordinate = (TargetTable.TableSize + TargetTable.FieldSize) / 2 - 1;
        min = new Point(minCoordinate, 1);
        max = new Point(maxCoordinate, 1);
    }

    private void OnMouseDown()
    {
        _current = TargetTable.Vector3ToPoint(transform.position);
        _current = Point.Clamp(_current, min, max);
        onClick?.Invoke(this);
    }

    //useless comment

    private void OnMouseDrag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Point point = TargetTable.Vector3ToPoint(worldPosition);
        point = Point.Clamp(point, min, max);

        if (point != _current)
        {
            GameLogic.Instance.ScrollUnit(_current, point);
            _current = point;
        }

    }

    private void OnMouseUp()
    {
        onEndScrolling?.Invoke(_current);
        GetComponent<Collider2D>().enabled = false;

        Player.Instance.Experience++;
    }

    private void OnDisable()
    {
        onEndScrolling = null;
        onEndMove = null;
        onCostChanged = null;
    }

    private void OnDestroy()
    {
        onEndScrolling = null;
        onEndMove = null;
        onCostChanged = null;
    }
}

public enum MoveMode { SCROLL, MOVE, PUSH }