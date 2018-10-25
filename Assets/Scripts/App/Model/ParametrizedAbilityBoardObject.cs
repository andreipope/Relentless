namespace Loom.ZombieBattleground.Data
{
    public class ParametrizedAbilityBoardObject
    {
        public BoardObject BoardObject;

        public AbilityParameters Parameters;

        public ParametrizedAbilityBoardObject()
        {
            Parameters = new AbilityParameters();
        }

        public class AbilityParameters
        {
            public int Damage;
            public int Defense;
        }
    }
}
