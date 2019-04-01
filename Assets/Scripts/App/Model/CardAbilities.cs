using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class CardAbilities
    {
        [JsonProperty]
        public List<CardAbilitiesCombination> Combinations { get; protected set; }

        [JsonConstructor]
        public CardAbilities(List<CardAbilitiesCombination> cardAbilitiesCombinations)
        {
            Combinations = cardAbilitiesCombinations;
        }

        public CardAbilities(CardAbilities source)
        {
            Combinations = source.Combinations;
        }
    }

    public class CardAbilitiesCombination
    {
        [JsonProperty]
        public List<GenericParameter> DefaultParameters { get; protected set; }

        [JsonProperty]
        public List<CardAbilityData> CardAbilities { get; protected set; }

        [JsonConstructor]
        public CardAbilitiesCombination(
            List<GenericParameter> defaultParameters,
            List<CardAbilityData> cardAbilityData)
        {
            DefaultParameters = defaultParameters;
            CardAbilities = cardAbilityData;
        }

        public CardAbilitiesCombination(CardAbilitiesCombination source)
        {
            DefaultParameters = source.DefaultParameters;
            CardAbilities = source.CardAbilities;
        }
    }
}
