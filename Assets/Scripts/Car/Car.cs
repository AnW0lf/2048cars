using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Field
{

    public class Car : MonoBehaviour, ICell
    {
        [SerializeField] private float scrollSpeed;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float pushSpeed;

        public bool IsMovable { get; protected set; } = true;
        public MoveState State { get; set; }
        public int Distance { get; set; }
        public UnityAction onEndMove { get; set; } = null;

        private Coroutine _moveCoroutine = null;

        public void MoveTo(Vector3 pos)
        {
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(Moving(pos));
        }

        public void SetPosition(Vector3 pos)
        {
            transform.localPosition = pos;
            transform.eulerAngles = Vector3.zero;
        }

        private IEnumerator Moving(Vector3 endPosition)
        {
            float speed = GetSpeed();
            if(speed <= 0f)
            {
                _moveCoroutine = null;
                yield break;
            }

            Vector3 startPosition = transform.localPosition;
            float timer = 0f, duration = Vector3.Distance(startPosition, endPosition) / speed;
            WaitForEndOfFrame wait = new WaitForEndOfFrame();

            while(timer < duration)
            {
                timer += Time.deltaTime;

                transform.localPosition = Vector3.Lerp(startPosition, endPosition, timer / duration);

                yield return wait;

            }

            if (State == MoveState.MOVE || State == MoveState.PUSH)
                onEndMove?.Invoke();

            _moveCoroutine = null;
        }

        private float GetSpeed()
        {
            switch (State)
            {
                case MoveState.STOP:
                    return 0f;
                case MoveState.SCROLL:
                    return scrollSpeed;
                case MoveState.MOVE:
                    return moveSpeed;
                case MoveState.PUSH:
                    return pushSpeed;
                default:
                    throw new NotImplementedException(
                    string.Format("switch/case does not contains implementation for enum \'MoveState\' value {0}.", State));
            }
        }
    }

    public interface ICell
    {
        bool IsMovable { get; }
        MoveState State { get; set; }
        int Distance { get; set; }
        void MoveTo(Vector3 pos);
        void SetPosition(Vector3 pos);
        UnityAction onEndMove { get; set; }
    }

    public enum MoveState { STOP = 0, SCROLL = 1, MOVE = 2, PUSH = 3 }
}
