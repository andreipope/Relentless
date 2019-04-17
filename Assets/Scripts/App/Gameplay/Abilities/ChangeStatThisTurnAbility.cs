using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class ChangeStatThisTurnAbility : AbilityBase
    {
        private List<ChangedStatInfo> _affectedUnits;
        private int Attack { get; }

        private int Defense { get; }

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

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            HandleTargets();
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
            List<BoardUnitModel> targets = new List<BoardUnitModel>();

            foreach (Enumerators.Target target in AbilityTargets)
            {
                switch (target)
                {
                    case Enumerators.Target.PLAYER_CARD:
                    case Enumerators.Target.PLAYER_ALL_CARDS:
                        targets.AddRange(GetAliveUnits(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard));
                        break;
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                    case Enumerators.Target.OPPONENT_CARD:
                        targets.AddRange(GetAliveUnits(GetOpponentOverlord().PlayerCardsController.CardsOnBoard));
                        break;
                }
            }

            SetStatsOfUnits(targets, Defense, Attack);
        }

        private void RestoreAffectedUnits()
        {
            foreach(ChangedStatInfo changedStatInfo in _affectedUnits)
            {
                changedStatInfo.BoardUnitModel.CurrentDefense += changedStatInfo.RemovedDefense;
                changedStatInfo.BoardUnitModel.CurrentDamage += changedStatInfo.RemovedAttack;
            }

            _affectedUnits.Clear();
        }

        private void SetStatsOfUnits(List<BoardUnitModel> units, int defense, int damage)
        {
            ChangedStatInfo changedStatInfo;
            foreach (BoardUnitModel unit in units)
            {
                changedStatInfo = new ChangedStatInfo()
                {
                    BoardUnitModel = unit,
                    RemovedAttack = damage != 0 ? unit.CurrentDamage - damage : 0,
                    RemovedDefense = defense != 0 ? unit.CurrentDefense - defense : 0
                };

                if (defense != 0)
                {
                    unit.CurrentDefense = defense;
                }

                if (damage != 0)
                {
                    unit.CurrentDamage = damage;
                }

                _affectedUnits.Add(changedStatInfo);
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
