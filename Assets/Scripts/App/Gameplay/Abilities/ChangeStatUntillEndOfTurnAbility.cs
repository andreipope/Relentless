using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStatUntillEndOfTurnAbility : AbilityBase
    {
        public int Health { get; }

        public int Damage { get; }

        private List<BoardUnitModel> _boardUnits = new List<BoardUnitModel>();

        public ChangeStatUntillEndOfTurnAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Health = ability.Health;
            Damage = ability.Damage;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue: true);

            InvokeActionTriggered();
        }


        protected override void UnitDiedHandler()
        {
            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            InvokeActionTriggered();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            _boardUnits.Clear();

            foreach (Enumerators.AbilityTargetType targetType in AbilityTargetTypes)
            {
                switch (targetType)
                {
                    case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:
                    case Enumerators.AbilityTargetType.PLAYER_CARD:
                        _boardUnits.AddRange(PlayerCallerOfAbility.CardsOnBoard);
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                        _boardUnits.AddRange(GetOpponentOverlord().CardsOnBoard);
                        break;
                }
            }

            foreach (BoardUnitModel unit in _boardUnits)
            {
                if (Damage != 0)
                {
                    unit.DamageDebuffUntillEndOfTurn += Damage;
                    int buffresult = unit.CurrentDamage + Damage;

                    if (buffresult < 0)
                    {
                        unit.DamageDebuffUntillEndOfTurn -= buffresult;
                    }

                    unit.CurrentDamage += Damage;
                }

                if (Health != 0)
                {
                    unit.HpDebuffUntillEndOfTurn += Health;
                    unit.CurrentHp += Health;
                }
            }
        }

        protected override void TurnEndedHandler()
        {
            if (_boardUnits.Count <= 0) 
                return;

            base.TurnEndedHandler();

            foreach (BoardUnitModel unit in _boardUnits)
            {
                if (unit == null)
                    continue;

                if (unit.DamageDebuffUntillEndOfTurn != 0)
                {
                    unit.CurrentDamage -= unit.DamageDebuffUntillEndOfTurn;
                    unit.DamageDebuffUntillEndOfTurn = 0;
                }

                if (unit.HpDebuffUntillEndOfTurn != 0)
                {
                    unit.CurrentHp -= unit.HpDebuffUntillEndOfTurn;
                    unit.HpDebuffUntillEndOfTurn = 0;
                }
            }

            _boardUnits.Clear();

            Deactivate();
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Action();

            AbilityProcessingAction?.ForceActionDone();
        }
    }
}
