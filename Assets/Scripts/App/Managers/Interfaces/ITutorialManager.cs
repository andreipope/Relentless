using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public interface ITutorialManager
    {
        TutorialData CurrentTutorial { get; }
        TutorialStep CurrentTutorialStep { get; }

        bool IsTutorial { get; }

        int TutorialsCount { get; }

        void StartTutorial();

        void SetupTutorialById(int id);

        void StopTutorial();

        void ReportActivityAction(Enumerators.TutorialActivityAction action, int sender = -1);

        SpecificTurnInfo GetCurrentTurnInfo();
        bool IsCompletedActivitiesForThisTurn();
        string GetCardNameById(int id);
    }
}
