using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AdjacentUnitsGetStatAndHeavyAbility : AbilityBase
    {
        public Enumerators.Stat StatType { get; }

        public int Defense { get; }

        public int Damage { get; }

        public AdjacentUnitsGetStatAndHeavyAbility(Enumerators.CardKind cardKind, AbilityData ability)
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

            ChangeStats(BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner), Defense, Damage);
        }

        protected override void ChangeAuraStatusAction(bool status)
        {
            base.ChangeAuraStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            if (status)
            {
                ChangeStats(BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner), Defense, Damage);
            }
            else
            {
                ChangeStats(BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner), -Defense, -Damage);
            }
        }

        private void ChangeStats(List<BoardUnitModel> units, int defense, int damage)
        {
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (BoardUnitModel unit in units)
            {
                if (StatType == Enumerators.Stat.DEFENSE)
                {
                    unit.BuffedDefense += defense;
                    unit.CurrentDefense += defense;

                    targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = defense >= 0 ? Enumerators.ActionEffectType.ShieldBuff : Enumerators.ActionEffectType.ShieldDebuff,
                        Target = unit,
                    });
                }
                else if (StatType == Enumerators.Stat.DAMAGE)
                {
                    unit.BuffedDamage += damage;
                    unit.CurrentDamage += damage;

                    targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = damage >= 0 ? Enumerators.ActionEffectType.AttackBuff : Enumerators.ActionEffectType.AttackDebuff,
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
