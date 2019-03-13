using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AdjacentUnitsGetStatAbility : AbilityBase
    {
        public Enumerators.StatType StatType { get; }

        public int Defense { get; }

        public int Damage { get; }

        public AdjacentUnitsGetStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.AbilityStatType;
            Damage = ability.Damage;
            Defense = ability.Defense;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardUnitView> adjacent = BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner);

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (BoardUnitView unit in adjacent)
            {
                if (StatType == Enumerators.StatType.DEFENSE)
                {
                    unit.Model.BuffedDefense += Defense;
                    unit.Model.CurrentDefense += Defense;

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                        Target = unit.Model,
                    });
                }
                else if (StatType == Enumerators.StatType.DAMAGE)
                {
                    unit.Model.BuffedDamage += Damage;
                    unit.Model.CurrentDamage += Damage;

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.AttackBuff,
                        Target = unit.Model,
                    });
                }
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });
        }
    }
}
