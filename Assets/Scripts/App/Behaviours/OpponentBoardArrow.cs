using UnityEngine;

namespace LoomNetwork.CZB
{
    public class OpponentBoardArrow : BattleBoardArrow
    {
        private Vector3 _targetPosition = Vector3.zero;

        private object _target;

        public void SetTarget(object target)
        {
            _target = target;

            if (_target is Player)
            {
                _targetPosition = (_target as Player).AvatarObject.transform.position;
                (_target as Player).SetGlowStatus(true);
            }
            else if (_target is BoardUnit)
            {
                _targetPosition = (_target as BoardUnit).Transform.position;
                (_target as BoardUnit).SetSelectedUnit(true);
            }

            _targetPosition.z = 0;

            UpdateLength(_targetPosition, false);
            CreateTarget(_targetPosition);
        }

        public void Dispose()
        {
            if (_target != null)
            {
                if (_target is Player)
                {
                    (_target as Player).SetGlowStatus(false);
                }
                else
                {
                    (_target as BoardUnit)?.SetSelectedUnit(false);
                }

                _target = null;
            }
        }

        protected override void Update()
        {
            UpdateLength(_targetPosition, false);
        }

        private void Awake()
        {
            Init();
            SetInverse();
        }
    }
}
