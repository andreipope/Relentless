using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class AbilityData
    {
        [JsonProperty("type")]
        public Enumerators.AbilityType AbilityType { get; private set; }

        [JsonProperty("activity_type")]
        public Enumerators.AbilityActivityType ActivityType { get; private set; }

        [JsonProperty("call_type")]
        public Enumerators.AbilityCallType CallType { get; private set; }

        [JsonProperty("target_type")]
        public List<Enumerators.AbilityTargetType> AbilityTargetTypes { get; private set; }

        [JsonProperty("stat_type")]
        public Enumerators.StatType AbilityStatType { get; private set; }

        [JsonProperty("set_type")]
        public Enumerators.SetType AbilitySetType { get; private set; }

        [JsonProperty("effect_type")]
        public Enumerators.AbilityEffectType AbilityEffectType { get; private set; }

        [JsonProperty("attack_restriction")]
        public Enumerators.AttackRestriction AttackRestriction { get; private set; }

        [JsonProperty("card_type")]
        public Enumerators.CardType TargetCardType { get; private set; }

        [JsonProperty("unit_status")]
        public Enumerators.UnitStatusType TargetUnitStatusType { get; private set; }

        [JsonProperty("unit_type")]
        public Enumerators.CardType TargetUnitType { get; private set; }

        [JsonProperty("value")]
        public int Value { get; private set; }

        [JsonProperty("damage")]
        public int Damage { get; private set; }

        [JsonProperty("health")]
        public int Health { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("turns")]
        public int Turns { get; private set; }

        [JsonProperty("count")]
        public int Count { get; private set; }

        [JsonProperty("delay")]
        public int Delay { get; private set; }

        [JsonProperty("VisualEffectsToPlay")]
        public List<VisualEffectInfo> VisualEffectsToPlay { get; private set; }

        [JsonProperty("mechanic_description_type")]
        public Enumerators.GameMechanicDescriptionType GameMechanicDescriptionType { get; private set; }

        [JsonProperty("target_set")]
        public Enumerators.SetType TargetSetType { get; private set; }

        [JsonProperty("sub_trigger")]
        public Enumerators.AbilitySubTrigger AbilitySubTrigger { get; private set; }

        [JsonProperty("choosable_abilities")]
        public List<ChoosableAbility> ChoosableAbilities { get; private set; }

        [JsonProperty("defense")]
        public int Defense { get; private set; }

        [JsonProperty("cost")]
        public int Cost { get; private set; }

        [JsonConstructor]
        public AbilityData(
            Enumerators.AbilityType abilityType,
            Enumerators.AbilityActivityType activityType,
            Enumerators.AbilityCallType callType,
            List<Enumerators.AbilityTargetType> abilityTargetTypes,
            Enumerators.StatType abilityStatType,
            Enumerators.SetType abilitySetType,
            Enumerators.AbilityEffectType abilityEffectType,
            Enumerators.AttackRestriction attackRestriction,
            Enumerators.CardType targetCardType,
            Enumerators.UnitStatusType targetUnitStatusType,
            Enumerators.CardType targetUnitType,
            int value,
            int damage,
            int health,
            string name,
            int turns,
            int count,
            int delay,
            List<VisualEffectInfo> visualEffectsToPlay,
            Enumerators.GameMechanicDescriptionType gameMechanicDescriptionType,
            Enumerators.SetType targetSetType,
            Enumerators.AbilitySubTrigger abilitySubTrigger,
            List<ChoosableAbility> choosableAbilities,
            int defense,
            int cost)
        {
            AbilityType = abilityType;
            ActivityType = activityType;
            CallType = callType;
            AbilityTargetTypes = abilityTargetTypes ?? new List<Enumerators.AbilityTargetType>();
            AbilityStatType = abilityStatType;
            AbilitySetType = abilitySetType;
            AbilityEffectType = abilityEffectType;
            AttackRestriction = attackRestriction;
            TargetCardType = targetCardType;
            TargetUnitStatusType = targetUnitStatusType;
            TargetUnitType = targetUnitType;
            Value = value;
            Damage = damage;
            Health = health;
            Name = name;
            Turns = turns;
            Count = count;
            Delay = delay;
            VisualEffectsToPlay = visualEffectsToPlay ?? new List<VisualEffectInfo>();
            GameMechanicDescriptionType = gameMechanicDescriptionType;
            TargetSetType = targetSetType;
            AbilitySubTrigger = abilitySubTrigger;
            ChoosableAbilities = choosableAbilities ?? new List<ChoosableAbility>();
            Defense = defense;
            Cost = cost;
        }

        public AbilityData(AbilityData source) :
            this(
                source.AbilityType,
                source.ActivityType,
                source.CallType,
                source.AbilityTargetTypes.ToList(),
                source.AbilityStatType,
                source.AbilitySetType,
                source.AbilityEffectType,
                source.AttackRestriction,
                source.TargetCardType,
                source.TargetUnitStatusType,
                source.TargetUnitType,
                source.Value,
                source.Damage,
                source.Health,
                source.Name,
                source.Turns,
                source.Count,
                source.Delay,
                source.VisualEffectsToPlay.Select(v => new VisualEffectInfo(v)).ToList(),
                source.GameMechanicDescriptionType,
                source.TargetSetType,
                source.AbilitySubTrigger,
                source.ChoosableAbilities.Select(a => new ChoosableAbility(a)).ToList(),
                source.Defense,
                source.Cost
            ) {
        }

        public bool HasVisualEffectType(Enumerators.VisualEffectType type)
        {
            return GetVisualEffectByType(type) != null;
        }

        public VisualEffectInfo GetVisualEffectByType(Enumerators.VisualEffectType type)
        {
            return VisualEffectsToPlay.Find(vfx => vfx.Type == type);
        }

        public bool HasChoosableAbilities()
        {
            return ChoosableAbilities != null && ChoosableAbilities.Count > 1;
        }

        public override string ToString() {
            return $"({nameof(AbilityType)}: {AbilityType}, {nameof(CallType)}: {CallType}, {nameof(Name)}: {Name})";
        }

        public class VisualEffectInfo
        {
            [JsonProperty("Type")]
            public Enumerators.VisualEffectType Type { get; private set; }

            [JsonProperty("Path")]
            public string Path { get; private set; }

            [JsonConstructor]
            public VisualEffectInfo(Enumerators.VisualEffectType type, string path)
            {
                Type = type;
                Path = path;
            }

            public VisualEffectInfo(VisualEffectInfo source)
            {
                Type = source.Type;
                Path = source.Path;
            }

            public void ForceSetPath(string path)
            {
                Path = path;
            }

            public void ForceSetType(Enumerators.VisualEffectType type)
            {
                Type = type;
            }
        }

        public class ChoosableAbility
        {
            [JsonProperty("description")]
            public string Description { get; private set; }

            [JsonProperty("ability_data")]
            public AbilityData AbilityData { get; private set; }

            [JsonConstructor]
            public ChoosableAbility(string description, AbilityData abilityData)
            {
                Description = description;
                AbilityData = abilityData;
            }

            public ChoosableAbility(ChoosableAbility source)
            {
                Description = source.Description;
                AbilityData = new AbilityData(source.AbilityData);
            }
        }
    }
}
