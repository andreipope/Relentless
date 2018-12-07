using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System;

namespace Loom.ZombieBattleground
{
    public interface IMatchManager
    {
        event Action MatchFinished;

        Enumerators.MatchType MatchType { get; set; }

        void FinishMatch(Enumerators.AppState appStateAfterMatch);

        void FindMatch();
        void FindMatch(Enumerators.MatchType matchType);
        void DebugFindPvPMatch(Deck deck);

        AnalyticsTimer FindOpponentTime { get; set; }
    }
}
