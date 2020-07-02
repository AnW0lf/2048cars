using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Field
{
    public class GameField : MonoBehaviour
    {
        [SerializeField] private GameObject carPrefab = null;

        public static readonly int size = 10;

        private Car[,] cells = new Car[size, size];
        private Direction direction = Direction.UP;

        public Car this[int x, int y] { get => cells[x, y]; private set => cells[x, y] = value; }
        public Car this[Point point] { get => cells[point.X, point.Y]; private set => cells[point.X, point.Y] = value; }

        private void Start()
        {
            InstantiateCar();
        }

        private void InstantiateCar()
        {
            Point startPoint = new Point(Random.Range(2, 8), 1);

            GameObject obj = Instantiate(carPrefab, transform);
            Car cell = obj.GetComponent<Car>();
            if (cell == null) throw new ArgumentException("GameObject \'CarPrefab\' does not contains component realizing interface \'ICell\'.");

            cell.State = MoveState.SCROLL;
            cell.Distance = Random.Range(2, 7);
            cell.SetPosition(PointToVector3(RelativeToAbsolute(startPoint)));
            cell.onEndMove += Next;

            obj.GetComponent<CarScroller>().onEndScrolling += MoveCar;

            cell.Top = startPoint;
            cell.Bot = startPoint + Point.down;
            this[RelativeToAbsolute(cell.Top)] = cell;
            this[RelativeToAbsolute(cell.Bot)] = cell;
        }

        private void Next()
        {
            direction = (Direction)(((int)direction + 1) % 4);
            StartCoroutine(Rotor());
        }

        private IEnumerator Rotor()
        {
            float start = transform.eulerAngles.z,
                  end = (int)direction * 90f,
                  timer = 0f,
                  duration = 1.5f;
            WaitForEndOfFrame wait = new WaitForEndOfFrame();

            if (start > end) end = 360f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                Vector3 euler = transform.eulerAngles;
                euler.z = Mathf.Lerp(start, end, timer / duration);
                transform.eulerAngles = euler;
                yield return wait;
            }

            InstantiateCar();
        }

        public void ScrollCar(Car car, Point newPoint)
        {
            Point oldAbsolute = RelativeToAbsolute(car.Top);
            Point oldAbsoluteBottom = RelativeToAbsolute(car.Bot);
            Point newAbsolute = RelativeToAbsolute(newPoint);
            Point newAbsoluteBottom = RelativeToAbsolute(newPoint + Point.down);

            this[oldAbsolute] = null;
            this[oldAbsoluteBottom] = null;

            this[newAbsolute] = car;
            this[newAbsoluteBottom] = car;
            car.Top = newPoint;
            car.Bot = newPoint + Point.down;

            car.MoveTo(PointToVector3(newAbsolute));
        }

        private void MoveCar(Point current)
        {
            Car car = this[RelativeToAbsolute(current)];
            if (car == null) throw new ArgumentException(string.Format("Field does not contains car on point: relative {0}, absolute {1}", current, RelativeToAbsolute(current)));

            StartCoroutine(MovingCar(car));
        }

        private void MoveCar(Car car, Point to, MoveState state)
        {
            Point currentBottom = to + Point.down;
            Point target = new Point(to);
            target += (Point.up + Direction.UP);
            Point targetBottom = target + Point.down;

            this[RelativeToAbsolute(to)] = null;
            this[RelativeToAbsolute(currentBottom)] = null;

            this[RelativeToAbsolute(target)] = car;
            this[RelativeToAbsolute(targetBottom)] = car;

            car.Top = target;
            car.Bot = targetBottom;

            car.State = state;
            car.MoveTo(PointToVector3(RelativeToAbsolute(target)));
        }

        private IEnumerator MovingCar(Car car)
        {
            print(car.Distance);
            for (int i = 0; i < car.Distance; i++)
            {
                if (CanMove(car)) MoveCar(car, car.Top + Point.up, MoveState.MOVE);
                else print("STOP");
                //MoveCar(car, car.Top + Point.up, MoveState.MOVE);
                yield return null;
            }
        }

        private bool CanMove(Car car)
        {
            Point min = new Point(2, 2);
            Point max = new Point(7, 7);
            Point top = car.Top;
            Point bot = car.Bot;
            if (RelativeToAbsolute(top).Y == RelativeToAbsolute(bot).Y)
            {
                Point newTop = top + Point.up;
                Point newBot = bot + Point.up;
                if (Point.InRange(newTop, min, max) && Point.InRange(newBot, min, max) &&
                    this[RelativeToAbsolute(newTop)] == null && this[RelativeToAbsolute(newBot)] == null)
                    return true;
            }
            else
            {
                Point newTop = top + Point.up;
                if (Point.InRange(newTop, min, max) &&
                    this[RelativeToAbsolute(newTop)] == null)
                    return true;
            }
            return false;
        }

        private Point RelativeToAbsolute(Point relative)
        {
            Point absolute = new Point(relative);

            switch (direction)
            {
                case Direction.UP:
                    break;
                case Direction.RIGHT:
                    absolute.X = relative.Y;
                    absolute.Y = size - 1 - relative.X;
                    break;
                case Direction.DOWN:
                    absolute.X = size - 1 - relative.X;
                    absolute.Y = size - 1 - relative.Y;
                    break;
                case Direction.LEFT:
                    absolute.X = size - 1 - relative.Y;
                    absolute.Y = relative.X;
                    break;
                default:
                    throw new NotImplementedException(
                    string.Format("switch/case does not contains implementation for enum \'Direction\' value {0}.", direction));
            }

            return absolute;
        }

        private Vector3 PointToVector3(Point point)
        {
            float x = point.X - (size - 1) / 2f;
            float y = point.Y - (size - 1) / 2f;
            return new Vector3(x, y);
        }

        public static Point Vector3ToPoint(Vector3 pos)
        {
            int x = Mathf.RoundToInt(pos.x + 5.5f);
            int y = Mathf.RoundToInt(pos.y + 5.5f);
            return new Point(x, y);
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < size * size; i++)
            {
                int x = i % size, y = i / size;
                Vector3 center = PointToVector3(RelativeToAbsolute(new Point(x, y)));
                Color color = Color.clear;

                if (x >= 2 && x <= 7 && y >= 2 && y <= 7) color = Color.red;
                else if ((x >= 2 && x <= 7 && (y <= 1 || y >= 8)) ||
                    (y >= 2 && y <= 7 && (x <= 1 || x >= 8))) color = Color.blue;
                else color = Color.white;

                if (this[x, y] != null) color = Color.green;
                else color.a = 0.5f;

                Gizmos.color = color;
                Gizmos.DrawCube(center, Vector3.one * 0.95f);
            }
        }
    }

    public class Point
    {
        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }

        private int x, y;

        public static readonly Point zero = new Point();
        public static readonly Point one = new Point(1, 1);
        public static readonly Point up = new Point(0, 1);
        public static readonly Point down = new Point(0, -1);
        public static readonly Point right = new Point(1, 0);
        public static readonly Point left = new Point(-1, 0);

        public Point() { x = 0; y = 0; }

        public Point(int x, int y) { this.x = x; this.y = y; }

        public Point(Point other) { x = other.x; y = other.y; }

        public static Point Clamp(Point point, Point first, Point second)
        {
            int minX = Mathf.Min(first.x, second.x),
                minY = Mathf.Min(first.y, second.y),
                maxX = Mathf.Max(first.x, second.x),
                maxY = Mathf.Max(first.y, second.y);

            Point clamped = new Point(point);
            clamped.x = Mathf.Clamp(clamped.x, minX, maxX);
            clamped.y = Mathf.Clamp(clamped.y, minY, maxY);
            return clamped;
        }

        public static bool InRange(Point point, Point first, Point second)
        {
            int minX = Mathf.Min(first.x, second.x),
                minY = Mathf.Min(first.y, second.y),
                maxX = Mathf.Max(first.x, second.x),
                maxY = Mathf.Max(first.y, second.y);

            return point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY;
        }

        public override int GetHashCode()
        {
            var hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }

        public override string ToString() { return string.Format("({0}; {1})", x, y); }

        public override bool Equals(object obj)
        {
            return obj is Point point &&
                   x == point.x &&
                   y == point.y;
        }

        public static Point operator +(Point p1, Point p2) { return new Point(p1.x + p2.x, p1.y + p2.y); }

        public static Point operator +(Point p, Direction direction)
        {
            switch (direction)
            {
                case Direction.UP:
                    return p + up;
                case Direction.LEFT:
                    return p + left;
                case Direction.DOWN:
                    return p + down;
                case Direction.RIGHT:
                    return p + right;
                default:
                    throw new NotImplementedException(
                    string.Format("switch/case does not contains implementation for enum \'Direction\' value {0}.", direction));
            }
        }

        public static Point operator +(Direction direction, Point p) { return p + direction; }

        public static Point operator -(Point p1, Point p2) { return new Point(p1.x - p2.x, p1.y - p2.y); }

        public static Point operator -(Point p) { return new Point(-p.x, -p.y); }

        public static Point operator *(Point p, int i) { return new Point(p.x * i, p.y * i); }

        public static Point operator *(int i, Point p) { return new Point(p.x * i, p.y * i); }

        public static bool operator ==(Point p1, Point p2) { return p1.Equals(p2); }

        public static bool operator !=(Point p1, Point p2) { return !p1.Equals(p2); }
    }

    public enum Direction { UP = 0, RIGHT = 1, DOWN = 2, LEFT = 3 }
}