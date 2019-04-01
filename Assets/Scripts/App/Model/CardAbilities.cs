using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class CardAbilities
    {
        [JsonProperty]
        public List<GenericParameter> DefaultParameters { get; protected set; }

        [JsonProperty]
        public List<CardAbilityData> CardAbilityDatas { get; protected set; }

        [JsonConstructor]
        public CardAbilities(
            List<GenericParameter> defaultParameters,
            List<CardAbilityData> cardAbilityData)
        {
            DefaultParameters = defaultParameters;
            CardAbilityDatas = cardAbilityData;
        }

        public CardAbilities(CardAbilities source)
        {
            DefaultParameters = source.DefaultParameters;
            CardAbilityDatas = source.CardAbilityDatas;
        }
    }
}
