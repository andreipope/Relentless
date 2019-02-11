using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStatAbility : AbilityBase
    {
        public Enumerators.StatType StatType { get; }

        public int Value { get; }

        public int Attack { get; }

        public int Defense { get; }

        public ChangeStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.AbilityStatType;
            Value = ability.Value;
            Attack = ability.Damage;
            Defense = ability.Defense;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
            {
                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Character);

                if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.AllOtherAllyUnitsInPlay)
                {
                    if (AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.ITSELF))
                    {
                        ChangeStatsToItself();
                    }
                    else if(AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.PLAYER_ALL_CARDS))
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

                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>() { TargetUnit }, AbilityData.AbilityType, Enumerators.AffectObjectType.Character);
            }
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            ChangeStatsToItself();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            ChangeStatsToItself();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.END)
                return;

            ChangeStatsToItself();
        }

        private void ChangeStatsToItself()
        {
            ChangeStatsOfTarget(AbilityUnitOwner);
        }

        private void ChangeStatsOfPlayerAllyCards(bool withCaller = false)
        {
            foreach (BoardUnitView unit in PlayerCallerOfAbility.BoardCards)
            {
                if (!withCaller && unit.Model.Card == MainWorkingCard)
                    continue;

                ChangeStatsOfTarget(unit.Model);
            }
        }

        private void ChangeStatsOfTarget(BoardUnitModel unit)
        {
            if (StatType != Enumerators.StatType.UNDEFINED)
            {
                switch (StatType)
                {
                    case Enumerators.StatType.HEALTH:
                        unit.BuffedHp += Value;
                        unit.CurrentHp += Value;
                        break;
                    case Enumerators.StatType.DAMAGE:
                        unit.BuffedDamage += Value;
                        unit.CurrentDamage += Value;
                        break;
                }
            }
            else
            {
                unit.BuffedHp += Defense;
                unit.CurrentHp += Defense;
                unit.BuffedDamage += Attack;
                unit.CurrentDamage += Attack;
            }
        }
    }
}
