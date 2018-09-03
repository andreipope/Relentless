using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public interface IMatchManager
    {
        Enumerators.MatchType MatchType { get; set; }

        void FinishMatch(Enumerators.AppState appStateAfterMatch);

        void FindMatch(Enumerators.MatchType matchType);
    }
}
