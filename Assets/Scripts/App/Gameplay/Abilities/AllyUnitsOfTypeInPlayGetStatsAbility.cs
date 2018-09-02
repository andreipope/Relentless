using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class AllyUnitsOfTypeInPlayGetStatsAbility : AbilityBase
    {
        public int health;

        public int damage;

        public Enumerators.SetType setType;

        public AllyUnitsOfTypeInPlayGetStatsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            health = ability.health;
            damage = ability.damage;
            setType = ability.abilitySetType;
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

            foreach (BoardUnit unit in playerCallerOfAbility.BoardCards)
            {
                if (unit.Card.libraryCard.cardSetType.Equals(setType) && (unit != abilityUnitOwner))
                {
                    unit.BuffedDamage += damage;
                    unit.CurrentDamage += damage;

                    unit.BuffedHP += health;
                    unit.CurrentHP += health;
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
    }
}
