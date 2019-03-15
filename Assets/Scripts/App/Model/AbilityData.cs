using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class AbilityData
    {
        [JsonProperty("type")]
        public Enumerators.AbilityType Ability { get; private set; }

        [JsonProperty("activity_type")]
        public Enumerators.AbilityActivity Activity { get; private set; }

        [JsonProperty("call_type")]
        public Enumerators.AbilityTrigger Trigger { get; private set; }

        [JsonProperty("target_type")]
        public List<Enumerators.Target> AbilityTarget { get; private set; }

        [JsonProperty("stat_type")]
        public Enumerators.Stat Stat { get; private set; }

        [JsonProperty("set_type")]
        public Enumerators.Faction Faction { get; private set; }

        [JsonProperty("effect_type")]
        public Enumerators.AbilityEffect Effect { get; private set; }

        [JsonProperty("attack_restriction")]
        public Enumerators.AttackRestriction AttackRestriction { get; private set; }

        [JsonProperty("card_type")]
        public Enumerators.CardType TargetCardType { get; private set; }

        [JsonProperty("unit_status")]
        public Enumerators.UnitStatus TargetUnitStatus { get; private set; }

        [JsonProperty("unit_type")]
        public Enumerators.CardType TargetUnitType { get; private set; }

        [JsonProperty("value")]
        public int Value { get; private set; }

        [JsonProperty("damage")]
        public int Damage { get; private set; }

        [JsonProperty("health")]
        public int Defense { get; private set; }

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
        public Enumerators.GameMechanicDescription GameMechanicDescription { get; private set; }

        [JsonProperty("target_set")]
        public Enumerators.Faction TargetFaction { get; private set; }

        [JsonProperty("sub_trigger")]
        public Enumerators.AbilitySubTrigger SubTrigger { get; private set; }

        [JsonProperty("choosable_abilities")]
        public List<ChoosableAbility> ChoosableAbilities { get; private set; }

        [JsonProperty("defense")]
        public int Defense2 { get; private set; }

        [JsonProperty("cost")]
        public int Cost { get; private set; }

        [JsonConstructor]
        public AbilityData(
            Enumerators.AbilityType ability,
            Enumerators.AbilityActivity activity,
            Enumerators.AbilityTrigger trigger,
            List<Enumerators.Target> abilityTarget,
            Enumerators.Stat stat,
            Enumerators.Faction faction,
            Enumerators.AbilityEffect effect,
            Enumerators.AttackRestriction attackRestriction,
            Enumerators.CardType targetCardType,
            Enumerators.UnitStatus targetUnitStatus,
            Enumerators.CardType targetUnitType,
            int value,
            int damage,
            int defense,
            string name,
            int turns,
            int count,
            int delay,
            List<VisualEffectInfo> visualEffectsToPlay,
            Enumerators.GameMechanicDescription gameMechanicDescription,
            Enumerators.Faction targetFaction,
            Enumerators.AbilitySubTrigger abilitySubTrigger,
            List<ChoosableAbility> choosableAbilities,
            int defense2,
            int cost)
        {
            Ability = ability;
            Activity = activity;
            Trigger = trigger;
            AbilityTarget = abilityTarget ?? new List<Enumerators.Target>();
            Stat = stat;
            Faction = faction;
            Effect = effect;
            AttackRestriction = attackRestriction;
            TargetCardType = targetCardType;
            TargetUnitStatus = targetUnitStatus;
            TargetUnitType = targetUnitType;
            Value = value;
            Damage = damage;
            Defense = defense;
            Name = name;
            Turns = turns;
            Count = count;
            Delay = delay;
            VisualEffectsToPlay = visualEffectsToPlay ?? new List<VisualEffectInfo>();
            GameMechanicDescription = gameMechanicDescription;
            TargetFaction = targetFaction;
            SubTrigger = abilitySubTrigger;
            ChoosableAbilities = choosableAbilities ?? new List<ChoosableAbility>();
            Defense2 = defense2;
            Cost = cost;
        }

        public AbilityData(AbilityData source) : this (
            source.Ability,
            source.Activity,
            source.Trigger,
            source.AbilityTarget.ToList(),
            source.Stat,
            source.Faction,
            source.Effect,
            source.AttackRestriction,
            source.TargetCardType,
            source.TargetUnitStatus,
            source.TargetUnitType,
            source.Value,
            source.Damage,
            source.Defense,
            source.Name,
            source.Turns,
            source.Count,
            source.Delay,
            source.VisualEffectsToPlay.Select(v => new VisualEffectInfo(v)).ToList(),
            source.GameMechanicDescription,
            source.TargetFaction,
            source.SubTrigger,
            source.ChoosableAbilities.Select(a => new ChoosableAbility(a)).ToList(),
            source.Defense2,
            source.Cost
        ) 
        {
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
            return $"({nameof(Ability)}: {Ability}, {nameof(Trigger)}: {Trigger}, {nameof(Name)}: {Name})";
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

            [JsonProperty("attribute")]
            public string Attribute { get; private set; }

            [JsonConstructor]
            public ChoosableAbility(string description, AbilityData abilityData, string attribute)
            {
                Description = description;
                AbilityData = abilityData;
                Attribute = attribute;
            }

            public ChoosableAbility(ChoosableAbility source)
            {
                Description = source.Description;
                Attribute = source.Attribute;
                AbilityData = new AbilityData(source.AbilityData);
            }
        }
    }
}
