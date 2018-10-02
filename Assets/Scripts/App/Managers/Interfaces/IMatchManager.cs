using Loom.Client;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public interface IMatchManager
    {
        Enumerators.MatchType MatchType { get; set; }
        Address? CustomGameAddress { get; set; }

        void FinishMatch(Enumerators.AppState appStateAfterMatch);

        void FindMatch();
        void FindMatch(Enumerators.MatchType matchType);
    }
}
