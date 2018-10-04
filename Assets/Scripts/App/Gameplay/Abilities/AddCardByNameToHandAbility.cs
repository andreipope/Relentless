using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AddCardByNameToHandAbility : AbilityBase
    {
        public string Name { get; }

        public AddCardByNameToHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Name = ability.Name;
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

            if (Name != "Corrupted Goo" && Name != "Tainted Goo" ||
                (Name == "Corrupted Goo" || Name == "Tainted Goo") &&
                CardOwnerOfAbility.CardSetType == PlayerCallerOfAbility.SelfHero.HeroElement)
            {
                CardsController.CreateNewCardByNameAndAddToHand(PlayerCallerOfAbility, Name);

                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Card);
            }
        }
    }
}
