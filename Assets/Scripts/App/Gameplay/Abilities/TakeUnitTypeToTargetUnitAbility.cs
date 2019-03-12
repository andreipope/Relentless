using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class TakeUnitTypeToTargetUnitAbility : AbilityBase
    {
        public Enumerators.CardType UnitType;

        public TakeUnitTypeToTargetUnitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            UnitType = ability.TargetUnitType;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardUnitModel> allies;

            BoardUnitModel target = null;
            if (PredefinedTargets != null)
            {
                allies = PredefinedTargets.Select(x => x.BoardObject).Cast<BoardUnitModel>().ToList();

                if (allies.Count > 0)
                {
                    target = allies[0];
                }
            }
            else
            {
                allies = PlayerCallerOfAbility.CardsOnBoard
                    .Where(unit => unit != AbilityUnitOwner && !unit.HasFeral && unit.NumTurnsOnBoard == 0)
                    .ToList();

                if (allies.Count > 0)
                {
                    target = allies[Random.Range(0, allies.Count)];
                }
            }

            if (target == null)
            {
                TakeTypeToUnit(target);
            }
        }

        private void TakeTypeToUnit(BoardUnitModel unit)
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

            Enumerators.ActionEffectType effectType = Enumerators.ActionEffectType.None;

            if (UnitType == Enumerators.CardType.FERAL)
            {
                effectType = Enumerators.ActionEffectType.Feral;
            }
            else if (UnitType == Enumerators.CardType.HEAVY)
            {
                effectType = Enumerators.ActionEffectType.Heavy;
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = effectType,
                        Target = unit
                    }
                }
            });

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(unit)
                }
            );
        }
    }
}
