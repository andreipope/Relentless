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

        private List<CardModel> _boardUnits = new List<CardModel>();

        public ChangeStatUntillEndOfTurnAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Defense = ability.Defense;
            Damage = ability.Damage;
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

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action(TargetUnit);
            }
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

            if (info != null)
            {
                _boardUnits.Add((CardModel)info);
            }
            else
            {
                foreach (Enumerators.Target targetType in AbilityTargets)
                {
                    switch (targetType)
                    {
                        case Enumerators.Target.PLAYER_ALL_CARDS:
                        case Enumerators.Target.PLAYER_CARD:
                            _boardUnits.AddRange(PlayerCallerOfAbility.CardsOnBoard);
                            break;
                        case Enumerators.Target.OPPONENT_ALL_CARDS:
                        case Enumerators.Target.OPPONENT_CARD:
                            _boardUnits.AddRange(GetOpponentOverlord().CardsOnBoard);
                            break;
                    }
                }
            }

            foreach (CardModel unit in _boardUnits)
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

                if (Defense != 0)
                {
                    unit.HpDebuffUntillEndOfTurn += Defense;
                    unit.CurrentDefense += Defense;
                }
            }
        }

        protected override void TurnEndedHandler()
        {
            if (_boardUnits.Count <= 0) 
                return;

            base.TurnEndedHandler();

            foreach (CardModel unit in _boardUnits)
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
                    unit.CurrentDefense -= unit.HpDebuffUntillEndOfTurn;
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

            AbilityProcessingAction?.TriggerActionManually();
        }
    }
}
