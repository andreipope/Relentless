namespace Loom.ZombieBattleground.Data
{
    public class ParametrizedAbilityBoardObject
    {
        public readonly IBoardObject BoardObject;
        public readonly ParametrizedAbilityParameters Parameters;

        public ParametrizedAbilityBoardObject(IBoardObject boardObject, ParametrizedAbilityParameters parameters = null)
        {
            BoardObject = boardObject;
            Parameters = parameters ?? new ParametrizedAbilityParameters();
        }
    }
}
