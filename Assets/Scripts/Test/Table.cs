using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Table : MonoBehaviour
{
    //[SerializeField] private bool _alternativeMergeMode = false;
    [SerializeField] private GameObject _unitPrefab = null;
    [Space(20)]
    [SerializeField] private int _minCost = 1;
    [SerializeField] private int _maxCost = 4;

    private int size = 10;
    private int turnCounter = 0;
    private Unit[,] cells;

    public UnityAction<Unit> OnUnitInstantiated { get; set; } = null;
    public UnityAction<Unit> OnUnitScrolled { get; set; } = null;
    public UnityAction<Unit> OnUnitLaunched { get; set; } = null;
    public UnityAction OnGameOver { get; set; } = null;

    public int Size
    {
        get => size;
        set
        {
            if (size != value)
            {
                size = value;
                Init();
            }
        }
    }

    public Unit this[int x, int y] { get => cells[x, y]; set => cells[x, y] = value; }
    public Unit this[Point p] { get => cells[p.X, p.Y]; set => cells[p.X, p.Y] = value; }

    private void Start()
    {
        Init();
        InstantiateUnit();
    }

    private void Init()
    {
        cells = new Unit[size, size];
        for (int i = 0; i < cells.Length; i++) cells[i % size, i / size] = null;
    }

    private IEnumerator Rotate(UnityAction onComplete)
    {
        float start = turnCounter * 90f;
        turnCounter++;
        float end = turnCounter * 90f;

        yield return new WaitForSeconds(0.5f);

        Unit[,] newCells = new Unit[size, size];
        for (int i = 0; i < cells.Length; i++)
        {
            int x = i % size, y = i / size;
            int oldX = y, oldY = size - 1 - x;
            newCells[x, y] = cells[oldX, oldY];
        }
        cells = newCells;

        float timer = 0f, duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            Vector3 angle = transform.eulerAngles;
            angle.z = Mathf.Lerp(start, end, timer / duration);
            transform.eulerAngles = angle;
            yield return null;
        }

        onComplete?.Invoke();
    }

    public Vector3 PointToVector3(Point point)
    {
        return new Vector3(point.X - (size - 1) / 2f, point.Y - (size - 1) / 2f);
    }

    public Point Vector3ToPoint(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x + (size - 1) / 2f);
        int y = Mathf.RoundToInt(pos.y + (size - 1) / 2f);
        return new Point(x, y);
    }

    private void OnDrawGizmos()
    {
        if (cells == null) return;
        for (int i = 0; i < cells.Length; i++)
        {
            int x = i % this.size, y = i / this.size;

            Vector3 center = PointToVector3(new Point(x, y));
            Vector3 size = Vector3.one * 0.95f;
            Color color = Color.white;
            if (this[x, y] != null) color = Color.green;
            color.a = 0.4f;

            Gizmos.color = color;
            Gizmos.DrawCube(center, size);
        }
    }

    #region GameLogic

    private void InstantiateUnit()
    {
        Point point = new Point(Random.Range(2, 8), 1);
        Unit unit = Instantiate(_unitPrefab, transform).GetComponent<Unit>();
        unit.transform.position = PointToVector3(point);
        unit.transform.eulerAngles = Vector3.zero;

        this[point] = unit;
        this[point + Point.down] = unit;

        unit.onEndScrolling += MoveUnit;
        unit.Cost = Random.Range(_minCost, _maxCost + 1);
        unit.Distance = Random.Range(2, 7);

        OnUnitInstantiated?.Invoke(unit);
    }

    private void MoveUnit(Point point)
    {
        Unit unit = this[point];

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

        this[point] = null;
        this[secondPoint] = null;

        secondTarget = target + (secondPoint - point);
        this[target] = unit;
        this[secondTarget] = unit;
        unit.MoveTo(PointToVector3(target), MoveMode.MOVE);
        unit.onEndMove += PushUnit;
    }

    private void PushUnit(Point point)
    {
        Unit unit = this[point];
        unit.onEndMove = null;

        if (unit.Distance == 0)
        {
            Next(point);
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
            else Next(point);
        }
    }

    private bool CheckPushAnotherUnit(Unit unit)
    {
        Point first = GetFirstPoint(unit), second = GetSecondPoint(unit);

        if (Math.Max(first.Y, second.Y) >= 7) return false;

        if (first.X == second.X)
        {
            Point up;
            if (first.Y > second.Y) up = first + Point.up;
            else up = second + Point.up;

            Unit another = this[up];
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

            Unit leftAnother = this[leftPoint], rightAnother = this[rightPoint];

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
        if (Math.Max(first.Y, second.Y) >= 7) return;

        if (first.X == second.X)
        {
            Point up;
            if (first.Y > second.Y) up = first + Point.up;
            else up = second + Point.up;

            Unit another = this[up];
            if (another != null)
                PushAnotherUnit(another);

            this[first] = null;
            this[second] = null;
            this[up] = unit;
            this[up + Point.down] = unit;
            unit.MoveTo(PointToVector3(first + Point.up), MoveMode.PUSH);
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

            Unit leftAnother = this[leftPoint], rightAnother = this[rightPoint];
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

            this[first] = null;
            this[second] = null;
            this[leftPoint] = unit;
            this[rightPoint] = unit;
            unit.MoveTo(PointToVector3(first + Point.up), MoveMode.PUSH);
        }
    }

    private bool MergeUnits(Unit first, Unit second)
    {
        if (first == null || second == null ||
            first.GetInstanceID() == second.GetInstanceID() ||
            first.Cost != second.Cost) return false;

        //if (_alternativeMergeMode)
        //{
        //    Point firstPoint = GetFirstPoint(first),
        //        secondPoint = GetSecondPoint(first);

        //    this[firstPoint] = null;
        //    this[secondPoint] = null;
        //    Destroy(first.gameObject);
        //    first.Cost++;
        //    return true;
        //}
        //else
        //{
        Point firstPoint = GetFirstPoint(second),
            secondPoint = GetSecondPoint(second);

        this[firstPoint] = null;
        this[secondPoint] = null;
        Destroy(second.gameObject);
        first.Cost++;
        return true;
        //}
    }

    private void Next(Point point)
    {
        if (GameOver)
        {
            //TODO
            OnGameOver?.Invoke();
        }
        else
        {
            this[point].onEndMove = null;
            StartCoroutine(Rotate(InstantiateUnit));
        }
    }

    private bool GameOver
    {
        get
        {
            for(int i = 0; i < cells.Length; i++)
            {
                int x = i % size, y = i / size;
                if (this[x, y] != null && (x < 2 || x > 7 || y < 2 || y > 7))
                    return true;
            }
            return false;
        }
    }


    public Point GetFirstPoint(Unit unit)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            int x = i % size, y = i / size;
            Point point = new Point(x, y);
            if (this[point] != null && unit.GetInstanceID() == this[point].GetInstanceID())
            {
                if (Mathf.RoundToInt((unit.transform.position - PointToVector3(point)).magnitude) == 0)
                    return point;
                else return SearchUnitSecondPart(point);
            }
        }
        return null;
    }

    public Point GetSecondPoint(Unit unit)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            int x = i % size, y = i / size;
            Point point = new Point(x, y);
            if (this[point] != null && unit.GetInstanceID() == this[point].GetInstanceID())
            {
                if (Mathf.RoundToInt((unit.transform.position - PointToVector3(point)).magnitude) != 0)
                    return point;
                else return SearchUnitSecondPart(point);
            }
        }
        return null;
    }

    private bool IsEmpty(Point point) => this[point] == null;

    private Point SearchUnitSecondPart(Point first)
    {
        Point max = Point.one * 11;
        Point[] seconds = { Point.Clamp(first + Point.up, Point.zero, max) ,
                            Point.Clamp(first + Point.down, Point.zero, max),
                            Point.Clamp(first + Point.right, Point.zero, max),
                            Point.Clamp(first + Point.left, Point.zero, max)};

        foreach (Point second in seconds)
            if (CheckUnitSecondPart(first, second)) return second;
        return new Point(first);
    }

    private bool CheckUnitSecondPart(Point first, Point second) => first != second && this[first] != null &&
        this[second] != null && this[first].GetInstanceID() == this[second].GetInstanceID();

    public void ScrollUnit(Point oldPoint, Point newPoint)
    {
        Unit unit = this[oldPoint];

        this[oldPoint] = null;
        this[oldPoint + Point.down] = null;

        this[newPoint] = unit;
        this[newPoint + Point.down] = unit;

        unit.MoveTo(PointToVector3(newPoint), MoveMode.SCROLL);

        OnUnitScrolled?.Invoke(unit);
    }

    #endregion GameLogic
}

