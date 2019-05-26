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

            if (AbilityActivity != Enumerators.AbilityActivity.ACTIVE)
            {
                InvokeUseAbilityEvent();
            }

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY || AbilityActivity != Enumerators.AbilityActivity.PASSIVE)
                return;

            AbilityProcessingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue: true);

            InvokeActionTriggered();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action(TargetUnit);
                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });
            }
        }

        protected override void UnitDiedHandler()
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            AbilityProcessingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityUsageBlocker);

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
                            if(AbilityUnitOwner != null && _boardUnits.Contains(AbilityUnitOwner))
                            {
                                _boardUnits.Remove(AbilityUnitOwner);
                            }
                            break;
                        case Enumerators.Target.OPPONENT_ALL_CARDS:
                        case Enumerators.Target.OPPONENT_CARD:
                            _boardUnits.AddRange(GetOpponentOverlord().CardsOnBoard);
                            break;
                    }
                }
            }

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

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

                    unit.AddToCurrentDamageHistory(unit.DamageDebuffUntillEndOfTurn, Enumerators.ReasonForValueChange.AbilityBuff);

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = unit.DamageDebuffUntillEndOfTurn > 0 ? Enumerators.ActionEffectType.AttackBuff : Enumerators.ActionEffectType.AttackDebuff,
                        Target = unit,
                        HasValue = true,
                        Value = unit.DamageDebuffUntillEndOfTurn
                    });
                }

                if (Defense != 0)
                {
                    unit.HpDebuffUntillEndOfTurn += Defense;
                    unit.AddToCurrentDefenseHistory(Defense, Enumerators.ReasonForValueChange.AbilityBuff);

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Defense > 0 ? Enumerators.ActionEffectType.ShieldBuff : Enumerators.ActionEffectType.ShieldDebuff,
                        Target = unit,
                        HasValue = true,
                        Value = Defense
                    });
                }
            }

            if (TargetEffects.Count > 0)
            {
                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                    Caller = AbilityUnitOwner,
                    TargetEffects = TargetEffects
                });
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
                    unit.AddToCurrentDamageHistory(-unit.DamageDebuffUntillEndOfTurn, Enumerators.ReasonForValueChange.AbilityBuff);
                    unit.DamageDebuffUntillEndOfTurn = 0;
                }

                if (unit.HpDebuffUntillEndOfTurn != 0)
                {
                    unit.AddToCurrentDefenseHistory(-unit.HpDebuffUntillEndOfTurn, Enumerators.ReasonForValueChange.AbilityBuff);
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

            AbilityProcessingAction?.TriggerActionExternally();
        }
    }
}
