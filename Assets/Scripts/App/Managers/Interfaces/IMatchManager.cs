// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public interface IMatchManager
    {
        Enumerators.MatchType MatchType { get; set; }

        void FinishMatch(Enumerators.AppState appStateAfterMatch, bool tutorialCancel = false);
        void FindMatch(Enumerators.MatchType matchType);
    }
}
