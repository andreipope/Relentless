using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class TakeUnitTypeToAdjacentAllyUnitsAbility : AbilityBase
    {
        public Enumerators.CardType CardType;

        public TakeUnitTypeToAdjacentAllyUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            CardType = ability.TargetCardType;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();

            InvokeUseAbilityEvent();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            Enumerators.ActionEffectType effectType = Enumerators.ActionEffectType.None;

            if (CardType == Enumerators.CardType.FERAL)
            {
                effectType = Enumerators.ActionEffectType.Feral;
            }
            else if (CardType == Enumerators.CardType.HEAVY)
            {
                effectType = Enumerators.ActionEffectType.Heavy;
            }

            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            Player opponent = GetOpponentOverlord();

            int targetIndex = -1;
            for (int i = 0; i < opponent.CardsOnBoard.Count; i++)
            {
                if (opponent.CardsOnBoard[i] == AbilityUnitOwner)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    TakeTypeToUnit(opponent.CardsOnBoard[targetIndex - 1]);

                    targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = effectType,
                        Target = opponent.CardsOnBoard[targetIndex - 1]
                    });
                }

                if (targetIndex + 1 < opponent.CardsOnBoard.Count)
                {
                    TakeTypeToUnit(opponent.CardsOnBoard[targetIndex + 1]);

                    targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = effectType,
                        Target = opponent.CardsOnBoard[targetIndex + 1]
                    });
                }
            }

            if (targetEffects.Count > 0)
            {
                Enumerators.ActionType actionType = Enumerators.ActionType.CardAffectingMultipleCards;

                if (targetEffects.Count == 1)
                {
                    actionType = Enumerators.ActionType.CardAffectingCard;
                }

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = actionType,
                    Caller = AbilityUnitOwner,
                    TargetEffects = targetEffects
                });
            }
        }

        private void TakeTypeToUnit(BoardUnitModel unit)
        {
            if (unit == null)
                return;

            switch (CardType)
            {
                case Enumerators.CardType.HEAVY:
                    unit.SetAsHeavyUnit();
                    break;
                case Enumerators.CardType.FERAL:
                    unit.SetAsFeralUnit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(CardType), CardType, null);
            }
        }
    }
}
