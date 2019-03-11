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

        public override string ToString()
        {
            return $"({nameof(Id)}: {Id}, {nameof(Parameters)}: {Parameters})";
        }
    }
}
