// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class TakeUnitTypeToAdjacentAllyUnitsAbility : AbilityBase
    {
        public Enumerators.CardType cardType;

        public TakeUnitTypeToAdjacentAllyUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            cardType = ability.targetCardType;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.ENTRY)

                return;

            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            Player opponent = GetOpponentOverlord();
            object caller = abilityUnitOwner != null?abilityUnitOwner:(object)boardSpell;

            int targetIndex = -1;
            for (int i = 0; i < opponent.BoardCards.Count; i++)
            {
                if (opponent.BoardCards[i] == targetUnit)
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

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }

        private void TakeTypeToUnit(BoardUnit unit)
        {
            if (unit == null)
                return;

            // implement functionality for animations
            switch (cardType)
            {
                case Enumerators.CardType.HEAVY:
                    unit.hasHeavy = true;
                    break;
                case Enumerators.CardType.FERAL:
                    unit.hasFeral = true;
                    break;
            }
        }
    }
}
