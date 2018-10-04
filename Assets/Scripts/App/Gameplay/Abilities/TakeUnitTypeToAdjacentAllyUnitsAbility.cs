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

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            Player opponent = GetOpponentOverlord();

            int targetIndex = -1;
            for (int i = 0; i < opponent.BoardCards.Count; i++)
            {
                if (opponent.BoardCards[i].Model == TargetUnit)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    TakeTypeToUnit(opponent.BoardCards[targetIndex - 1]);
                }

                if (targetIndex + 1 < opponent.BoardCards.Count)
                {
                    TakeTypeToUnit(opponent.BoardCards[targetIndex + 1]);
                }
            }
        }

        private void TakeTypeToUnit(BoardUnitView unit)
        {
            if (unit == null)
                return;

            // implement functionality for animations
            switch (CardType)
            {
                case Enumerators.CardType.HEAVY:
                    unit.Model.SetAsHeavyUnit();
                    break;
                case Enumerators.CardType.FERAL:
                    unit.Model.SetAsFeralUnit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(CardType), CardType, null);
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
            {
                 unit.Model
            }, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }
    }
}
