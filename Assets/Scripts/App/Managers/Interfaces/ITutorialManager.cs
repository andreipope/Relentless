// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public interface ITutorialManager
    {
        int CurrentStep { get; }

        bool IsTutorial { get; }

        bool IsBubbleShow { get; set; }

        void StartTutorial();

        void StopTutorial();

        void ReportAction(Enumerators.TutorialReportAction action);

        void ActivateSelectTarget();

        void DeactivateSelectTarget();

        void NextButtonClickHandler();

        void SkipTutorial(Enumerators.AppState state);
    }
}
