using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class AllyUnitsOfTypeInPlayGetStatsAbility : AbilityBase
    {
        public int Defense { get; }

        public int Damage { get; }

        public Enumerators.Faction Faction { get; }

        public AllyUnitsOfTypeInPlayGetStatsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Defense = ability.Defense;
            Damage = ability.Damage;
            Faction = ability.Faction;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            foreach (CardModel unit in PlayerCallerOfAbility.CardsOnBoard)
            {
                if (unit.Card.Prototype.Faction == Faction && unit != AbilityUnitOwner)
                {
                    unit.BuffedDamage += Damage;
                    unit.AddToCurrentDamageHistory(Damage, Enumerators.ReasonForValueChange.AbilityBuff);
                    
                    unit.BuffedDefense += Defense;
                    unit.AddToCurrentDefenseHistory(Defense, Enumerators.ReasonForValueChange.AbilityBuff);
                }
            }
        }
    }
}
