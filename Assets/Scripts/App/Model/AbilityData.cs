using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class AbilityData
    {
        [JsonProperty("BuffType")]
        public Enumerators.BuffType BuffType;

        [JsonProperty("Type")]
        public Enumerators.AbilityType AbilityType;

        [JsonProperty("ActivityType")]
        public Enumerators.AbilityActivityType ActivityType;

        [JsonProperty("CallType")]
        public Enumerators.AbilityCallType CallType;

        [JsonProperty("TargetType")]
        public List<Enumerators.AbilityTargetType> AbilityTargetTypes;

        [JsonProperty("StatType")]
        public Enumerators.StatType AbilityStatType;

        [JsonProperty("SetType")]
        public Enumerators.SetType AbilitySetType;

        [JsonProperty("EffectType")]
        public Enumerators.AbilityEffectType AbilityEffectType;

        [JsonProperty("AttackInfo")]
        public Enumerators.AttackRestriction AttackRestriction;

        [JsonProperty("CardType")]
        public Enumerators.CardType TargetCardType;

        [JsonProperty("UnitStatus")]
        public Enumerators.UnitStatusType TargetUnitStatusType;

        [JsonProperty("UnitType")]
        public Enumerators.CardType TargetUnitType;

        public int Value;

        public int Damage;

        public int Health;

        public string Name;

        public int Turns;

        public int Count;

        public int Delay;

        public List<VisualEffectInfo> VisualEffectsToPlay;

        public AbilityData(
            Enumerators.BuffType buffType,
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
            List<VisualEffectInfo> visualEffectsToPlay)
        {
            BuffType = buffType;
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
        }

        public AbilityData(AbilityData source)
        {
            AbilityType = source.AbilityType;
            ActivityType = source.ActivityType;
            CallType = source.CallType;
            AbilityTargetTypes = new List<Enumerators.AbilityTargetType>(source.AbilityTargetTypes);
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
            VisualEffectsToPlay =
                source.VisualEffectsToPlay
                    .Select(v => new VisualEffectInfo(v))
                    .ToList();
        }

        public bool HasVisualEffectType(Enumerators.VisualEffectType type)
        {
            return GetVisualEffectByType(type) != null;
        }

        public VisualEffectInfo GetVisualEffectByType(Enumerators.VisualEffectType type)
        {
            return VisualEffectsToPlay.Find(vfx => vfx.Type == type);
        }

        public class VisualEffectInfo
        {
            public Enumerators.VisualEffectType Type;
            public string Path;

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
    }
}
