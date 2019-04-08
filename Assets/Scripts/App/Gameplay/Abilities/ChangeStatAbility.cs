using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class ChangeStatAbility : AbilityBase
    {
        public Enumerators.Stat StatType { get; }

        public int Value { get; }

        public int Attack { get; }

        public int Defense { get; }

        public ChangeStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.Stat;
            Value = ability.Value;
            Attack = ability.Damage;
            Defense = ability.Defense;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityActivity == Enumerators.AbilityActivity.PASSIVE)
            {
                InvokeUseAbilityEvent();

                if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.AllOtherAllyUnitsInPlay)
                {
                    if (AbilityTargets.Contains(Enumerators.Target.ITSELF))
                    {
                        ChangeStatsToItself();
                    }
                    else if(AbilityTargets.Contains(Enumerators.Target.PLAYER_ALL_CARDS))
                    {
                        ChangeStatsOfPlayerAllyCards(false);
                    }
                }
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                ChangeStatsOfTarget(TargetUnit);

                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });
            }
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            ChangeStatsToItself();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            ChangeStatsToItself();
        }

		protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.END)
                return;

            ChangeStatsToItself();
        }

        private void ChangeStatsToItself()
        {
            ChangeStatsOfTarget(AbilityUnitOwner);
        }

        private void ChangeStatsOfPlayerAllyCards(bool withCaller = false)
        {
            foreach (BoardUnitModel unit in PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard)
            {
                if (!withCaller && unit.Card == BoardUnitModel.Card)
                    continue;

                ChangeStatsOfTarget(unit);
            }
        }
        
        private void ChangeStatsOfTarget(BoardUnitModel unit)
        {
            if (StatType != Enumerators.Stat.UNDEFINED)
            {
                switch (StatType)
                {
                    case Enumerators.Stat.DEFENSE:
                        unit.BuffedDefense += Value;
                        unit.CurrentDefense += Value;
                        break;
                    case Enumerators.Stat.DAMAGE:
                        unit.BuffedDamage += Value;
                        unit.CurrentDamage += Value;
                        break;
                }
            }
            else
            {
                unit.BuffedDefense += Defense;
                unit.CurrentDefense += Defense;
                unit.BuffedDamage += Attack;
                unit.CurrentDamage += Attack;
            }
        }
    }
}
