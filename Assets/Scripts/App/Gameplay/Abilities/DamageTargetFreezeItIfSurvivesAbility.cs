using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class DamageTargetFreezeItIfSurvivesAbility : AbilityBase
    {
        public DamageTargetFreezeItIfSurvivesAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            HandleSubtriggers();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                DamageTarget(AffectObjectType == Enumerators.AffectObjectType.Player ? (IBoardObject)TargetPlayer : TargetUnit);
            }
        }

        private void HandleSubtriggers()
        {
            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                List<CardModel> units;
                if (PredefinedTargets != null)
                {
                    units = PredefinedTargets.Select(x => (x.BoardObject as CardModel)).ToList();
                }
                else
                {
                    units = GetRandomEnemyUnits(1);
                }
                if (units.Count > 0)
                {
                    DamageTarget(units[0]);
                }
            }
        }

        private void DamageTarget(IBoardObject boardObject)
        {
            IBoardObject caller = AbilityUnitOwner;
            IBoardObject target;
            Enumerators.ActionType actionType;

            bool isFreezed = false;

            switch (boardObject)
            {
                case Player player:
                    BattleController.AttackPlayerByAbility(caller, AbilityData, player);
                    target = player;
                    actionType = Enumerators.ActionType.CardAffectingOverlord;

                    if (player.Defense > 0)
                    {
                        player.Stun(Enumerators.StunType.FREEZE, 1);
                        isFreezed = true;
                    }
                    break;
                case CardModel unit:
                    BattleController.AttackUnitByAbility(caller, AbilityData, unit);
                    target = unit;
                    actionType = Enumerators.ActionType.CardAffectingCard;

                    if (unit.CurrentDefense > 0)
                    {
                        unit.Stun(Enumerators.StunType.FREEZE, 1);
                        isFreezed = true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(boardObject), boardObject, null);
            }

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject> {
                    new ParametrizedAbilityBoardObject(target)
                }
            );

            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            targetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                Target = target,
                HasValue = true,
                Value = -AbilityData.Value
            });

            if (isFreezed)
            {
                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.Freeze,
                    Target = target,
                });
            }

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = actionType,
                Caller = AbilityUnitOwner,
                TargetEffects = targetEffects
            });
        }
    }
}
