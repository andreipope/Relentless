using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using Newtonsoft.Json;
using System;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB.Data
{
    public class AbilityData
    {
        public string type;
        public string activityType;
        public string callType;
        public string targetType;
        public string statType;
        public string setType;


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

        public int value;

        public AbilityData()
        {

        }

        public void ParseData()
        {
            abilityType             =  CastStringTuEnum<Enumerators.AbilityType>(type);
            abilityActivityType     =  CastStringTuEnum<Enumerators.AbilityActivityType>(activityType);
            abilityCallType         =  CastStringTuEnum<Enumerators.AbilityCallType>(callType);
            abilityTargetTypes      =  CastList<Enumerators.AbilityTargetType>(targetType);
            if(statType != null)
                abilityStatType     =  CastStringTuEnum<Enumerators.StatType>(statType);
            if (setType != null)
                abilitySetType      =  CastStringTuEnum<Enumerators.SetType>(setType);
        }

        private T CastStringTuEnum<T>(string data)
        {
            return (T)Enum.Parse(typeof(T), data.ToUpper());
        }

        private List<T> CastList<T>(string data, char separator = '|')
        {
            List<T> list = new List<T>();
            string[] targets = data.Split(separator);
            foreach(var target in targets)
            {
                list.Add(CastStringTuEnum<T>(target));
            }
            return list;
        }
    }
}
