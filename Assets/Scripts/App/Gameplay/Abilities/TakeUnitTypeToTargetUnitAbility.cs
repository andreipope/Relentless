using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TakeUnitTypeToTargetUnitAbility : AbilityBase
    {
        public Enumerators.CardType UnitType;

        public int Count { get; }

        public TakeUnitTypeToTargetUnitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            UnitType = ability.TargetUnitType;
            Count = Mathf.Clamp(ability.Count, 1, ability.Count);
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger == Enumerators.AbilityTrigger.ENTRY && AbilityActivity == Enumerators.AbilityActivity.PASSIVE)
            {
                HandleTargets();
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                TakeTypeToUnits(new List<CardModel>() { TargetUnit });
                InvokeUseAbilityEvent(
                    new List<ParametrizedAbilityBoardObject>
                    {
                        new ParametrizedAbilityBoardObject(TargetUnit)
                    }
                );
            }
        }

        private void HandleTargets()
        {
            List<CardModel> units;

            if (PredefinedTargets != null)
            {
                units = PredefinedTargets.Select(x => x.BoardObject).Cast<CardModel>().ToList();
            }
            else
            {
                units = PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard
               .Where(unit => unit != AbilityUnitOwner && !unit.HasFeral && unit.NumTurnsOnBoard == 0)
               .ToList();

                if (AbilityData.SubTrigger != Enumerators.AbilitySubTrigger.AllOtherAllyUnitsInPlay)
                {
                    units = InternalTools.GetRandomElementsFromList(units, Count);
                }
            }

            if (units.Count > 0)
            {
                TakeTypeToUnits(units);

                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>(
                    units.Select(item => new ParametrizedAbilityBoardObject(item)))
                );
            }
        }

        private void TakeTypeToUnits(List<CardModel> units)
        {
            foreach (CardModel unit in units)
            {
                switch (UnitType)
                {
                    case Enumerators.CardType.HEAVY:
                        unit.SetAsHeavyUnit();
                        break;
                    case Enumerators.CardType.FERAL:
                        unit.SetAsFeralUnit();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(UnitType), UnitType, null);
                }
            }

            PostGameActionReport(units);
        }

        private void PostGameActionReport(List<CardModel> targets)
        {
            Enumerators.ActionEffectType effectType = Enumerators.ActionEffectType.None;

            if (UnitType == Enumerators.CardType.FERAL)
            {
                effectType = Enumerators.ActionEffectType.Feral;
            }
            else if (UnitType == Enumerators.CardType.HEAVY)
            {
                effectType = Enumerators.ActionEffectType.Heavy;
            }

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (CardModel target in targets)
            {
                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = effectType,
                    Target = target
                });
            }

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = AbilityUnitOwner,
                TargetEffects = TargetEffects
            });
        }
    }
}
