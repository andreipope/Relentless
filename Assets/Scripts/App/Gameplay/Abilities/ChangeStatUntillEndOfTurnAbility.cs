using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStatUntillEndOfTurnAbility : AbilityBase
    {
        public int Defense { get; }

        public int Damage { get; }

        private List<BoardUnitView> _boardUnits;

        public ChangeStatUntillEndOfTurnAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Defense = ability.Defense;
            Damage = ability.Damage;

            _boardUnits = new List<BoardUnitView>();
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue: true);

            InvokeActionTriggered();
        }


        protected override void UnitDiedHandler()
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            InvokeActionTriggered();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            _boardUnits.Clear();

            foreach (Enumerators.AbilityTarget targetType in AbilityTargetTypes)
            {
                switch (targetType)
                {
                    case Enumerators.AbilityTarget.PLAYER_ALL_CARDS:
                    case Enumerators.AbilityTarget.PLAYER_CARD:
                        _boardUnits.AddRange(PlayerCallerOfAbility.BoardCards);
                        break;
                    case Enumerators.AbilityTarget.OPPONENT_ALL_CARDS:
                    case Enumerators.AbilityTarget.OPPONENT_CARD:
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

                if (Defense != 0)
                {
                    unit.Model.HpDebuffUntillEndOfTurn += Defense;
                    unit.Model.CurrentDefense += Defense;
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
                    unit.Model.CurrentDefense -= unit.Model.HpDebuffUntillEndOfTurn;
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
