using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AdjacentUnitsGetStatAbility : AbilityBase
    {
        public Enumerators.Stat StatType { get; }

        public int Defense { get; }

        public int Damage { get; }

        public AdjacentUnitsGetStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.Stat;
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

            List<BoardUnitModel> adjacent = BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner);

            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (BoardUnitModel unit in adjacent)
            {
                if (StatType == Enumerators.Stat.DEFENSE)
                {
                    unit.BuffedDefense += Defense;
                    unit.CurrentDefense += Defense;

                    targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                        Target = unit,
                    });
                }
                else if (StatType == Enumerators.Stat.DAMAGE)
                {
                    unit.BuffedDamage += Damage;
                    unit.CurrentDamage += Damage;

                    targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.AttackBuff,
                        Target = unit
                    });
                }
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = GetCaller(),
                TargetEffects = targetEffects
            });
        }
    }
}
