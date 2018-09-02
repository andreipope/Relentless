using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class AddCardByNameToHandAbility : AbilityBase
    {
        public string Name;

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

            if (Name != "Corrupted Goo" && Name != "Tainted Goo" || (Name == "Corrupted Goo" || Name == "Tainted Goo") && CardOwnerOfAbility.CardSetType == PlayerCallerOfAbility.SelfHero.HeroElement)
            {
                CardsController.CreateNewCardByNameAndAddToHand(PlayerCallerOfAbility, Name);
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }
    }
}
