// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using UnityEngine;

namespace LoomNetwork.CZB
{
    public class TutorialBoardArrow : BoardArrow
    {
        public bool isEnabled;

        private Vector3 _targetPosition;

        public void UpdateTargetPosition(Vector3 position)
        {
            _targetPosition = position;
            Activate();
        }

        public void Activate()
        {
            isEnabled = true;
            UpdateVisibility();
        }

        public void Deactivate()
        {
            isEnabled = false;
            UpdateVisibility();
        }

        protected override void Update()
        {
            // base.Update();
            if (isEnabled)
            {
                UpdateLength(_targetPosition);
            }
        }

        private void Awake()
        {
            Init();
        }

        private void UpdateVisibility()
        {
            gameObject.SetActive(isEnabled);
        }
    }
}
