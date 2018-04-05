using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using Newtonsoft.Json;
using static GrandDevs.CZB.Common.Enumerators;
using System;


namespace GrandDevs.CZB.Data
{
    public class AbilityData
    {
        public string type;
        public string activityType;
        public string callType;
        public string targetType;
        public string affectObjectType;
        public string statType;


        [JsonIgnore]
		public AbilityType abilityType;
        [JsonIgnore]
		public AbilityActivityType abilityActivityType;
        [JsonIgnore]
		public AbilityCallType abilityCallType;
        [JsonIgnore]
		public AbilityTargetType abilityTargetType;
        [JsonIgnore]
		public AffectObjectType abilityAffectObjectType;
        [JsonIgnore]
		public StatType abilityStatType;

		public int value;

        public AbilityData()
        {

        }

        public void ParseData()
        {
            //abilityType = (AbilityType)Enum.Parse(typeof(AbilityType), type.ToUpper());
            abilityType = CastStringTuEnum<AbilityType>(type);
            UnityEngine.Debug.Log(abilityType);
		}

        private T CastStringTuEnum<T>(string data)
        {
            return (T)Enum.Parse(typeof(T), data.ToUpper());
        }
    }
}
