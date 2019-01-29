using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public interface ITutorialManager
    {
        TutorialData CurrentTutorial { get; }
        TutorialStep CurrentTutorialStep { get; }

        bool IsTutorial { get; }

        int TutorialsCount { get; }

        bool PlayerWon { get; set; }

        void StartTutorial();

        void SetupTutorialById(int id);

        void StopTutorial();

        void ActivateSelectHandPointer(Enumerators.TutorialObjectOwner owner);

        void DeactivateSelectHandPointer(Enumerators.TutorialObjectOwner owner);

        void ReportActivityAction(Enumerators.TutorialActivityAction action, int sender = -1);

        void ReportActivityAction(Enumerators.TutorialActivityAction activityAction, BoardObject sender, string tag = "");

        void ActivateDescriptionTooltipByOwner(Enumerators.TutorialObjectOwner owner, Vector3 position);

        TutorialDescriptionTooltipItem GetDescriptionTooltip(int id);

        SpecificTurnInfo GetCurrentTurnInfo();
        bool IsCompletedActivitiesForThisTurn();
        string GetCardNameById(int id);

        bool IsButtonBlockedInTutorial(string name);
        bool CheckNextTutorial();
    }
}
