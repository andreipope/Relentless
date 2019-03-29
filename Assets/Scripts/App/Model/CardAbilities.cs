using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class CardAbilities
    {
        public IReadOnlyList<GenericParameter> DefaultParameters { get; set; }
        public IReadOnlyList<CardAbilityData> CardAbilityData { get; set; }

        public CardAbilities(
            IReadOnlyList<GenericParameter> defaultParameters,
            IReadOnlyList<CardAbilityData> cardAbilityData)
        {
            DefaultParameters = defaultParameters;
            CardAbilityData = cardAbilityData;
        }

        public CardAbilities(CardAbilities source)
        {
            DefaultParameters = source.DefaultParameters;
            CardAbilityData = source.CardAbilityData;
        }
    }
}
