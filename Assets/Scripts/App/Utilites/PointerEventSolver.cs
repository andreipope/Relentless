using System;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class PointerEventSolver
    {
        public static float DefaultDelta = Application.isMobilePlatform ?
                                           Constants.PointerMinDragDelta * 2f :
                                           Constants.PointerMinDragDeltaMobile;

        private float _pressTimer;

        private float _dragDelta;

        private bool _isResolved;

        private Vector3 _initialPointerPosition;

        public PointerEventSolver()
        {
            _dragDelta = Constants.PointerMinDragDelta;
        }

        public event Action Clicked;

        public event Action DragStarted;

        public event Action Ended;

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
                DragStarted?.Invoke();
            }
            else
            {
                _pressTimer += Time.unscaledDeltaTime;

                if (_pressTimer >= Constants.PointerOnClickDelay)
                {
                    _isResolved = true;
                    DragStarted?.Invoke();
                }
            }
        }

        public void PopPointer()
        {
            if (!IsPushed)
                return;

            if (!_isResolved)
            {
                Clicked?.Invoke();
            }

            IsPushed = false;
            _isResolved = false;

            Ended?.Invoke();
        }
    }
}
