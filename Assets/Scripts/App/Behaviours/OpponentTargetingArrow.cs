using UnityEngine;

namespace GrandDevs.CZB
{
    public class OpponentTargetingArrow : FightTargetingArrow
    {
        private Vector3 _target = Vector3.zero;

        protected override void Update()
        {
            UpdateLength(_target);
        }

        protected override void LateUpdate()
        {
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