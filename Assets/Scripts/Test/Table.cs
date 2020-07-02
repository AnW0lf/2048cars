using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Table : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab = null;

    private int size = 10;
    private int turnCounter = 0;
    private Unit[,] cells;

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

    private void Rotate()
    {
        turnCounter++;
        transform.localEulerAngles = Vector3.forward * turnCounter * 90f;

        Unit[,] newCells = new Unit[size, size];
        for(int i = 0; i < cells.Length; i++)
        {
            int x = i % size, y = i / size;
            int oldX = y, oldY = size - 1 - x;
            newCells[x, y] = cells[oldX, oldY];
        }
        cells = newCells;
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
        Unit unit = Instantiate(unitPrefab, transform).GetComponent<Unit>();
        unit.transform.position = PointToVector3(point);
        unit.transform.eulerAngles = Vector3.zero;

        this[point] = unit;
        this[point + Point.down] = unit;

        unit.onEndScrolling += MoveUnit;
        unit.Cost = Random.Range(1, 6);
        unit.Distance = Random.Range(2, 7);
    }

    private void MoveUnit(Point point)
    {
        Unit unit = this[point];
        unit.onEndMove = null;
        Point target = new Point(point);

        for(int i = 0; i < unit.Distance; i++)
        {
            if (IsEmpty(target + Point.up)) target += Point.up;
            else break;
        }

        this[point] = null;
        this[point + Point.down] = null;

        this[target] = unit;
        this[target + Point.down] = unit;
        unit.MoveTo(PointToVector3(target), MoveMode.MOVE);
        unit.onEndMove += PushUnit;
    }

    private void PushUnit(Point point)
    {
        Unit unit = this[point];
        unit.onEndMove = null;

        Rotate();
        InstantiateUnit();
    }

    private bool IsEmpty(Point point) => this[point] == null;

    public void ScrollUnit(Point oldPoint, Point newPoint)
    {
        Unit unit = this[oldPoint];

        this[oldPoint] = null;
        this[oldPoint + Point.down] = null;

        this[newPoint] = unit;
        this[newPoint + Point.down] = unit;

        unit.MoveTo(PointToVector3(newPoint), MoveMode.SCROLL);
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

    public static readonly Point zero = new Point(0, 0);
    public static readonly Point one = new Point(1, 1);
    public static readonly Point up = new Point(0, 1);
    public static readonly Point down = new Point(0, -1);
    public static readonly Point right = new Point(1, 0);
    public static readonly Point left = new Point(-1, 0);
}