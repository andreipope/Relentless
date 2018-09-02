// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class AddCardByNameToHandAbility : AbilityBase
    {
        public string name;

        public AddCardByNameToHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            name = ability.name;
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

            if (((name != "Corrupted Goo") && (name != "Tainted Goo")) || (((name == "Corrupted Goo") || (name == "Tainted Goo")) && (cardOwnerOfAbility.cardSetType == playerCallerOfAbility.SelfHero.heroElement)))
            {
                _cardsController.CreateNewCardByNameAndAddToHand(playerCallerOfAbility, name);
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }
    }
}
