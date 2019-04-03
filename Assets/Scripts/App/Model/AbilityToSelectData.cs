using Loom.Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class AbilityToSelectData
    {
        [JsonProperty]
        public string Description { get; private set; }

        [JsonProperty]
        public AbilityData AbilityData { get; private set; }

        [JsonProperty]
        public string Attribute { get; private set; }

        [JsonConstructor]
        public AbilityToSelectData(string description, AbilityData abilityData, string attribute)
        {
            Description = description;
            AbilityData = abilityData;
            Attribute = attribute;
        }

        public AbilityToSelectData(AbilityToSelectData source)
        {
            Description = source.Description;
            Attribute = source.Attribute;
            AbilityData = new AbilityData(source.AbilityData);
        }
    }
}
