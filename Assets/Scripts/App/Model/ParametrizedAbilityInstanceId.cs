using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public class ParametrizedAbilityInstanceId
    {
        public readonly InstanceId Id;
        public readonly Enumerators.AffectObjectType AffectObjectType;
        public readonly ParametrizedAbilityParameters Parameters;

        public ParametrizedAbilityInstanceId(InstanceId id, Enumerators.AffectObjectType affectObjectType, ParametrizedAbilityParameters parameters = null)
        {
            Id = id;
            AffectObjectType = affectObjectType;
            Parameters = parameters ?? new ParametrizedAbilityParameters();
        }
    }
}
