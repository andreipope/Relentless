using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public interface IMatchManager
    {
        Enumerators.MatchType MatchType { get; set; }

        void FinishMatch(Enumerators.AppState appStateAfterMatch, bool tutorialCancel = false);
        void FindMatch(Enumerators.MatchType matchType);
    }
}
