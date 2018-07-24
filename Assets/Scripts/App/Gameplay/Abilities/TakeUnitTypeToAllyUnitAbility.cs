// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class TakeUnitTypeToAllyUnitAbility : AbilityBase
    {
        public Enumerators.CardType cardType;

        public TakeUnitTypeToAllyUnitAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            cardType = ability.targetCardType;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.AT_START)
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

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                TakeTypeToUnit(targetUnit);
            }
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
