using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loom.ZombieBattleground.Data
{
    public class RankBuffsData
    {
        [JsonProperty]
        public List<RankBuffInfo> RankBuffs { get; set; }

        [JsonConstructor]
        public RankBuffsData(List<RankBuffInfo> rankBuffs)
        {
            RankBuffs = rankBuffs;
        }

        public AbilityData GetDataByType(Enumerators.BuffType type)
        {
            return RankBuffs.Find(buff => buff.BuffType == type)?.AbilityData;
        }

        public class RankBuffInfo
        {
            [JsonProperty]
            public Enumerators.BuffType BuffType { get; set; }

            [JsonProperty]
            public AbilityData AbilityData { get; set; }

            [JsonConstructor]
            public RankBuffInfo(Enumerators.BuffType buffType, AbilityData abilityData)
            {
                BuffType = buffType;
                AbilityData = abilityData;
            }
        }
    }
}
