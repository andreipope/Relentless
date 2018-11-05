using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class DamageEnemyOrRestoreDefenseAllyAbility : AbilityBase
    {
        public int Health { get; }
        public int Damage { get; }

        public List<Enumerators.AbilityTargetType> TargetTypes { get; }

        public DamageEnemyOrRestoreDefenseAllyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Health = AbilityData.Health;
            Damage = AbilityData.Damage;
            TargetTypes = AbilityData.AbilityTargetTypes;
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Character);

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);
        }
    }
}
