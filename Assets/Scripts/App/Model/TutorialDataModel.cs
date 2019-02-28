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

        public bool HiddenTutorial;

        public bool IsGameplayTutorial()
        {
            return TutorialContent is TutorialGameplayContent;
        }
    }

    public class TutorialContent
    {
        public List<ActionActivityHandler> ActionActivityHandlers;

        public List<TutorialDescriptionTooltip> TutorialDescriptionTooltips;

        public TutorialReward TutorialReward;

        [JsonConverter(typeof(TutorialStepConverter))]
        public List<TutorialStep> TutorialSteps;

        public TutorialContent()
        {
            ActionActivityHandlers = new List<ActionActivityHandler>();

            TutorialDescriptionTooltips = new List<TutorialDescriptionTooltip>();

            TutorialReward = new TutorialReward();

            TutorialSteps = new List<TutorialStep>();
        }
    }

    public class TutorialGameplayContent : TutorialContent
    {
        public bool PlayerOrderScreenShouldAppear;

        public bool MulliganScreenShouldAppear;

        public bool DisabledSpecificTurnInfos;

        public bool GameplayFlowBeginsManually;

        public int RewardCardPackCount;

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

        public SpecificHordeInfo SpecificHordeInfo;

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

        public List<int> ConnectedActivities;

        public List<HandPointerInfo> HandPointers;

        public string SoundToPlay;

        public float SoundToPlayBeginDelay;

        public TutorialStep()
        {
            TutorialDescriptionTooltipsToActivate = new List<int>();

            TutorialDescriptionTooltipsToDeactivate = new List<int>();

            ConnectedActivities = new List<int>();

            HandPointers = new List<HandPointerInfo>();
        }
    }

    public class TutorialGameplayStep : TutorialStep
    {
        public int ConnectedTurnIndex;

        public int TutorialObjectIdStepOwner;

        public List<Enumerators.SkillTargetType> SelectableTargets;
        public List<Enumerators.TutorialActivityAction> RequiredActivitiesToDoneDuringStep;

        public bool MatchShouldBePaused;
        public bool AIShouldBePaused;
        public bool PlayerOverlordAbilityShouldBeUnlocked;
        public bool PlayerOrderScreenCloseManually;
        public bool CanEndTurn;
        public bool UnitsCanAttack;
        public bool CanInteractWithGameplay;
        public bool LaunchAIBrain;
        public bool LaunchGameplayManually;

        public List<OverlordSayTooltipInfo> OverlordSayTooltips;

        public string SpecificScreenToShow;

        public bool BeginGameplayFlowManually;

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

        public bool AISpecificOrderEnabled;

        public bool DisabledInitialization;

        public bool EnableCustomDeckForOpponent;

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
            public int TutorialObjectId;
            public int BuffedHealth;
            public int BuffedDamage;

            public UnitOnBoardInfo()
            {
                Name = string.Empty;
                IsManuallyPlayable = false;
            }
        }
    }

    public class ActionActivityHandler
    {
        public int Id;

        public Enumerators.TutorialActivityAction TutorialActivityAction;

        public Enumerators.TutorialActivityActionHandler TutorialActivityActionHandler;

        [JsonConverter(typeof(TutorialActivityActionHandlerDataConverter))]
        public TutorialActivityActionHandlerData TutorialActivityActionHandlerData;

        public Enumerators.TutorialActivityAction ConnectedTutorialActivityAction;

        public bool HasSpecificConnection;
    }

    public abstract class TutorialActivityActionHandlerData { }

    public class OverlordSayTooltipInfo : TutorialActivityActionHandlerData
    {
        public Enumerators.TooltipAlign TutorialTooltipAlign;
        public Enumerators.TutorialObjectOwner TutorialTooltipOwner;
        public Enumerators.TutorialActivityAction ActionToHideThisPopup;
        public string Description;
        public string SoundToPlay;
        public float AppearDelay;
        public float Duration = Constants.OverlordTalkingPopupDuration;
        public float SoundToPlayBeginDelay;
        public float MinimumShowTime = Constants.OverlordTalkingPopupMinimumShowTime;
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
        public bool AboveUI;
    }

    public class TutorialDescriptionTooltip
    {
        public int Id;
        public Enumerators.TooltipAlign TutorialTooltipAlign;
        public Enumerators.TutorialObjectOwner TutorialTooltipOwner;
        public int TutorialTooltipOwnerId;
        public string Description;
        public FloatVector3 Position;
        public bool Resizable;
        public bool DynamicPosition;
        public float AppearDelay;
        public float MinimumShowTime = Constants.DescriptionTooltipMinimumShowTime;
        public Enumerators.TutorialObjectLayer TutorialTooltipLayer;

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

    public class UseOverlordSkillActionInfo
    {
        public Enumerators.SkillType SkillType;
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
        public Enumerators.TutorialObjectOwner TutorialHandPointerOwner;
        public bool AppearOnce;
        public Enumerators.TutorialObjectLayer TutorialHandLayer;
        public float AppearDelay;
        public float HandPointerSpeed;
        public int TutorialObjectIdStepOwner;
        public string TutorialUIElementOwnerName;
        public float Rotation;
        public int TargetTutorialObjectId;
        public List<int> AdditionalObjectIdOwners;
        public List<int> AdditionalObjectIdTargets;

        public HandPointerInfo()
        {
            StartPosition = new FloatVector3();
            EndPosition = new FloatVector3();
            TutorialHandPointerType = Enumerators.TutorialHandPointerType.Single;
            TutorialHandPointerOwner = Enumerators.TutorialObjectOwner.Undefined;

            AdditionalObjectIdOwners = new List<int>();
            AdditionalObjectIdTargets = new List<int>();

            HandPointerSpeed = Constants.HandPointerSpeed;
        }
    }

    public class SpecificTurnInfo
    {
        public int TurnIndex;
        public List<Enumerators.TutorialActivityAction> RequiredActivitiesToDoneDuringTurn;
        public List<PlayCardActionInfo> PlayCardsSequence;
        public List<UseBattleframeActionInfo> UseBattleframesSequence;
        public List<UseOverlordSkillActionInfo> UseOverlordSkillsSequence;
        public List<ActionActivityHandler> ActionActivityHandlers;

        public SpecificTurnInfo()
        {
            PlayCardsSequence = new List<PlayCardActionInfo>();
            UseBattleframesSequence = new List<UseBattleframeActionInfo>();
            RequiredActivitiesToDoneDuringTurn = new List<Enumerators.TutorialActivityAction>();
            ActionActivityHandlers = new List<ActionActivityHandler>();
            UseOverlordSkillsSequence = new List<UseOverlordSkillActionInfo>();
        }
    }

    public class SpecificHordeInfo
    {
        public Enumerators.SetType MainSet;
        public List<Data.CollectionCardData> CardsForArmy;
        public int MaximumCardsCount;

        public SpecificHordeInfo()
        {
            CardsForArmy = new List<Data.CollectionCardData>();
        }
    }

    public class TutorialReward
    {
        public List<CardRewardInfo> CardPackReward;
        public int CardPackCount;

        public TutorialReward()
        {
            CardPackReward = new List<CardRewardInfo>();
        }
    }

    public class CardRewardInfo
    {
        public string Name;
    }

    public class TutorialMenuStep : TutorialStep
    {
        public string OpenScreen;
        public List<string> BlockedButtons;
        public bool CardsInteractingLocked;
        public bool BattleShouldBeWonBlocker;
        public bool CanDragCards;
        public bool CanDoubleTapCards;

        public TutorialMenuStep()
        {
            BlockedButtons = new List<string>();
        }
    }
}
