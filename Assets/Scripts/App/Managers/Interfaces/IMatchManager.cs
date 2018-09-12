using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public interface IMatchManager
    {
        Enumerators.MatchType MatchType { get; set; }

        void FinishMatch(Enumerators.AppState appStateAfterMatch);

        void FindMatch(Enumerators.MatchType matchType);
    }
}
