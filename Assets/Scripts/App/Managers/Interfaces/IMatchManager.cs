using Loom.ZombieBattleground.Common;
using System;
using System.Threading.Tasks;

namespace Loom.ZombieBattleground
{
    public interface IMatchManager
    {
        event Action MatchFinished;

        Enumerators.MatchType MatchType { get; set; }

        void FinishMatch(Enumerators.AppState appStateAfterMatch);

        Task FindMatch();
        Task FindMatch(Enumerators.MatchType matchType);
        AnalyticsTimer FindOpponentTime { get; set; }
    }
}
