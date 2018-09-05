using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class TutorialData
    {
        public int TutorialId;
        public List<TutorialDataStep> TutorialDataSteps;

        public void ParseData()
        {
            foreach (var element in TutorialDataSteps)
                element.ParseData();
        }
    }

    public class TutorialDataStep
    {
        public string janePose;
        public string requiredAction;

        [JsonIgnore]
        public Enumerators.TutorialJanePoses JanePose;
        [JsonIgnore]
        public Enumerators.TutorialReportAction RequiredAction;

        public FloatVector3 ArrowStartPosition;
        public FloatVector3 ArrowEndPosition;

        public string JaneText;
        public string SoundName;
        public string EmotionDescription;

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

        public float DelayToPlaySound;

        public TutorialDataStep()
        {
            janePose = string.Empty;
            requiredAction = string.Empty;

            ArrowStartPosition = new FloatVector3();
            ArrowEndPosition = new FloatVector3();

            JaneText = string.Empty;
            SoundName = string.Empty;
            EmotionDescription = string.Empty;

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
        }

        public void ParseData()
        {
            if (!string.IsNullOrEmpty(janePose))
            {
                JanePose = Utilites.CastStringTuEnum<Enumerators.TutorialJanePoses>(janePose);
            }
            else
            {
                JanePose = Enumerators.TutorialJanePoses.NORMAL;
            }

            if (!string.IsNullOrEmpty(requiredAction))
            {
                RequiredAction = Utilites.CastStringTuEnum<Enumerators.TutorialReportAction>(requiredAction);
            }
            else
            {
                RequiredAction = Enumerators.TutorialReportAction.NONE;
            }
        }
    }
}
