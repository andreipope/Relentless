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
                        GetParameters(out int defense, out int attack);
                        ChangeStatsOfPlayerAllyCards(defense, attack, false);
                    }
                }
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                GetParameters(out int defense, out int attack);
                ChangeStatsOfTarget(TargetUnit, defense, attack);

                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });
            }
        }

        protected override void UnitAttackedHandler(IBoardObject info, int damage, bool isAttacker)
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

        protected override void ChangeAuraStatusAction(bool status)
        {
            base.ChangeAuraStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            if (status)
            {
                ChangeStatsOfPlayerAllyCards(Defense, Attack, false);
            }
            else
            {
                ChangeStatsOfPlayerAllyCards(-Defense, -Attack, false);
            }
        }

        private void ChangeStatsToItself()
        {
            GetParameters(out int defense, out int attack);
            ChangeStatsOfTarget(AbilityUnitOwner, defense, attack);
        }

        private void ChangeStatsOfPlayerAllyCards(int defense, int damage, bool withCaller = false)
        {
            foreach (CardModel unit in PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard)
            {
                if (!withCaller && unit.Card == CardModel.Card)
                    continue;

                ChangeStatsOfTarget(unit, defense, damage);
            }
        }
        
        private void ChangeStatsOfTarget(CardModel unit, int defense, int damage)
        {
            unit.BuffedDefense += defense;
            unit.CurrentDefense += defense;
            unit.BuffedDamage += damage;
            unit.CurrentDamage += damage;
        }

        private void GetParameters(out int defense, out int attack)
        {
            if(StatType != Enumerators.Stat.UNDEFINED)
            {
                defense = Value;
                attack = Value;
            }
            else
            {
                defense = Defense;
                attack = Attack;
            }
        }
    }
}
