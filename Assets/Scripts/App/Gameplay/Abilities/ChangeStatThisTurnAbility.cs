using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class ChangeStatThisTurnAbility : AbilityBase
    {
        private List<ChangedStatInfo> _affectedUnits;
        public int Attack { get; }

        public int Defense { get; }

        public ChangeStatThisTurnAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Attack = ability.Damage;
            Defense = ability.Defense;

            _affectedUnits = new List<ChangedStatInfo>();
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            RestoreAffectedUnits();
        }

        private void HandleTargets()
        {
            foreach(Enumerators.Target target in AbilityTargets)
            {
                switch(target)
                {
                    case Enumerators.Target.PLAYER:
                        break;
                    case Enumerators.Target.OPPONENT:
                        break;
                }
            }
        }

        private void RestoreAffectedUnits()
        {
            foreach(ChangedStatInfo changedStatInfo in _affectedUnits)
            {
                changedStatInfo.BoardUnitModel.CurrentDefense -= changedStatInfo.RemovedDefense;
                changedStatInfo.BoardUnitModel.BuffedDefense -= changedStatInfo.RemovedDefense;

                changedStatInfo.BoardUnitModel.CurrentDamage -= changedStatInfo.RemovedAttack;
                changedStatInfo.BoardUnitModel.BuffedDamage -= changedStatInfo.RemovedAttack;
            }
        }

        private void ChangeStatsOfUnits(List<BoardUnitModel> units, int defense, int damage)
        {
            foreach (BoardUnitModel unit in units)
            {
                unit.CurrentDefense += defense;
                unit.BuffedDefense += defense;

                unit.CurrentDamage += damage;
                unit.BuffedDamage += damage;
            }
        }

        class ChangedStatInfo
        {
            public BoardUnitModel BoardUnitModel;

            public int RemovedDefense;

            public int RemovedAttack;
        }
    }
}
