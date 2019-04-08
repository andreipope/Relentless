using System;

namespace Loom.ZombieBattleground.Data
{
    public class ParametrizedAbilityParameters
    {
        public int Attack;
        public int Defense;
        public string CardName = String.Empty;

        public override string ToString()
        {
            return $"({nameof(Attack)}: {Attack}, {nameof(Defense)}: {Defense}, {nameof(CardName)}: {CardName})";
        }
    }
}
