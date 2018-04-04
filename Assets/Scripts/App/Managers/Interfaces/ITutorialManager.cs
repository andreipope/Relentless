using GrandDevs.CZB.Common;
using System;

namespace GrandDevs.CZB
{
    public interface ITutorialManager
    {
        void StartTutorial();
        void StopTutorial();
        void ReportAction(Enumerators.TutorialReportAction action);
        int CurrentStep { get; }
        bool IsTutorial { get; }
        //void StopTimer(Action<object[]> handler);
        //void AddTimer(Action<object[]> handler, object[] parameters = null, float time = 1, bool loop = false, bool storeTimer = false);
    }
}