public class Point
{
    private int x, y;

    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }

    public Point()
    {
        this.x = 0;
        this.y = 0;
    }

    public Point(Point other)
    {
        this.x = other.x;
        this.y = other.y;
    }

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        return obj is Point point &&
               x == point.x &&
               y == point.y;
    }

    public override int GetHashCode()
    {
        var hashCode = 1502939027;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + y.GetHashCode();
        return hashCode;
    }

    public override string ToString() => string.Format("({0} ; (1))", x, y);

    public static Point Clamp(Point p, Point first, Point second)
    {
        int minX = Math.Min(first.x, second.x);
        int maxX = Math.Max(first.x, second.x);
        int minY = Math.Min(first.y, second.y);
        int maxY = Math.Max(first.y, second.y);
        return new Point(Mathf.Clamp(p.x, minX, maxX), Mathf.Clamp(p.y, minY, maxY));
    }

    public static Point operator +(Point p0, Point p1) => new Point(p0.X + p1.X, p0.Y + p1.Y);
    public static Point operator -(Point p0, Point p1) => new Point(p0.X - p1.X, p0.Y - p1.Y);
    public static Point operator -(Point p) => new Point(-p.X, -p.Y);
    public static Point operator *(Point p, int i) => new Point(p.X * i, p.Y * i);
    public static bool operator ==(Point p0, Point p1) => p0.Equals(p1);
    public static bool operator !=(Point p0, Point p1) => !p0.Equals(p1);

    public static readonly Point zero = new Point(0, 0);
    public static readonly Point one = new Point(1, 1);
    public static readonly Point up = new Point(0, 1);
    public static readonly Point down = new Point(0, -1);
    public static readonly Point right = new Point(1, 0);
    public static readonly Point left = new Point(-1, 0);
}