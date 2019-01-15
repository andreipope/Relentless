using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class TutorialData
    {
        public int Id;
        public string Name;

        [JsonConverter(typeof(TutorialContentConverter))]
        public TutorialContent TutorialContent;

        public bool Ignore;

        public bool IsGameplayTutorial()
        {
            return TutorialContent is TutorialGameplayContent;
        }
    }

    public class TutorialContent
    {
        public List<ActionActivityHandler> ActionActivityHandlers;

        public List<TutorialDescriptionTooltip> TutorialDescriptionTooltips;

        [JsonConverter(typeof(TutorialStepConverter))]
        public List<TutorialStep> TutorialSteps;

        public TutorialContent()
        {
            ActionActivityHandlers = new List<ActionActivityHandler>();

            TutorialDescriptionTooltips = new List<TutorialDescriptionTooltip>();

            TutorialSteps = new List<TutorialStep>();
        }
    }

    public class TutorialGameplayContent : TutorialContent
    {
        public bool PlayerOrderScreenShouldAppear;

        public SpecificBattlegroundInfo SpecificBattlegroundInfo;

        public List<SpecificTurnInfo> SpecificTurnInfos;

        public TutorialGameplayContent() : base()
        {
            SpecificTurnInfos = new List<SpecificTurnInfo>();
        }
    }

    public class TutorialMenusContent : TutorialContent
    {
        public bool HasBlockedInteractivityInGameScreens;

        public TutorialMenusContent() : base()
        {
        }
    }

    public class TutorialStep
    {
        [JsonIgnore]
        public bool IsDone;

        public TutorialAvatarInfo TutorialAvatar;

        public Enumerators.TutorialActivityAction ActionToEndThisStep;

        public List<int> TutorialDescriptionTooltipsToActivate;

        public List<int> TutorialDescriptionTooltipsToDeactivate;

        public List<HandPointerInfo> HandPointers;

        public TutorialStep()
        {
            TutorialDescriptionTooltipsToActivate = new List<int>();

            TutorialDescriptionTooltipsToDeactivate = new List<int>();

            HandPointers = new List<HandPointerInfo>();
        }
    }

    public class TutorialGameplayStep : TutorialStep
    {
        public int ConnectedTurnIndex;

        public int TutorialObjectIdStepOwner;

        public List<Enumerators.SkillTargetType> SelectableTargets;

        public bool MatchShouldBePaused;
        public bool PlayerOverlordAbilityShouldBeUnlocked;
        public string SoundToPlay;
        public float SoundToPlayBeginDelay;
        public bool CanEndTurn;
        public bool UnitsCanAttack;
        public bool CanInteractWithGameplay;
        public bool LaunchAIBrain;
        public bool LaunchGameplayManually;

        public List<OverlordSayTooltipInfo> OverlordSayTooltips;

        public string SpecificScreenToShow;

        public TutorialGameplayStep()
        {
            SelectableTargets = new List<Enumerators.SkillTargetType>();

            OverlordSayTooltips = new List<OverlordSayTooltipInfo>();
        }
    }

    public class SpecificBattlegroundInfo
    {
        public int TutorialTurnBeginAtStart;

        public bool PlayerTurnFirst;

        public bool RankSystemHasEnabled;

        public bool GameplayBeginManually;

        public SpecificBattlegroundOverlordInfo PlayerInfo;
        public SpecificBattlegroundOverlordInfo OpponentInfo;

        public SpecificBattlegroundInfo()
        {
            TutorialTurnBeginAtStart = Constants.FirstGameTurnIndex;
            RankSystemHasEnabled = true;

            PlayerInfo = new SpecificBattlegroundOverlordInfo();
            OpponentInfo = new SpecificBattlegroundOverlordInfo();
        }

        public class SpecificBattlegroundOverlordInfo
        {
            public int OverlordId;
            public int Defense;
            public int MaximumDefense;
            public int CurrentGoo;
            public int MaximumGoo;

            public List<OverlordCardInfo> CardsInHand;
            public List<OverlordCardInfo> CardsInDeck;
            public List<UnitOnBoardInfo> CardsOnBoard;

            public Enumerators.OverlordSkill PrimaryOverlordAbility;
            public Enumerators.OverlordSkill SecondaryOverlordAbility;

            public SpecificBattlegroundOverlordInfo()
            {
                CardsInHand = new List<OverlordCardInfo>();
                CardsInDeck = new List<OverlordCardInfo>();
                CardsOnBoard = new List<UnitOnBoardInfo>();

                MaximumDefense = Constants.DefaultPlayerHp;
                Defense = MaximumDefense;

                MaximumGoo = Constants.DefaultPlayerGoo;
                CurrentGoo = MaximumGoo;

                OverlordId = 0;
            }
        }

        public class OverlordCardInfo
        {
            public string Name;
            public int TutorialObjectId;
        }

        public class UnitOnBoardInfo
        {
            public string Name;
            public bool IsManuallyPlayable;

            public UnitOnBoardInfo()
            {
                Name = string.Empty;
                IsManuallyPlayable = false;
            }
        }
    }

    public class ActionActivityHandler
    {
        public Enumerators.TutorialActivityAction TutorialActivityAction;

        public Enumerators.TutorialActivityActionHandler TutorialActivityActionHandler;

        [JsonConverter(typeof(TutorialActivityActionHandlerDataConverter))]
        public TutorialActivityActionHandlerData TutorialActivityActionHandlerData;

        public Enumerators.TutorialActivityAction ConnectedTutorialActivityAction;
    }

    public abstract class TutorialActivityActionHandlerData { }

    public class OverlordSayTooltipInfo : TutorialActivityActionHandlerData
    {
        public Enumerators.TooltipAlign TutorialTooltipAlign;
        public Enumerators.TooltipOwner TutorialTooltipOwner;
        public string Description;
        public float AppearDelay;
    }

    public class DrawDescriptionTooltipsInfo : TutorialActivityActionHandlerData
    {
        public List<int> TutorialDescriptionTooltipsToActivate;

        public DrawDescriptionTooltipsInfo()
        {
            TutorialDescriptionTooltipsToActivate = new List<int>();
        }
    }

    public class TutorialAvatarInfo
    {
        public Enumerators.TutorialAvatarPose Pose;
        public string Description;
        public string DescriptionTooltipCloseText;
    }

    public class TutorialDescriptionTooltip
    {
        public int Id;
        public Enumerators.TooltipAlign TutorialTooltipAlign;
        public Enumerators.TooltipOwner TutorialTooltipOwner;
        public string TutorialTooltipOwnerName;
        public string Description;
        public FloatVector3 Position;
        public bool Resizable;

        public TutorialDescriptionTooltip()
        {
            Position = new FloatVector3();
            Resizable = true;
        }
    }

    public class UseBattleframeActionInfo
    {
        public int TutorialObjectId;
        public Enumerators.SkillTargetType TargetType;
        public int TargetTutorialObjectId;
    }

    public class PlayCardActionInfo
    {
        public int TutorialObjectId;
        public Enumerators.SkillTargetType TargetType;
        public int TargetTutorialObjectId;
    }

    public class HandPointerInfo
    {
        public FloatVector3 StartPosition;
        public FloatVector3 EndPosition;
        public Enumerators.TutorialHandPointerType TutorialHandPointerType;
        public bool AppearOnce;
        public float AppearDelay;
        public int TutorialObjectIdStepOwner;

        public HandPointerInfo()
        {
            StartPosition = new FloatVector3();
            EndPosition = new FloatVector3();
            TutorialHandPointerType = Enumerators.TutorialHandPointerType.Single;
        }
    }

    public class SpecificTurnInfo
    {
        public int TurnIndex;
        public List<Enumerators.TutorialActivityAction> RequiredActivitiesToDoneDuringTurn;
        public List<PlayCardActionInfo> PlayCardsSequence;
        public List<UseBattleframeActionInfo> UseBattleframesSequence;
        public List<ActionActivityHandler> ActionActivityHandlers;

        public SpecificTurnInfo()
        {
            PlayCardsSequence = new List<PlayCardActionInfo>();
            UseBattleframesSequence = new List<UseBattleframeActionInfo>();
            RequiredActivitiesToDoneDuringTurn = new List<Enumerators.TutorialActivityAction>();
            ActionActivityHandlers = new List<ActionActivityHandler>();
        }
    }

    public class TutorialMenuStep : TutorialStep
    {
        public List<string> BlockedButtons;

        public TutorialMenuStep()
        {
            BlockedButtons = new List<string>();
        }
    }
}
