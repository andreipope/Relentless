
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class GenericParameter
    {
        [JsonProperty]
        public Enumerators.AbilityParameter AbilityParameter { get; protected set; }

        [JsonProperty]
        public object Value { get; protected set; }

        [JsonConstructor]
        public GenericParameter(Enumerators.AbilityParameter abilityParameter, object value)
        {
            AbilityParameter = abilityParameter;
            Value = value;
        }

        public GenericParameter(GenericParameter source)
        {
            AbilityParameter = source.AbilityParameter;
            Value = source.Value;
        }
    }
}
