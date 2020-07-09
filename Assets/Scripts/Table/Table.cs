using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Table : MonoBehaviour
{
    private int _fieldSize = 6;
    private int _turnCounter = 0;
    private Unit[,] _cells;

    public int TableSize { get => _fieldSize + 8; }
    public int FieldSize
    {
        get => _fieldSize;
        set
        {
            if (value < 2)
                throw new ArgumentException($"Field size {value} is too small. It must be greater than 2.");
            _fieldSize = value;
        }
    }

    public int Length { get => _cells.Length; }

    public Unit this[int x, int y] { get => _cells[x, y]; set => _cells[x, y] = value; }
    public Unit this[Point p] { get => _cells[p.X, p.Y]; set => _cells[p.X, p.Y] = value; }

    public void Init()
    {
        _cells = new Unit[TableSize, TableSize];
        for (int i = 0; i < _cells.Length; i++) _cells[i % TableSize, i / TableSize] = null;
    }

    public IEnumerator Rotate(UnityAction onComplete)
    {
        float start = _turnCounter * 90f;
        _turnCounter++;
        float end = _turnCounter * 90f;

        yield return new WaitForSeconds(0.5f);

        Unit[,] newCells = new Unit[TableSize, TableSize];
        for (int i = 0; i < _cells.Length; i++)
        {
            int x = i % TableSize, y = i / TableSize;
            int oldX = y, oldY = TableSize - 1 - x;
            newCells[x, y] = _cells[oldX, oldY];
        }
        _cells = newCells;

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
        return new Vector3(point.X - (TableSize - 1) / 2f, point.Y - (TableSize - 1) / 2f);
    }

    public Point Vector3ToPoint(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x + (TableSize - 1) / 2f);
        int y = Mathf.RoundToInt(pos.y + (TableSize - 1) / 2f);
        return new Point(x, y);
    }

    private void OnDrawGizmos()
    {
        if (_cells == null) return;
        for (int i = 0; i < _cells.Length; i++)
        {
            int x = i % this.TableSize, y = i / this.TableSize;

            Vector3 center = PointToVector3(new Point(x, y));
            Vector3 size = Vector3.one * 0.95f;
            Color color = Color.white;
            if (this[x, y] != null) color = Color.green;
            color.a = 0.4f;

            Gizmos.color = color;
            Gizmos.DrawCube(center, size);
        }
    }
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

    public override string ToString() => string.Format("({0} ; {1})", x, y);

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