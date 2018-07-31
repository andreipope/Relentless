// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;

namespace LoomNetwork.CZB
{
    public interface ITutorialManager
    {
        void StartTutorial();
        void StopTutorial();
        void CancelTutorial();
        void ReportAction(Enumerators.TutorialReportAction action);
        int CurrentStep { get; }
        bool IsTutorial { get; }
        bool IsBubbleShow { get; set; }
        void ActivateSelectTarget();
        void DeactivateSelectTarget();
        void NextButtonClickHandler();
        void SkipTutorial();
        //void StopTimer(Action<object[]> handler);
        //void AddTimer(Action<object[]> handler, object[] parameters = null, float time = 1, bool loop = false, bool storeTimer = false);
    }
}