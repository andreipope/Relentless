using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class CardAbilitiesCombination
    {
        [JsonProperty]
        public List<CardAbilityData> CardAbilities { get; protected set; }

        [JsonProperty]
        public Enumerators.GameMechanicDescription DefaultGameMechanicDescription { get; protected set; }

        [JsonProperty]
        public List<GenericParameter> DefaultGenericParameters { get; protected set; }

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

        public bool HasTrigger(Enumerators.AbilityTrigger trigger)
        {
            return DefaultTriggers?.FindAll(trig => trig.Trigger == trigger).Count > 0;
        }

        public bool TryGetTrigger(Enumerators.AbilityTrigger trigger, out CardAbilityData.TriggerInfo triggerInfo)
        {
            if (!HasTrigger(trigger))
            {
                triggerInfo = default(CardAbilityData.TriggerInfo);
                return false;
            }

            triggerInfo = DefaultTriggers.Find(trig => trig.Trigger == trigger);

            return true;
        }
    }
}
