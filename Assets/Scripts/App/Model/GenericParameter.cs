
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public class GenericParameter
    {
        public readonly Enumerators.AbilityParameter AbilityParameter;
        public readonly object Value;

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
