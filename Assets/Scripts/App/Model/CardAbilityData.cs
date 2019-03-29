using Loom.ZombieBattleground.Common;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class CardAbilityData
    {
        public Enumerators.AbilityType Ability;
        public Enumerators.GameMechanicDescription GameMechanicDescription;


        public IReadOnlyList<Enumerators.AbilityTrigger> Triggers;
        public IReadOnlyList<TargetInfo> Targets;
        public IReadOnlyList<GenericParameter> GenericParameters;

        public CardAbilityData(
            Enumerators.AbilityType ability,
            Enumerators.GameMechanicDescription gameMechanicDescription,
            IReadOnlyList<Enumerators.AbilityTrigger> triggers,
            IReadOnlyList<TargetInfo> targets,
            IReadOnlyList<GenericParameter> genericParameters)
        {
            Ability = ability;
            GameMechanicDescription = gameMechanicDescription;
            Triggers = triggers;
            Targets = targets;
            GenericParameters = genericParameters;
        }

        public CardAbilityData(CardAbilityData source)
        {
            Ability = source.Ability;
            GameMechanicDescription = source.GameMechanicDescription;
            Triggers = source.Triggers;
            Targets = source.Targets;
            GenericParameters = source.GenericParameters;
        }

        public class TargetInfo
        {
            public Enumerators.Target Target;
            public Enumerators.TargetFilter TargetFilter;

            public TargetInfo(
                Enumerators.Target target,
                Enumerators.TargetFilter targetFilter)
            {
                Target = target;
                TargetFilter = targetFilter;
            }

            public TargetInfo(TargetInfo source)
            {
                Target = source.Target;
                TargetFilter = source.TargetFilter;
            }
        }
    }
}
