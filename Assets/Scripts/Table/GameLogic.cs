﻿using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;

public class GameLogic : MonoBehaviour
{
    public static GameLogic Instance { get; private set; }

    [SerializeField] private WindowQueue _windowQueue = null;
    [SerializeField] private GameObject _tablePrefab = null;
    [SerializeField] private GameObject _unitPrefab = null;
    [SerializeField] private TableInfo[] _tables = null;

    private Table _table;
    private TableInfo _tableInfo;

    public UnityAction<Unit> OnUnitInstantiated { get; set; } = null;
    public UnityAction<Unit> OnUnitScrolled { get; set; } = null;
    public UnityAction<Unit> OnUnitLaunched { get; set; } = null;
    public UnityAction OnGameOver { get; set; } = null;
    public UnityAction OnGameWin { get; set; } = null;
    public UnityAction<TableInfo> OnTableInstantiated { get; set; } = null;
    public UnityAction<Unit> OnMerge { get; set; } = null;

    public int FieldSize { get => _table.FieldSize; }
    public int TableSize { get => _table.TableSize; }

    public TableInfo GetTableInfo(int id) => _tables[Mathf.Clamp(id, 0, _tables.Length - 1)];

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        InstantiateTable();
    }

    private IEnumerator MoveFromTo(Vector3 from, Vector3 to, float duration, Transform target, UnityAction onComplete)
    {
        float timer = 0f;
        target.position = from;

        while (timer <= duration)
        {
            timer += Time.deltaTime;
            target.position = Vector3.Lerp(from, to, timer / duration);
            yield return null;
        }

        onComplete?.Invoke();
    }

    private void InstantiateTable()
    {
        _tableInfo = _tables[Player.Instance.TableNumber];
        _table = Instantiate(_tablePrefab).GetComponent<Table>();
        _table.FieldSize = _tableInfo.fieldSize;
        _table.Init();

        Vector3 from = Vector3.right * 20f, to = Vector3.zero;
        UnityAction onComplete = InstantiateUnit;

        OnTableInstantiated?.Invoke(_tableInfo);

        StartCoroutine(MoveFromTo(from, to, 0.5f, _table.transform, onComplete));
    }

    private void DestroyTable(Table table)
    {
        Vector3 from = Vector3.zero, to = Vector3.left * 20f;
        UnityAction onComplete = () => { Destroy(table.gameObject); };
        StartCoroutine(MoveFromTo(from, to, 1.5f, table.transform, onComplete));
    }

    private void InstantiateUnit()
    {
        int minX = (_table.TableSize - _table.FieldSize) / 2;
        int maxX = (_table.TableSize + _table.FieldSize) / 2;
        Point point = new Point(Random.Range(minX, maxX), 1);
        Unit unit = Instantiate(_unitPrefab, _table.transform).GetComponent<Unit>();
        unit.TargetTable = _table;
        unit.transform.position = _table.PointToVector3(point) + Vector3.down * 4f;
        unit.transform.eulerAngles = Vector3.zero;
        unit.MoveTo(_table.PointToVector3(point), MoveMode.SCROLL);

        _table[point] = unit;
        _table[point + Point.down] = unit;

        unit.onEndScrolling += MoveUnit;
        unit.Cost = Random.Range(_tableInfo.minCost, _tableInfo.maxCost + 1);
        int minDistance = (_table.TableSize - _table.FieldSize) / 2;
        int maxDistance = _table.TableSize - _table.FieldSize + 1;
        unit.Distance = Random.Range(minDistance, maxDistance);

        OnUnitInstantiated?.Invoke(unit);
    }

    private void MoveUnit(Point point)
    {
        Unit unit = _table[point];

        OnUnitLaunched?.Invoke(unit);

        unit.onEndMove = null;
        Point secondPoint = SearchUnitSecondPart(point);
        Point target = new Point(point);
        Point secondTarget;

        while (unit.Distance > 0)
        {
            if (IsEmpty(target + Point.up))
            {
                target += Point.up;
                unit.Distance--;
            }
            else break;
        }

        if (target == point)
        {
            PushUnit(point);
            return;
        }

        _table[point] = null;
        _table[secondPoint] = null;

        secondTarget = target + (secondPoint - point);
        _table[target] = unit;
        _table[secondTarget] = unit;
        unit.MoveTo(_table.PointToVector3(target), MoveMode.MOVE);
        unit.onEndMove += PushUnit;
    }

    private void PushUnit(Point point)
    {
        Unit unit = _table[point];
        unit.onEndMove = null;

        if (unit.Distance == 0)
        {
            NextStep(point);
        }
        else
        {
            Point first = GetFirstPoint(unit), second = GetSecondPoint(unit);
            Point up = first + Point.up;
            if (CheckPushAnotherUnit(unit))
            {
                PushAnotherUnit(unit);
                unit.Distance--;
                unit.onEndMove += PushUnit;
            }
            else NextStep(point);
        }
    }

    private bool CheckPushAnotherUnit(Unit unit)
    {
        Point first = GetFirstPoint(unit), second = GetSecondPoint(unit);

        if (Math.Max(first.Y, second.Y) >= (_table.TableSize + _table.FieldSize) / 2 - 1) return false;

        if (first.X == second.X)
        {
            Point up;
            if (first.Y > second.Y) up = first + Point.up;
            else up = second + Point.up;

            Unit another = _table[up];
            if (MergeUnits(unit, another)) return true;
            else
            {
                if (another == null) return true;
                else return CheckPushAnotherUnit(another);
            }
        }
        else
        {
            Point leftPoint, rightPoint;
            if (first.X < first.X)
            {
                leftPoint = first + Point.up;
                rightPoint = second + Point.up;
            }
            else
            {
                leftPoint = second + Point.up;
                rightPoint = first + Point.up;
            }

            Unit leftAnother = _table[leftPoint], rightAnother = _table[rightPoint];

            if (leftAnother == null && rightAnother == null) return true;
            else if (leftAnother == null)
            {
                if (MergeUnits(unit, rightAnother)) return true;
                else return CheckPushAnotherUnit(rightAnother);
            }
            else if (rightAnother == null)
            {
                if (MergeUnits(unit, leftAnother)) return true;
                else return CheckPushAnotherUnit(leftAnother);
            }
            else if (leftAnother.GetInstanceID() != rightAnother.GetInstanceID())
            {
                bool leftMerged = MergeUnits(unit, leftAnother);
                bool rightMerged = MergeUnits(unit, rightAnother);
                if (!leftMerged) leftMerged = MergeUnits(unit, leftAnother);

                if (leftMerged && rightMerged) return true;
                else if (leftMerged) return CheckPushAnotherUnit(rightAnother);
                else if (rightMerged) return CheckPushAnotherUnit(leftAnother);
                else return CheckPushAnotherUnit(leftAnother) && CheckPushAnotherUnit(rightAnother);
            }
            else
            {
                if (MergeUnits(unit, leftAnother)) return true;
                else return CheckPushAnotherUnit(leftAnother);
            }
        }
    }

    private void PushAnotherUnit(Unit unit)
    {
        Point first = GetFirstPoint(unit), second = GetSecondPoint(unit);
        if (first is null || second is null) return;
        if (Math.Max(first.Y, second.Y) >= (_table.TableSize + _table.FieldSize) / 2) return;

        if (first.X == second.X)
        {
            Point up;
            if (first.Y > second.Y) up = first + Point.up;
            else up = second + Point.up;

            Unit another = _table[up];
            if (another != null)
                PushAnotherUnit(another);

            _table[first] = null;
            _table[second] = null;
            _table[up] = unit;
            _table[up + Point.down] = unit;
            unit.MoveTo(_table.PointToVector3(first + Point.up), MoveMode.PUSH);
        }
        else
        {
            Point leftPoint, rightPoint;
            if (first.X < first.X)
            {
                leftPoint = first + Point.up;
                rightPoint = second + Point.up;
            }
            else
            {
                leftPoint = second + Point.up;
                rightPoint = first + Point.up;
            }

            Unit leftAnother = _table[leftPoint], rightAnother = _table[rightPoint];
            if (leftAnother != null || rightAnother != null)
            {
                if (leftAnother == null)
                    PushAnotherUnit(rightAnother);
                else if (rightAnother == null)
                    PushAnotherUnit(leftAnother);
                else if (leftAnother.GetInstanceID() != rightAnother.GetInstanceID())
                {
                    PushAnotherUnit(leftAnother);
                    PushAnotherUnit(rightAnother);
                }
                else
                {
                    PushAnotherUnit(leftAnother);
                }
            }

            _table[first] = null;
            _table[second] = null;
            _table[leftPoint] = unit;
            _table[rightPoint] = unit;
            unit.MoveTo(_table.PointToVector3(first + Point.up), MoveMode.PUSH);
        }
    }

    private bool MergeUnits(Unit first, Unit second)
    {
        if (first == null || second == null ||
            first.GetInstanceID() == second.GetInstanceID() ||
            first.Cost != second.Cost) return false;

        Point firstPoint = GetFirstPoint(second),
            secondPoint = GetSecondPoint(second);

        _table[firstPoint] = null;
        _table[secondPoint] = null;
        second.Merge();
        first.Cost++;

        OnMerge?.Invoke(first);

        return true;
    }

    private void NextStep(Point point)
    {
        _table[point].onEndMove = null;

        if (GameOver)
        {
            OnGameOver?.Invoke();
            return;
        }

        Player.Instance.Experience++;

        StartCoroutine(WaitWhile(
            () => !_windowQueue.IsEmpty,
            () =>
            {
                if (GameWin) OnGameWin?.Invoke();
                else StartCoroutine(_table.Rotate(InstantiateUnit));
            }));
    }

    private IEnumerator WaitWhile(Func<bool> predicate, UnityAction onComplete)
    {
        WaitWhile wait = new WaitWhile(predicate);
        yield return wait;
        onComplete?.Invoke();
    }

    public void NextTable()
    {
        Player.Instance.TableNumber = (Player.Instance.TableNumber + 1) % _tables.Length;
        ReloadTable();
    }

    public void ReloadTable()
    {
        DestroyTable(_table);
        InstantiateTable();
    }

    private bool GameOver
    {
        get
        {
            int min = (_table.TableSize - _table.FieldSize) / 2;
            int max = (_table.TableSize + _table.FieldSize) / 2;
            for (int i = 0; i < _table.Length; i++)
            {
                int x = i % _table.TableSize, y = i / _table.TableSize;
                if (_table[x, y] != null && (x < min || x >= max || y < min || y >= max))
                    return true;
            }
            return false;
        }
    }

    private bool GameWin
    {
        get
        {
            for (int i = 0; i < _table.Length; i++)
            {
                int x = i % _table.TableSize, y = i / _table.TableSize;
                Unit unit = _table[x, y];
                if (unit != null &&
                    unit.Cost >= _tableInfo.winCostMin &&
                    unit.Cost >= _tableInfo.winCostMiddle &&
                    unit.Cost >= _tableInfo.winCostMax)
                    return true;
            }
            return false;
        }
    }

    public Point GetFirstPoint(Unit unit)
    {
        for (int i = 0; i < _table.Length; i++)
        {
            int x = i % _table.TableSize, y = i / _table.TableSize;
            Point point = new Point(x, y);
            if (_table[point] != null && unit.GetInstanceID() == _table[point].GetInstanceID())
            {
                if (Mathf.RoundToInt((unit.transform.position - _table.PointToVector3(point)).magnitude) == 0)
                    return point;
                else return SearchUnitSecondPart(point);
            }
        }
        return null;
    }

    public Point GetSecondPoint(Unit unit)
    {
        for (int i = 0; i < _table.Length; i++)
        {
            int x = i % _table.TableSize, y = i / _table.TableSize;
            Point point = new Point(x, y);
            if (_table[point] != null && unit.GetInstanceID() == _table[point].GetInstanceID())
            {
                if (Mathf.RoundToInt((unit.transform.position - _table.PointToVector3(point)).magnitude) != 0)
                    return point;
                else return SearchUnitSecondPart(point);
            }
        }
        return null;
    }

    private bool IsEmpty(Point point) => _table[point] == null;

    private Point SearchUnitSecondPart(Point first)
    {
        Point max = Point.one * (_table.TableSize - 1);
        Point[] seconds = { Point.Clamp(first + Point.up, Point.zero, max) ,
                            Point.Clamp(first + Point.down, Point.zero, max),
                            Point.Clamp(first + Point.right, Point.zero, max),
                            Point.Clamp(first + Point.left, Point.zero, max)};

        foreach (Point second in seconds)
            if (CheckUnitSecondPart(first, second)) return second;
        return new Point(first);
    }

    private bool CheckUnitSecondPart(Point first, Point second) => first != second && _table[first] != null &&
        _table[second] != null && _table[first].GetInstanceID() == _table[second].GetInstanceID();

    public void ScrollUnit(Point oldPoint, Point newPoint)
    {
        Unit unit = _table[oldPoint];

        _table[oldPoint] = null;
        _table[oldPoint + Point.down] = null;

        _table[newPoint] = unit;
        _table[newPoint + Point.down] = unit;

        unit.MoveTo(_table.PointToVector3(newPoint), MoveMode.SCROLL);

        OnUnitScrolled?.Invoke(unit);
    }
}
