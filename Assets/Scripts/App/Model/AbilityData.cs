using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class AbilityData
    {
        public string BuffType;

        public string Type;

        public string ActivityType;

        public string CallType;

        public string TargetType;

        public string StatType;

        public string SetType;

        public string EffectType;

        public string CardType;

        public string UnitStatus;

        public string UnitType;

        [JsonIgnore]
        public Enumerators.AbilityType AbilityType;

        [JsonIgnore]
        public Enumerators.AbilityActivityType AbilityActivityType;

        [JsonIgnore]
        public Enumerators.AbilityCallType AbilityCallType;

        [JsonIgnore]
        public List<Enumerators.AbilityTargetType> AbilityTargetTypes;

        [JsonIgnore]
        public Enumerators.StatType AbilityStatType;

        [JsonIgnore]
        public Enumerators.SetType AbilitySetType;

        [JsonIgnore]
        public Enumerators.AbilityEffectType AbilityEffectType;

        [JsonIgnore]
        public Enumerators.AttackInfoType AttackInfoType;

        [JsonIgnore]
        public Enumerators.CardType TargetCardType;

        [JsonIgnore]
        public Enumerators.UnitStatusType TargetUnitStatusType;

        [JsonIgnore]
        public Enumerators.CardType TargetUnitType;

        public int Value;

        public int Damage;

        public int Health;

        public string AttackInfo;

        public string Name;

        public int Turns;

        public int Count;

        public int Delay;

        public List<VisualEffectInfo> VisualEffectsToPlay;

        public void ParseData()
        {
            AbilityType = Utilites.CastStringTuEnum<Enumerators.AbilityType>(Type);
            AbilityActivityType = Utilites.CastStringTuEnum<Enumerators.AbilityActivityType>(ActivityType);
            AbilityCallType = Utilites.CastStringTuEnum<Enumerators.AbilityCallType>(CallType);

            if (!string.IsNullOrEmpty(TargetType))
            {
                AbilityTargetTypes = Utilites.CastList<Enumerators.AbilityTargetType>(TargetType);
            }
            else
            {
                AbilityTargetTypes = new List<Enumerators.AbilityTargetType>();
            }

            if (!string.IsNullOrEmpty(StatType))
            {
                AbilityStatType = Utilites.CastStringTuEnum<Enumerators.StatType>(StatType);
            }
            else
            {
                AbilityStatType = Enumerators.StatType.NONE;
            }

            if (!string.IsNullOrEmpty(SetType))
            {
                AbilitySetType = Utilites.CastStringTuEnum<Enumerators.SetType>(SetType);
            }
            else
            {
                AbilitySetType = Enumerators.SetType.NONE;
            }

            if (!string.IsNullOrEmpty(EffectType))
            {
                AbilityEffectType = Utilites.CastStringTuEnum<Enumerators.AbilityEffectType>(EffectType);
            }
            else
            {
                AbilityEffectType = Enumerators.AbilityEffectType.NONE;
            }

            if (!string.IsNullOrEmpty(AttackInfo))
            {
                AttackInfoType = Utilites.CastStringTuEnum<Enumerators.AttackInfoType>(AttackInfo);
            }
            else
            {
                AttackInfoType = Enumerators.AttackInfoType.ANY;
            }

            if (!string.IsNullOrEmpty(CardType))
            {
                TargetCardType = Utilites.CastStringTuEnum<Enumerators.CardType>(CardType);
            }
            else
            {
                TargetCardType = Enumerators.CardType.NONE;
            }

            if (!string.IsNullOrEmpty(UnitStatus))
            {
                TargetUnitStatusType = Utilites.CastStringTuEnum<Enumerators.UnitStatusType>(UnitStatus);
            }
            else
            {
                TargetUnitStatusType = Enumerators.UnitStatusType.NONE;
            }

            if (!string.IsNullOrEmpty(UnitType))
            {
                TargetUnitType = Utilites.CastStringTuEnum<Enumerators.CardType>(UnitType);
            }
        }

        public bool HasVisualEffectType(Enumerators.VisualEffectType type)
        {
            if (VisualEffectsToPlay == null)
                return false;

            return VisualEffectsToPlay.Find(vfx => vfx.Type == type) != null;
        }

        public VisualEffectInfo GetVisualEffectByType(Enumerators.VisualEffectType type)
        {
            if (VisualEffectsToPlay == null) 
            {
                return new VisualEffectInfo()
                {
                    Path = string.Empty, 
                    Type = type
                };
            }

            return VisualEffectsToPlay.Find(vfx => vfx.Type == type);
        }

        public class VisualEffectInfo
        {
            public Enumerators.VisualEffectType Type;
            public string Path;
        }
    }
}
