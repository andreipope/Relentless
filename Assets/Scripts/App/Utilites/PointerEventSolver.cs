using System;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class PointerEventSolver
    {
        private float _pressTimer;

        private float _dragDelta;

        private bool _isResolved;

        private Vector3 _initialPointerPosition;

        public PointerEventSolver()
        {
            _dragDelta = Constants.POINTER_MIN_DRAG_DELTA;
        }

        public event Action OnClickEvent;

        public event Action OnDragStartedEvent;

        public event Action OnEndEvent;

        public bool IsPushed { get; private set; }

        public void PushPointer(float delta = -1)
        {
            if (IsPushed)

                return;

            IsPushed = true;
            _isResolved = false;

            _initialPointerPosition = Input.mousePosition;
            _pressTimer = 0f;

            if (delta >= 0)
            {
                _dragDelta = delta;
            }
        }

        public void Update()
        {
            if (!IsPushed || _isResolved)

                return;

            if (Mathf.Abs((_initialPointerPosition - Input.mousePosition).magnitude) > _dragDelta)
            {
                _isResolved = true;
                OnDragStartedEvent?.Invoke();
            } else
            {
                _pressTimer += Time.unscaledDeltaTime;

                if (_pressTimer >= Constants.POINTER_ON_CLICK_DELAY)
                {
                    _isResolved = true;
                    OnDragStartedEvent?.Invoke();
                }
            }
        }

        public void PopPointer()
        {
            if (!IsPushed)

                return;

            if (!_isResolved)
            {
                OnClickEvent?.Invoke();
            }

            IsPushed = false;
            _isResolved = false;

            OnEndEvent?.Invoke();
        }
    }
}
