namespace Loom.ZombieBattleground.Data
{
    public class ParametrizedAbilityBoardObject
    {
        public readonly BoardObject BoardObject;

        public readonly AbilityParameters Parameters;

        public ParametrizedAbilityBoardObject(BoardObject boardObject, AbilityParameters parameters = null)
        {
            BoardObject = boardObject;
            Parameters = parameters ?? new AbilityParameters();
        }

        public class AbilityParameters
        {
            public int Attack;
            public int Defense;
            public string CardName;

            public AbilityParameters()
            {
                Attack = 0;
                Defense = 0;
                CardName = string.Empty;
            }
        }
    }
}
