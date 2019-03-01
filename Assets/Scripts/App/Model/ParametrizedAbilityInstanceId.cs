using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public class ParametrizedAbilityInstanceId
    {
        public readonly InstanceId Id;
        public readonly ParametrizedAbilityParameters Parameters;

        public ParametrizedAbilityInstanceId(InstanceId id, ParametrizedAbilityParameters parameters = null)
        {
            Id = id;
            Parameters = parameters ?? new ParametrizedAbilityParameters();
        }
    }
}
