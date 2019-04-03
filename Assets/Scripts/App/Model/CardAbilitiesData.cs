using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loom.ZombieBattleground.Data
{
    public class CardAbilitiesData
    {
        [JsonProperty]
        public List<AbilityData> CardAbilities { get; private set; }

        [JsonProperty]
        public Enumerators.GameMechanicDescription DefaultGameMechanicDescription { get; private set; }

        [JsonProperty]
        public List<GenericParameter> DefaultGenericParameters { get; private set; }

        [JsonProperty]
        public List<AbilityData.TriggerInfo> DefaultTriggers { get; private set; }

        [JsonProperty]
        public List<AbilityData.TargetInfo> DefaultTargets { get; private set; }

        [JsonConstructor]
        public CardAbilitiesData(
            List<GenericParameter> defaultParameters,
            List<AbilityData> cardAbilityData,
            Enumerators.GameMechanicDescription gameMechanicDescription,
            List<AbilityData.TriggerInfo> defaultTriggers,
            List<AbilityData.TargetInfo> defaultTargets)
        {
            DefaultGenericParameters = defaultParameters;
            CardAbilities = cardAbilityData;
            DefaultGameMechanicDescription = gameMechanicDescription;
            DefaultTriggers = defaultTriggers;
            DefaultTargets = defaultTargets;
        }

        public CardAbilitiesData(CardAbilitiesData source)
        {
            DefaultGenericParameters = source.DefaultGenericParameters;
            CardAbilities = source.CardAbilities;
            DefaultGameMechanicDescription = source.DefaultGameMechanicDescription;
            DefaultTriggers = source.DefaultTriggers;
            DefaultTargets = source.DefaultTargets;
        }
    }
}
