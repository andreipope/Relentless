using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class AbilityData
    {
        [JsonProperty]
        public Enumerators.AbilityType Ability { get; private set; }

        [JsonProperty]
        public Enumerators.AbilityActivity Activity { get; private set; }

        [JsonProperty]
        public Enumerators.AbilityTrigger Trigger { get; private set; }

        [JsonProperty]
        public List<Enumerators.Target> Targets { get; private set; }

        [JsonProperty]
        public Enumerators.Stat Stat { get; private set; }

        [JsonProperty]
        public Enumerators.Faction Faction { get; private set; }

        [JsonProperty]
        public Enumerators.AbilityEffect Effect { get; private set; }

        [JsonProperty]
        public Enumerators.AttackRestriction AttackRestriction { get; private set; }

        [JsonProperty]
        public Enumerators.CardType TargetCardType { get; private set; }

        [JsonProperty]
        public Enumerators.UnitSpecialStatus TargetUnitSpecialStatus { get; private set; }

        [JsonProperty]
        public Enumerators.CardType TargetUnitType { get; private set; }

        [JsonProperty]
        public int Value { get; private set; }

        [JsonProperty]
        public string Name { get; private set; }
        
        [JsonProperty]
        public int Damage { get; private set; }

        [JsonProperty]
        public int Defense { get; private set; }

        [JsonProperty]
        public int Turns { get; private set; }

        [JsonProperty]
        public int Count { get; private set; }

        [JsonProperty]
        public int Delay { get; private set; }

        [JsonProperty]
        public List<VisualEffectInfo> VisualEffectsToPlay { get; private set; }

        [JsonProperty]
        public Enumerators.GameMechanicDescription GameMechanicDescription { get; private set; }

        [JsonProperty]
        public Enumerators.Faction TargetFaction { get; private set; }

        [JsonProperty]
        public Enumerators.AbilitySubTrigger SubTrigger { get; private set; }

        [JsonProperty]
        public List<ChoosableAbility> ChoosableAbilities { get; private set; }

        [JsonProperty]
        public int Defense2 { get; private set; }

        [JsonProperty]
        public int Cost { get; private set; }

        [JsonProperty]
        public Enumerators.CardKind TargetKind { get; private set; }

        [JsonProperty]
        public List<Enumerators.GameMechanicDescription> TargetGameMechanicDescriptions { get; private set; }

        [JsonProperty]
        public MouldId MouldId { get; private set; }

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
            Enumerators.CardKind targetCardkind,
            List<Enumerators.GameMechanicDescription> targetGameMechanicDescriptionTypes,
            MouldId mouldId
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
            TargetKind = targetCardkind;
            TargetGameMechanicDescriptions = targetGameMechanicDescriptionTypes;
            MouldId = mouldId;
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
            source.TargetGameMechanicDescriptions,
            source.MouldId
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
            public Enumerators.VisualEffectType Type { get; private set; }

            [JsonProperty]
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
            [JsonProperty]
            public string Description { get; private set; }

            [JsonProperty]
            public AbilityData AbilityData { get; private set; }

            [JsonProperty]
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
