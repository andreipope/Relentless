// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class OverflowGooAbility : AbilityBase
    {
        public int value = 0;

        public OverflowGooAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
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
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if((cardOwnerOfAbility.cardSetType == playerCallerOfAbility.SelfHero.heroElement) || cardOwnerOfAbility.name.Equals("Corrupted Goo") || cardOwnerOfAbility.name.Equals("Tainted Goo"))
                playerCallerOfAbility.Goo += value;
        }
    }
}
