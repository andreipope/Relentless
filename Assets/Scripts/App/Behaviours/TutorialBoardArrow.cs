// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class TutorialBoardArrow : BoardArrow
    {
        public bool isEnabled;

        private Vector3 _targetPosition;

        public void UpdateTargetPosition(Vector3 position)
        {
            DestroyMiddleBlocks();
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

        protected void Awake()
        {
        }

        protected override void Update()
        {
            //base.Update();
            if(isEnabled)
            {
                UpdateLength(_targetPosition);
            }
        }

        private void UpdateVisibility()
        {
            gameObject.SetActive(isEnabled);
        }

    }
}
