using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class OpponentBoardArrow : BattleBoardArrow
    {
        private object _target;

        public void SetTarget(object target)
        {
            _target = target;

            switch (_target)
            {
                case Player player:
                    TargetPosition = player.AvatarObject.transform.position;
                    player.SetGlowStatus(true);
                    break;
                case BoardUnit unit:
                    TargetPosition = unit.Transform.position;
                    unit.SetSelectedUnit(true);
                    break;
            }

            TargetPosition.z = 0;

            UpdateLength(TargetPosition, false);
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
            UpdateLength(TargetPosition, false);
        }

        private void Awake()
        {
            Init();
            SetInverse();
        }
    }
}
