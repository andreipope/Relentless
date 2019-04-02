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
        public List<TriggerInfo> Triggers { get; protected set; }

        [JsonProperty]
        public List<TargetInfo> Targets { get; protected set; }

        [JsonProperty]
        public List<GenericParameter> GenericParameters { get; protected set; }

        [JsonProperty]
        public List<VfxParameter> VfxParameters { get; protected set; }
        
        [JsonConstructor]
        public CardAbilityData(
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

        public CardAbilityData(CardAbilityData source)
        {
            Ability = source.Ability;
            GameMechanicDescription = source.GameMechanicDescription;
            Triggers = source.Triggers;
            Targets = source.Targets;
            GenericParameters = source.GenericParameters;
            VfxParameters = source.VfxParameters;
        }

        public bool HasTrigger(Enumerators.AbilityTrigger trigger)
        {
            return Triggers.FindAll(trig => trig.Trigger == trigger).Count > 0;
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

        public class TriggerInfo
        {
            [JsonProperty]
            public Enumerators.AbilityTrigger Trigger { get; protected set; }

            [JsonProperty]
            public List<Enumerators.AbilitySubTrigger> SubTriggers { get; protected set; }

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
