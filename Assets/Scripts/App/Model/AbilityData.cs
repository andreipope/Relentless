using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class AbilityData
    {
        [JsonProperty]
        public Enumerators.AbilityType Ability { get; }

        [JsonProperty]
        public Enumerators.AbilityActivity Activity { get; }

        [JsonProperty]
        public Enumerators.AbilityTrigger Trigger { get; }

        [JsonProperty]
        public List<Enumerators.Target> Targets { get; }

        [JsonProperty]
        public Enumerators.Stat Stat { get; }

        [JsonProperty]
        public Enumerators.Faction Faction { get; }

        [JsonProperty]
        public Enumerators.AbilityEffect Effect { get; }

        [JsonProperty]
        public Enumerators.AttackRestriction AttackRestriction { get; }

        [JsonProperty]
        public Enumerators.CardType TargetCardType { get; }

        [JsonProperty]
        public Enumerators.UnitSpecialStatus TargetUnitSpecialStatus { get;  }

        [JsonProperty]
        public Enumerators.CardType TargetUnitType { get; }

        [JsonProperty]
        public int Value { get; }

        [JsonProperty]
        public string Name { get; }
        
        [JsonProperty]
        public int Damage { get; }

        [JsonProperty]
        public int Defense { get; }

        [JsonProperty]
        public int Turns { get; }

        [JsonProperty]
        public int Count { get; }

        [JsonProperty]
        public int Delay { get; }

        [JsonProperty]
        public List<VisualEffectInfo> VisualEffectsToPlay { get; }

        [JsonProperty]
        public Enumerators.GameMechanicDescription GameMechanicDescription { get; }

        [JsonProperty]
        public Enumerators.Faction TargetFaction { get; }

        [JsonProperty]
        public Enumerators.AbilitySubTrigger SubTrigger { get; }

        [JsonProperty]
        public List<ChoosableAbility> ChoosableAbilities { get; }

        [JsonProperty]
        public int Defense2 { get; }

        [JsonProperty]
        public int Cost { get; }

        [JsonProperty("targetCardKind")]
        public Enumerators.CardKind TargetKind { get; private set; }

        [JsonProperty]
        public List<Enumerators.GameMechanicDescription> TargetGameMechanicDescriptions { get; private set; }

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
            Enumerators.UnitSpecialStatus targetUnitSpecialStatus,
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
            int cost,
            Enumerators.CardKind targetKind,
            List<Enumerators.GameMechanicDescription> targetGameMechanicDescriptionTypes
            )
        {
            Ability = ability;
            Activity = activity;
            Trigger = trigger;
            Targets = abilityTarget ?? new List<Enumerators.Target>();
            Stat = stat;
            Faction = faction;
            Effect = effect;
            AttackRestriction = attackRestriction;
            TargetCardType = targetCardType;
            TargetUnitSpecialStatus = targetUnitSpecialStatus;
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
            TargetKind = targetKind;
            TargetGameMechanicDescriptions = targetGameMechanicDescriptionTypes;
        }

        public AbilityData(AbilityData source) : this (
            source.Ability,
            source.Activity,
            source.Trigger,
            source.Targets.ToList(),
            source.Stat,
            source.Faction,
            source.Effect,
            source.AttackRestriction,
            source.TargetCardType,
            source.TargetUnitSpecialStatus,
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
            source.Cost,
            source.TargetKind,
            source.TargetGameMechanicDescriptions
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
            [JsonProperty]
            public Enumerators.VisualEffectType Type { get; }

            [JsonProperty]
            public string Path { get; }

            [JsonConstructor]
            public VisualEffectInfo(Enumerators.VisualEffectType type, string path)
            {
                Type = type;
                Path = path;
            }

            public VisualEffectInfo(VisualEffectInfo source) : this(
                source.Type,
                source.Path
                )
            {
            }
        }

        public class ChoosableAbility
        {
            [JsonProperty]
            public string Description { get; }

            [JsonProperty]
            public AbilityData AbilityData { get; }

            [JsonProperty]
            public string Attribute { get; }

            [JsonConstructor]
            public ChoosableAbility(string description, AbilityData abilityData, string attribute)
            {
                Description = description;
                AbilityData = abilityData;
                Attribute = attribute;
            }

            public ChoosableAbility(ChoosableAbility source) : this (
                source.Description,
                new AbilityData(source.AbilityData),
                source.Attribute
            )
            {
            }
        }
    }
}
