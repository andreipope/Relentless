using System.Collections.Generic;
using Loom.Newtonsoft.Json;
using LoomNetwork.CZB.Common;
using LoomNetwork.Internal;

namespace LoomNetwork.CZB.Data
{
    public class AbilityData
    {
        public string buffType;

        public string type;

        public string activityType;

        public string callType;

        public string targetType;

        public string statType;

        public string setType;

        public string effectType;

        public string cardType;

        public string unitStatus;

        public string unitType;

        [JsonIgnore]
        public Enumerators.AbilityType abilityType;

        [JsonIgnore]
        public Enumerators.AbilityActivityType abilityActivityType;

        [JsonIgnore]
        public Enumerators.AbilityCallType abilityCallType;

        [JsonIgnore]
        public List<Enumerators.AbilityTargetType> abilityTargetTypes;

        [JsonIgnore]
        public Enumerators.StatType abilityStatType;

        [JsonIgnore]
        public Enumerators.SetType abilitySetType;

        [JsonIgnore]
        public Enumerators.AbilityEffectType abilityEffectType;

        [JsonIgnore]
        public Enumerators.AttackInfoType attackInfoType;

        [JsonIgnore]
        public Enumerators.CardType targetCardType;

        [JsonIgnore]
        public Enumerators.UnitStatusType targetUnitStatusType;

        [JsonIgnore]
        public Enumerators.CardType targetUnitType;

        public int value;

        public int damage;

        public int health;

        public string attackInfo;

        public string name;

        public int turns;

        public int count;

        public int delay;

        public void ParseData()
        {
            abilityType = Utilites.CastStringTuEnum<Enumerators.AbilityType>(type);
            abilityActivityType = Utilites.CastStringTuEnum<Enumerators.AbilityActivityType>(activityType);
            abilityCallType = Utilites.CastStringTuEnum<Enumerators.AbilityCallType>(callType);

            if (!string.IsNullOrEmpty(targetType))
            {
                abilityTargetTypes = Utilites.CastList<Enumerators.AbilityTargetType>(targetType);
            } else
            {
                abilityTargetTypes = new List<Enumerators.AbilityTargetType>();
            }

            if (!string.IsNullOrEmpty(statType))
            {
                abilityStatType = Utilites.CastStringTuEnum<Enumerators.StatType>(statType);
            } else
            {
                abilityStatType = Enumerators.StatType.NONE;
            }

            if (!string.IsNullOrEmpty(setType))
            {
                abilitySetType = Utilites.CastStringTuEnum<Enumerators.SetType>(setType);
            } else
            {
                abilitySetType = Enumerators.SetType.NONE;
            }

            if (!string.IsNullOrEmpty(effectType))
            {
                abilityEffectType = Utilites.CastStringTuEnum<Enumerators.AbilityEffectType>(effectType);
            } else
            {
                abilityEffectType = Enumerators.AbilityEffectType.NONE;
            }

            if (!string.IsNullOrEmpty(attackInfo))
            {
                attackInfoType = Utilites.CastStringTuEnum<Enumerators.AttackInfoType>(attackInfo);
            } else
            {
                attackInfoType = Enumerators.AttackInfoType.ANY;
            }

            if (!string.IsNullOrEmpty(cardType))
            {
                targetCardType = Utilites.CastStringTuEnum<Enumerators.CardType>(cardType);
            } else
            {
                targetCardType = Enumerators.CardType.NONE;
            }

            if (!string.IsNullOrEmpty(unitStatus))
            {
                targetUnitStatusType = Utilites.CastStringTuEnum<Enumerators.UnitStatusType>(unitStatus);
            } else
            {
                targetUnitStatusType = Enumerators.UnitStatusType.NONE;
            }

            if (!string.IsNullOrEmpty(unitType))
            {
                targetUnitType = Utilites.CastStringTuEnum<Enumerators.CardType>(unitType);
            }
        }
    }
}
