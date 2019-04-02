using Loom.ZombieBattleground.Common;
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

        [JsonProperty]
        public Enumerators.GameMechanicDescription GeneralGameMechanicDescription { get; protected set; }

        [JsonConstructor]
        public CardAbilitiesCombination(
            List<GenericParameter> defaultParameters,
            List<CardAbilityData> cardAbilityData,
            Enumerators.GameMechanicDescription gameMechanicDescription)
        {
            DefaultParameters = defaultParameters;
            CardAbilities = cardAbilityData;
            GeneralGameMechanicDescription = gameMechanicDescription;
        }

        public CardAbilitiesCombination(CardAbilitiesCombination source)
        {
            DefaultParameters = source.DefaultParameters;
            CardAbilities = source.CardAbilities;
        }
    }
}
