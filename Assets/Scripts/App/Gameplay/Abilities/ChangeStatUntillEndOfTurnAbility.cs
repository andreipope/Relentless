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

        private List<BoardUnitView> _boardUnits;

        public ChangeStatUntillEndOfTurnAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Health = ability.Health;
            Damage = ability.Damage;

            _boardUnits = new List<BoardUnitView>();
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue: true);

            InvokeUseAbilityEvent();

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
                        _boardUnits.AddRange(PlayerCallerOfAbility.BoardCards);
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                        _boardUnits.AddRange(GetOpponentOverlord().BoardCards);
                        break;
                }
            }

            foreach (BoardUnitView unit in _boardUnits)
            {
                if (Damage != 0)
                {
                    unit.Model.DamageDebuffUntillEndOfTurn += Damage;
                    int buffresult = unit.Model.CurrentDamage + Damage;

                    if (buffresult < 0)
                    {
                        unit.Model.DamageDebuffUntillEndOfTurn -= buffresult;
                    }

                    unit.Model.CurrentDamage += Damage;
                }

                if (Health != 0)
                {
                    unit.Model.HpDebuffUntillEndOfTurn += Health;
                    unit.Model.CurrentHp += Health;
                }
            }
        }

        protected override void TurnEndedHandler()
        {
            if (_boardUnits.Count <= 0) 
                return;

            base.TurnEndedHandler();

            foreach (BoardUnitView unit in _boardUnits)
            {
                if (unit == null || unit.Model == null)
                    continue;

                if (unit.Model.DamageDebuffUntillEndOfTurn != 0)
                {
                    unit.Model.CurrentDamage -= unit.Model.DamageDebuffUntillEndOfTurn;
                    unit.Model.DamageDebuffUntillEndOfTurn = 0;
                }

                if (unit.Model.HpDebuffUntillEndOfTurn != 0)
                {
                    unit.Model.CurrentHp -= unit.Model.HpDebuffUntillEndOfTurn;
                    unit.Model.HpDebuffUntillEndOfTurn = 0;
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
