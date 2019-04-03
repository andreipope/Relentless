using Loom.ZombieBattleground.Common;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class AbilityData
    {
        [JsonProperty]
        public Enumerators.AbilityType Ability { get; private set; }

        [JsonProperty]
        public Enumerators.GameMechanicDescription GameMechanicDescription { get; private set; }

        [JsonProperty]
        public List<TriggerInfo> Triggers { get; private set; }

        [JsonProperty]
        public List<TargetInfo> Targets { get; private set; }

        [JsonProperty]
        public List<GenericParameter> GenericParameters { get; private set; }

        [JsonProperty]
        public List<VfxParameter> VfxParameters { get; private set; }
        
        [JsonConstructor]
        public AbilityData(
            Enumerators.AbilityType ability,
            Enumerators.GameMechanicDescription gameMechanicDescription,
            List<TriggerInfo> triggers,
            List<TargetInfo> targets,
            List<GenericParameter> genericParameters,
            List<VfxParameter> vfxParameters)
        {
            Ability = ability;
            GameMechanicDescription = gameMechanicDescription;
            Triggers = triggers;
            Targets = targets;
            GenericParameters = genericParameters;
            VfxParameters = vfxParameters;
        }

        public AbilityData(AbilityData source)
        {
            Ability = source.Ability;
            GameMechanicDescription = source.GameMechanicDescription;
            Triggers = source.Triggers;
            Targets = source.Targets;
            GenericParameters = source.GenericParameters;
            VfxParameters = source.VfxParameters;
        }

        public class TargetInfo
        {
            [JsonProperty]
            public Enumerators.Target Target { get; private set; }

            [JsonProperty]
            public Enumerators.TargetFilter TargetFilter { get; private set; }

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

        public class TriggerInfo
        {
            [JsonProperty]
            public Enumerators.AbilityTrigger Trigger { get; private set; }

            [JsonProperty]
            public List<Enumerators.AbilitySubTrigger> SubTriggers { get; private set; }

            [JsonConstructor]
            public TriggerInfo(
                Enumerators.AbilityTrigger trigger,
                List<Enumerators.AbilitySubTrigger> subTriggers)
            {
                Trigger = trigger;
                SubTriggers = subTriggers;
            }

            public TriggerInfo(TriggerInfo source)
            {
                Trigger = source.Trigger;
                SubTriggers = source.SubTriggers;
            }
        }
    }
}
