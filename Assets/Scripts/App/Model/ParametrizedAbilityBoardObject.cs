namespace Loom.ZombieBattleground.Data
{
    public class ParametrizedAbilityBoardObject
    {
        public readonly BoardObject BoardObject;
        public readonly ParametrizedAbilityParameters Parameters;

        public ParametrizedAbilityBoardObject(BoardObject boardObject, ParametrizedAbilityParameters parameters = null)
        {
            BoardObject = boardObject;
            Parameters = parameters ?? new ParametrizedAbilityParameters();
        }
    }
}
