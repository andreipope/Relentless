using System;

namespace Loom.ZombieBattleground.Data
{
    public class ParametrizedAbilityParameters
    {
        public int Attack;
        public int Defense;
        public string CardName;

        public ParametrizedAbilityParameters()
        {
            Attack = 0;
            Defense = 0;
            CardName = String.Empty;
        }
    }
}