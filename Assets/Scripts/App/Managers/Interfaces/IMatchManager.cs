using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public interface IMatchManager
    {
        Enumerators.MatchType MatchType { get; set; }

        void FinishMatch(Enumerators.AppState appStateAfterMatch);

        void FindMatch();
        void FindMatch(Enumerators.MatchType matchType);
        void DebugFindPvPMatch(Deck deck);
    }
}
