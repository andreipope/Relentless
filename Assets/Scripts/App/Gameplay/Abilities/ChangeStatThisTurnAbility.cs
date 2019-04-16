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
            List<CardModel> targets = new List<CardModel>();

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
                changedStatInfo.CardModel.CurrentDefense += changedStatInfo.RemovedDefense;
                changedStatInfo.CardModel.CurrentDamage += changedStatInfo.RemovedAttack;
            }

            _affectedUnits.Clear();
        }

        private void SetStatsOfUnits(List<CardModel> units, int defense, int damage)
        {
            ChangedStatInfo changedStatInfo;
            foreach (CardModel unit in units)
            {
                changedStatInfo = new ChangedStatInfo()
                {
                    CardModel = unit,
                    RemovedAttack = unit.CurrentDamage - damage,
                    RemovedDefense = unit.CurrentDefense - defense
                };

                unit.CurrentDefense = defense;
                unit.CurrentDamage = damage;

                _affectedUnits.Add(changedStatInfo);
            }
        }

        class ChangedStatInfo
        {
            public CardModel CardModel;

            public int RemovedDefense;

            public int RemovedAttack;
        }
    }
}
