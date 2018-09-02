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

            switch (_target)
            {
                case Player player:
                    _targetPosition = player.AvatarObject.transform.position;
                    player.SetGlowStatus(true);
                    break;
                case BoardUnit unit:
                    _targetPosition = unit.Transform.position;
                    unit.SetSelectedUnit(true);
                    break;
            }

            _targetPosition.z = 0;

            UpdateLength(_targetPosition, false);
        }

        public override void Dispose()
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
            base.Dispose();
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
