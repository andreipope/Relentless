// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using UnityEngine;
using System;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public class PointerEventSolver
    {
        public event Action OnClickEvent;
        public event Action OnDragStartedEvent;
        public event Action OnEndEvent;

        private float _pressTimer = 0f;
        private float _minDragDelta = 3f;

        private bool _isResolved = false;

        private Vector3 _initialPointerPosition;

        public bool IsPushed { get; private set; }

        public void PushPointer()
        {
            if (IsPushed)
                return;

            IsPushed = true;

            _initialPointerPosition = Input.mousePosition;
            _pressTimer = 0f;
        }

        public void Update()
        {
            if (!IsPushed || _isResolved)
                return;

            if ((_initialPointerPosition - Input.mousePosition).magnitude > _minDragDelta)
            {
                _isResolved = true;
                OnDragStartedEvent?.Invoke();
            }
            else
            {
                _pressTimer += Time.unscaledDeltaTime;

                if (_pressTimer >= Constants.TOOLTIP_APPEAR_ON_CLICK_DELAY)
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
                OnClickEvent?.Invoke();

            IsPushed = false;
            _isResolved = false;

            OnEndEvent?.Invoke();
        }
    }
}
