using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public struct Unit
    {
        public int InstanceId { get; }
        public Enumerators.AffectObjectType AffectObjectType { get; }

        public Unit(int instanceId, Enumerators.AffectObjectType affectObjectType)
        {
            InstanceId = instanceId;
            AffectObjectType = affectObjectType;
        }
    }
}
