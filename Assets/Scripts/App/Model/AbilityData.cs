using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class AbilityData
    {
        [JsonProperty("Type")]
        public Enumerators.AbilityType AbilityType { get; private set; }

        [JsonProperty("ActivityType")]
        public Enumerators.AbilityActivityType ActivityType { get; private set; }

        [JsonProperty("CallType")]
        public Enumerators.AbilityCallType CallType { get; private set; }

        [JsonProperty("TargetType")]
        public List<Enumerators.AbilityTargetType> AbilityTargetTypes { get; private set; }

        [JsonProperty("StatType")]
        public Enumerators.StatType AbilityStatType { get; private set; }

        [JsonProperty("SetType")]
        public Enumerators.SetType AbilitySetType { get; private set; }

        [JsonProperty("EffectType")]
        public Enumerators.AbilityEffectType AbilityEffectType { get; private set; }

        [JsonProperty("AttackRestriction")]
        public Enumerators.AttackRestriction AttackRestriction { get; private set; }

        [JsonProperty("CardType")]
        public Enumerators.CardType TargetCardType { get; private set; }

        [JsonProperty("UnitStatus")]
        public Enumerators.UnitStatusType TargetUnitStatusType { get; private set; }

        [JsonProperty("UnitType")]
        public Enumerators.CardType TargetUnitType { get; private set; }

        public int Value { get; private set; }

        public int Damage { get; private set; }

        public int Health { get; private set; }

        public string Name { get; private set; }

        public int Turns { get; private set; }

        public int Count { get; private set; }

        public int Delay { get; private set; }

        public List<VisualEffectInfo> VisualEffectsToPlay { get; private set; }

        [JsonProperty("MechanicPicture")]
        public Enumerators.GameMechanicDescriptionType GameMechanicDescriptionType { get; private set; }

        [JsonProperty("TargetSet")]
        public Enumerators.SetType TargetSetType { get; private set; }

        [JsonProperty("SubTrigger")]
        public Enumerators.AbilitySubTrigger SubTrigger { get; private set; }

        public List<ChoosableAbility> ChoosableAbilities { get; private set; }

        public int Defense { get; private set; }

        public int Cost { get; private set; }

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
            Enumerators.AbilitySubTrigger subTrigger,
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
            SubTrigger = subTrigger;
            ChoosableAbilities = choosableAbilities ?? new List<ChoosableAbility>();
            Defense = defense;
            Cost = cost;
        }

        public AbilityData(AbilityData source)
        {
            AbilityType = source.AbilityType;
            ActivityType = source.ActivityType;
            CallType = source.CallType;
            AbilityTargetTypes = source.AbilityTargetTypes.ToList();
            AbilityStatType = source.AbilityStatType;
            AbilitySetType = source.AbilitySetType;
            AbilityEffectType = source.AbilityEffectType;
            AttackRestriction = source.AttackRestriction;
            TargetCardType = source.TargetCardType;
            TargetUnitStatusType = source.TargetUnitStatusType;
            TargetUnitType = source.TargetUnitType;
            Value = source.Value;
            Damage = source.Damage;
            Health = source.Health;
            Name = source.Name;
            Turns = source.Turns;
            Count = source.Count;
            Delay = source.Delay;
            VisualEffectsToPlay = source.VisualEffectsToPlay.Select(v => new VisualEffectInfo(v)).ToList();
            GameMechanicDescriptionType = source.GameMechanicDescriptionType;
            TargetSetType = source.TargetSetType;
            SubTrigger = source.SubTrigger;
            ChoosableAbilities = source.ChoosableAbilities.Select(a => new ChoosableAbility(a)).ToList();
            Defense = source.Defense;
            Cost = source.Cost;
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

        public class VisualEffectInfo
        {
            public Enumerators.VisualEffectType Type { get; private set; }
            public string Path { get; private set; }

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
        }

        public class ChoosableAbility
        {
            public string Description { get; private set; }
            public AbilityData AbilityData { get; private set; }

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
