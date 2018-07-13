// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;

namespace LoomNetwork.CZB
{
    public class OpponentBoardArrow : BattleBoardArrow
    {
        private Vector3 _target = Vector3.zero;

        private void Awake()
        {
            Init();
        }


        protected override void Update()
        {
            UpdateLength(_target);
        }

        public void SetTarget(GameObject go)
        {
            _target = go.transform.position;
            _target.z = 0;

            UpdateLength(_target);
            CreateTarget(_target);
        }
    }
}