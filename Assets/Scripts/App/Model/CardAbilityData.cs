using Loom.ZombieBattleground.Common;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class CardAbilityData
    {
        [JsonProperty]
        public Enumerators.AbilityType Ability { get; protected set; }

        [JsonProperty]
        public Enumerators.GameMechanicDescription GameMechanicDescription { get; protected set; }

        [JsonProperty]
        public List<Enumerators.AbilityTrigger> Triggers { get; protected set; }

        [JsonProperty]
        public List<TargetInfo> Targets { get; protected set; }

        [JsonProperty]
        public List<GenericParameter> GenericParameters { get; protected set; }

        [JsonConstructor]
        public CardAbilityData(
            Enumerators.AbilityType ability,
            Enumerators.GameMechanicDescription gameMechanicDescription,
            List<Enumerators.AbilityTrigger> triggers,
            List<TargetInfo> targets,
            List<GenericParameter> genericParameters)
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
            [JsonProperty]
            public Enumerators.Target Target { get; protected set; }

            [JsonProperty]
            public Enumerators.TargetFilter TargetFilter { get; protected set; }

            [JsonConstructor]
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
