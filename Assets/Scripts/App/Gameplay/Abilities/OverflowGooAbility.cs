using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class OverflowGooAbility : AbilityBase
    {
        public int Value;

        public OverflowGooAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
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

            if (CardOwnerOfAbility.CardSetType == PlayerCallerOfAbility.SelfHero.HeroElement ||
                CardOwnerOfAbility.Name.Equals("Corrupted Goo") || CardOwnerOfAbility.Name.Equals("Tainted Goo"))
            {
                PlayerCallerOfAbility.Goo += Value;
            }
        }
    }
}
