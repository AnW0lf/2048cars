using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Field
{
    public class CarScroller : MonoBehaviour
    {
        private static readonly Point min = new Point(2, 1);
        private static readonly Point max = new Point(7, 1);

        private Point current;
        private GameField field;
        private Car car;

        public UnityAction<Point> onEndScrolling = null;

        private void OnEnable()
        {
            field = FindObjectOfType<GameField>();
            if (field == null) throw new ArgumentException("\'CarScroller\' can not find GameObject with component \'GameField\'.");
            car = GetComponent<Car>();
        }

        private void OnMouseDown()
        {
            current = GameField.Vector3ToPoint(transform.position);
            current = Point.Clamp(current, min, max);
        }

        private void OnMouseDrag()
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.left;
            Point point = GameField.Vector3ToPoint(worldPosition);
            point = Point.Clamp(point, min, max);

            if (point != current)
            {
                field.ScrollCar(car, point);
                current = point;
            }

        }

        private void OnMouseUp()
        {
            onEndScrolling?.Invoke(current);
        }

        private void OnDrawGizmos()
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Gizmos.DrawSphere(worldPosition, 0.5f);
        }
    }
}
