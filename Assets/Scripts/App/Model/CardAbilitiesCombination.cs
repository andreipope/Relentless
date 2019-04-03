using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class CardAbilitiesCombination
    {
        [JsonProperty]
        public List<GenericParameter> DefaultGenericParameters { get; protected set; }

        [JsonProperty]
        public List<CardAbilityData> CardAbilities { get; protected set; }

        [JsonProperty]
        public Enumerators.GameMechanicDescription DefaultGameMechanicDescription { get; protected set; }

        [JsonProperty]
        public List<CardAbilityData.TriggerInfo> DefaultTriggers { get; protected set; }

        [JsonProperty]
        public List<CardAbilityData.TargetInfo> DefaultTargets { get; protected set; }

        [JsonConstructor]
        public CardAbilitiesCombination(
            List<GenericParameter> defaultParameters,
            List<CardAbilityData> cardAbilityData,
            Enumerators.GameMechanicDescription gameMechanicDescription,
            List<CardAbilityData.TriggerInfo> defaultTriggers,
            List<CardAbilityData.TargetInfo> defaultTargets)
        {
            DefaultGenericParameters = defaultParameters;
            CardAbilities = cardAbilityData;
            DefaultGameMechanicDescription = gameMechanicDescription;
            DefaultTriggers = defaultTriggers;
            DefaultTargets = defaultTargets;
        }

        public CardAbilitiesCombination(CardAbilitiesCombination source)
        {
            DefaultGenericParameters = source.DefaultGenericParameters;
            CardAbilities = source.CardAbilities;
            DefaultGameMechanicDescription = source.DefaultGameMechanicDescription;
            DefaultTriggers = source.DefaultTriggers;
            DefaultTargets = source.DefaultTargets;
        }
    }
}
