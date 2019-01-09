using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public struct Unit
    {
        public Data.InstanceId InstanceId { get; }
        public Enumerators.AffectObjectType AffectObjectType { get; }
        public ParameterType Parameter { get; }

        public Unit(Data.InstanceId instanceId, Enumerators.AffectObjectType affectObjectType, ParameterType parameter)
        {
            InstanceId = instanceId;
            AffectObjectType = affectObjectType;
            Parameter = parameter;
        }

        public struct ParameterType
        {
            public int Attack { get; }
            public int Defense { get; }
            public string CardName { get; }

            public ParameterType(int attack, int defense, string cardName)
            {
                Attack = attack;
                Defense = defense;
                CardName = cardName;
            }
        }
    }
}
