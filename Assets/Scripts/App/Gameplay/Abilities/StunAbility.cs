using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class StunAbility : AbilityBase
    {
        public Enumerators.Stat StatType { get; }

        public int Value { get; }

        public StunAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.Stat;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (AbilityEffect)
            {
                case Enumerators.AbilityEffect.STUN_FREEZES:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
                    break;
                default:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
                    break;
            }

            InvokeUseAbilityEvent();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (AbilityTargets.Contains(Enumerators.Target.OPPONENT_ALL_CARDS))
            {
                List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

                foreach (CardModel unit in GetOpponentOverlord().CardsOnBoard)
                {
                    StunUnit(unit);

                    targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.Freeze,
                        Target = unit,
                    });
                }

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                    Caller = AbilityUnitOwner,
                    TargetEffects = targetEffects
                });
            }
        }

        protected override void UnitDamagedHandler(IBoardObject info)
        {
            base.UnitDamagedHandler(info);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AT_DEFENCE)
                return;

            if (info is CardModel unit)
            {
                StunUnit(unit);

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = AbilityUnitOwner,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.Freeze,
                            Target = unit,
                        }
                    }
                });
            }
        }

        protected override void UnitAttackedHandler(IBoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            if (info is CardModel unit)
            {
                StunUnit(unit);

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = AbilityUnitOwner,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.Freeze,
                            Target = unit,
                        }
                    }
                });
            }
        }

        private void StunUnit(CardModel unit)
        {
            unit.Stun(Enumerators.StunType.FREEZE, 1);

            CreateVfx(BattlegroundController.GetCardViewByModel<BoardUnitView>(unit).Transform.position);
        }
    }
}
