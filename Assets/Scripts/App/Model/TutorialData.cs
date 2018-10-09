using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class TutorialData
    {
        public int TutorialId;
        public List<TutorialDataStep> TutorialDataSteps;
        public SpecificBattlegroundInfo SpecificBattlegroundInfo;
        public bool PlayerTurnFirst;
    }

    public class TutorialDataStep
    {
        public Enumerators.TutorialJanePoses JanePose;
        public Enumerators.TutorialReportAction RequiredAction;

        public FloatVector3 ArrowStartPosition;
        public FloatVector3 ArrowEndPosition;

        public string JaneText;
        public string SoundName;
        public string EmotionDescription;
        public string AvailableCard;
        public string WaiterOfWhichTurn; 

        public bool IsFocusing;
        public bool IsArrowEnabled;
        public bool CanMoveToNextStepByClick;
        public bool CanMoveToNextStepByClickInPaused;
        public bool ShouldStopTurn;
        public bool IsShowNextButtonFocusing;
        public bool IsShowNextButton;
        public bool IsShowQuestion;
        public bool CanProceedWithEndStepManually;
        public bool HasDelayToPlaySound;
        public bool CanUseBoardSkill;
        public bool CanClickEndTurn;
        public bool IsManuallyHideBubble;
        public bool IsPauseTutorial;
        public bool CanHandleInput;
        public bool IsLaunchAIBrain;
        public bool AvailableCardUsableInHand;
        public bool UnitsCanAttack;
        public bool BoardArrowCanUsableOnUnits;
        public bool BoardArrowCanUsableOnPlayer;

        public float DelayToPlaySound;

        public TutorialDataStep()
        {
            ArrowStartPosition = new FloatVector3();
            ArrowEndPosition = new FloatVector3();

            JaneText = string.Empty;
            SoundName = string.Empty;
            EmotionDescription = string.Empty;
            AvailableCard = string.Empty;
            WaiterOfWhichTurn = string.Empty;

            DelayToPlaySound = 0f;

            IsFocusing = false;
            IsArrowEnabled = false;
            CanMoveToNextStepByClick = false;
            CanMoveToNextStepByClickInPaused = false;
            ShouldStopTurn = false;
            IsShowNextButtonFocusing = false;
            IsShowNextButton = false;
            IsShowQuestion = false;
            CanProceedWithEndStepManually = false;
            HasDelayToPlaySound = false;
            CanUseBoardSkill = false;
            CanClickEndTurn = false;
            IsManuallyHideBubble = false;
            IsPauseTutorial = false;
            CanHandleInput = false;
            IsLaunchAIBrain = false;
            UnitsCanAttack = false;
            BoardArrowCanUsableOnUnits = false;
            BoardArrowCanUsableOnPlayer = false;
        }
    }
}
