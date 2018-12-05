using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public interface ITutorialManager
    {
        TutorialData CurrentTutorial { get; }
        TutorialDataStep CurrentTutorialDataStep { get; }

        bool IsTutorial { get; }

        bool IsBubbleShow { get; set; }

        void StartTutorial();
        void SetupTutorialById(int id);

        void SetFirstAvailableTutorial();

        void StopTutorial();

        void ReportAction(Enumerators.TutorialReportAction action);

        void ActivateSelectTarget();

        void DeactivateSelectTarget();

        void NextButtonClickHandler();

        void SkipTutorial(Enumerators.AppState state);

        int TutorialsCount { get; }
    }
}
