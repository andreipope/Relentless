using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Newtonsoft.Json;
using DG.Tweening;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Helpers;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class TutorialManager : IService, ITutorialManager
    {
        private static readonly ILog Log = Logging.GetLog(nameof(TutorialManager));

        private const string TutorialDataPath = "Data/tutorial_data";

        private const string InGameTutorialDataPath = "Data/ingame_tutorial";

        private const int FirstDeckBuildTutorialIndex = 1;

        private IUIManager _uiManager;

        private ISoundManager _soundManager;

        private ILoadObjectsManager _loadObjectsManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private IDataManager _dataManager;

        private IGameplayManager _gameplayManager;

        private BattlegroundController _battlegroundController;

        private IAnalyticsManager _analyticsManager;

        private IAppStateManager _appStateManager;

        private INetworkActionManager _networkActionManager;

        private OverlordsTalkingController _overlordsChatController;

        private HandPointerController _handPointerController;

        private List<TutorialDescriptionTooltipItem> _tutorialDescriptionTooltipItems;

        private List<Enumerators.TutorialActivityAction> _activitiesDoneDuringThisTurn;

        private List<Sequence> _overlordSaysPopupSequences;

        private List<string> _buttonsWasDeactivatedPreviousStep;

        public bool IsTutorial { get; private set; }

        private List<TutorialData> _tutorials;
        private List<TutorialStep> _tutorialSteps;
        private int _currentTutorialStepIndex;

        private List<InGameTutorialData> _ingameTutorials;

        private List<TutorialDescriptionTooltipItem> _ingameTutorialActiveTooltips;

        private bool _playerOrderScreenCloseManually;

        private List<Card> _cardsForOpenPack;

        //public TutorialReward RewardFromLastTutorial { get; private set; }

        public TutorialData CurrentTutorial { get; private set; }
        public TutorialStep CurrentTutorialStep { get; private set; }

        public AnalyticsTimer TutorialDuration { get; set; }

        public List<string> BlockedButtons { get; private set; }

        public event Action OnMenuStepUpdated;

        public bool BattleShouldBeWonBlocker { get; private set; }

        public bool PlayerWon { get; set; }

        public bool UnfinishedTutorial { get; set; }

        public int TutorialsCount
        {
            get { return _tutorials.FindAll(tutor => !tutor.HiddenTutorial).Count; }
        }

        public bool IsLastTutorial => CurrentTutorial.Id == _tutorials[_tutorials.Count - 1].Id;

        public void Dispose()
        {
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _networkActionManager = GameClient.Get<INetworkActionManager>();

            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _overlordsChatController = _gameplayManager.GetController<OverlordsTalkingController>();
            _handPointerController = _gameplayManager.GetController<HandPointerController>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            _overlordSaysPopupSequences = new List<Sequence>();

            JsonSerializerSettings settings =
                JsonUtility.CreateStrictSerializerSettings((sender, args) => Log.Error("", args.ErrorContext.Error));
            settings.TypeNameHandling = TypeNameHandling.Auto;

            _tutorials = JsonConvert.DeserializeObject<List<TutorialData>>(_loadObjectsManager
                        .GetObjectByPath<TextAsset>(TutorialDataPath).text, settings);

            _ingameTutorials = JsonConvert.DeserializeObject<List<InGameTutorialData>>(_loadObjectsManager
                        .GetObjectByPath<TextAsset>(InGameTutorialDataPath).text, settings);

            TutorialDuration = new AnalyticsTimer();

            _tutorialDescriptionTooltipItems = new List<TutorialDescriptionTooltipItem>();
            _activitiesDoneDuringThisTurn = new List<Enumerators.TutorialActivityAction>();
            _buttonsWasDeactivatedPreviousStep = new List<string>();
            BlockedButtons = new List<string>();
            _ingameTutorialActiveTooltips = new List<TutorialDescriptionTooltipItem>();
        }

        public bool CheckNextTutorial()
        {
            SetupTutorialById(_dataManager.CachedUserLocalData.CurrentTutorialId);

            if (CurrentTutorial != null && !CurrentTutorial.IsGameplayTutorial())
            {
                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);

                StartTutorial();

                return true;
            }

            return false;
        }

        public void Update()
        {
            if (!IsTutorial)
            {
                for (int i = 0; i < _ingameTutorialActiveTooltips.Count; i++)
                {
                    _ingameTutorialActiveTooltips[i]?.Update();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    ReportActivityAction(Enumerators.TutorialActivityAction.TapOnScreen, null);
                }
                return;
            }

            for (int i = 0; i < _tutorialDescriptionTooltipItems.Count; i++)
            {
                _tutorialDescriptionTooltipItems[i]?.Update();
            }

            if (!CurrentTutorial.IsGameplayTutorial())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    ReportActivityAction(Enumerators.TutorialActivityAction.TapOnScreen);
                }
            }
        }

        public bool IsButtonBlockedInTutorial(string name)
        {
            if (!IsTutorial && !BattleShouldBeWonBlocker)
                return false;

            return BlockedButtons.Contains(name);
        }

        public int GetIndexOfCurrentTutorial()
        {
            return _tutorials.FindAll(tutor => !tutor.HiddenTutorial)
                .FindIndex(info => info.Id == CurrentTutorial.Id);
        }

        public void SetupTutorialById(int id)
        {
            if (CheckAvailableTutorial())
            {
                CurrentTutorial = _tutorials.Find(tutor => tutor.Id == id);
                _currentTutorialStepIndex = 0;
                _tutorialSteps = CurrentTutorial.TutorialContent.TutorialSteps;
                CurrentTutorialStep = _tutorialSteps[_currentTutorialStepIndex];

                if (CurrentTutorial.IsGameplayTutorial() && !CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.DisabledInitialization)
                {
                    FillTutorialDeck();
                }
                else if(!CurrentTutorial.IsGameplayTutorial() && CurrentTutorial.Id != FirstDeckBuildTutorialIndex)
                {
                    ResetTutorialDeck();
                }

                ClearToolTips();
            }

            IsTutorial = false;
        }

        public bool CheckAvailableTutorial()
        {
            int id = _dataManager.CachedUserLocalData.CurrentTutorialId;

            TutorialData tutorial = _tutorials.Find((x) => !x.Ignore &&
                x.Id >= _dataManager.CachedUserLocalData.CurrentTutorialId);

            if (tutorial != null)
            {
                _dataManager.CachedUserLocalData.CurrentTutorialId = tutorial.Id;
                _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                return true;
            }
            return false;
        }

        public void StartTutorial()
        {
            if (IsTutorial)
                return;

            IsTutorial = true;

            if (CurrentTutorial.IsGameplayTutorial())
            {
                if (!CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.DisabledInitialization)
                {
                    _battlegroundController.SetupBattlegroundAsSpecific(CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo);
                }
                else if(CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.EnableCustomDeckForOpponent)
                {
                    _battlegroundController.SetOpponentDeckAsSpecific(CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo);
                }

                _battlegroundController.TurnStarted += TurnStartedHandler;

                _gameplayManager.GetController<InputController>().PlayerPointerEnteredEvent += PlayerSelectedEventHandler;
                _gameplayManager.GetController<InputController>().UnitPointerEnteredEvent += UnitSelectedEventHandler;
            }

            for (int i = 0; i < _tutorialSteps.Count; i++)
            {
                _tutorialSteps[i].IsDone = false;
            }
            BattleShouldBeWonBlocker = false;
            PlayerWon = false;
            UnfinishedTutorial = false;

            ClearToolTips();
            EnableStepContent(CurrentTutorialStep);

            if(CurrentTutorial.Id == FirstDeckBuildTutorialIndex)
            {
                RemoveTutorialDeck();
            }

            StartTutorialEvent(CurrentTutorial.Id);
        }

        private void StartTutorialEvent(int currentTutorialId)
        {
            switch (currentTutorialId)
            {
                // Basic
                case 0:
                    SetStartTutorialEvent(AnalyticsManager.EventStartedTutorialBasic);
                    break;

                // Deck
                case 1:
                    SetStartTutorialEvent(AnalyticsManager.EventStartedTutorialDeck);
                    break;

                // Abilities
                case 2:
                    SetStartTutorialEvent(AnalyticsManager.EventStartedTutorialAbilities);
                    break;

                // battle
                case 4:
                    SetStartTutorialEvent(AnalyticsManager.EventStartedTutorialBattle);
                    break;
            }
        }

        private void SetStartTutorialEvent(string eventName)
        {
            TutorialDuration.StartTimer();
            _analyticsManager.SetEvent(eventName);
        }

        private void PlayerSelectedEventHandler(Player player)
        {
            SetTooltipsByOwnerIfHas(player.IsLocalPlayer ? Enumerators.TutorialObjectOwner.PlayerOverlord : Enumerators.TutorialObjectOwner.EnemyOverlord);
        }

        private void UnitSelectedEventHandler(BoardUnitView unit)
        {
            SetTooltipsStateIfHas(unit.Model.TutorialObjectId, true);
        }

        private void UnitDeselectedEventHandler(BoardUnitView unit)
        {
            SetTooltipsStateIfHas(unit.Model.TutorialObjectId, false);
        }

        private void TurnStartedHandler()
        {
            _activitiesDoneDuringThisTurn.Clear();
        }

        public void StopTutorial(bool isManual = false)
        {
            if (!IsTutorial)
                return;

            if (CurrentTutorial.IsGameplayTutorial())
            {
                _battlegroundController.TurnStarted -= TurnStartedHandler;

                _gameplayManager.GetController<InputController>().PlayerPointerEnteredEvent -= PlayerSelectedEventHandler;
                _gameplayManager.GetController<InputController>().UnitPointerEnteredEvent -= UnitSelectedEventHandler;
            }

            _uiManager.HidePopup<TutorialAvatarPopup>();

            _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);

            if (BattleShouldBeWonBlocker && !isManual)
                return;

            ClearToolTips();

            if (!UnfinishedTutorial)
            {
                _dataManager.CachedUserLocalData.CurrentTutorialId++;

                if (CurrentTutorial.IsGameplayTutorial())
                {
                    ApplyReward();
                }
            }

            if (_dataManager.CachedUserLocalData.CurrentTutorialId >= _tutorials.Count)
            {
                _dataManager.CachedUserLocalData.CurrentTutorialId = 0;
                CompletelyFinishTutorial();
            }

            if (!CheckAvailableTutorial())
            {
                _gameplayManager.IsTutorial = false;
                _dataManager.CachedUserLocalData.Tutorial = false;
                _gameplayManager.IsSpecificGameplayBattleground = false;
            }


            _buttonsWasDeactivatedPreviousStep.Clear();

            IsTutorial = false;
            BattleShouldBeWonBlocker = false;
            _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);

            CompleteTutorialEvent(CurrentTutorial.Id);
        }

        private void CompletelyFinishTutorial()
        {
            RemoveTutorialDeck();
            _analyticsManager.SetEvent(AnalyticsManager.EventDeckDeleted);
            _analyticsManager.SetEvent(AnalyticsManager.SkipTutorial);

            _gameplayManager.IsTutorial = false;
            _dataManager.CachedUserLocalData.Tutorial = false;
            _gameplayManager.IsSpecificGameplayBattleground = false;
        }

        private void CompleteTutorialEvent(int currentTutorialId)
        {
            switch (currentTutorialId)
            {
                // Basic
                case 0:
                    SetCompleteTutorialEvent(AnalyticsManager.EventCompletedTutorialBasic);
                    break;

                // Deck
                case 1:
                    SetCompleteTutorialEvent(AnalyticsManager.EventCompletedTutorialDeck);
                    break;

                // Abilities
                case 2:
                    SetCompleteTutorialEvent(AnalyticsManager.EventCompletedTutorialAbilities);
                    break;

                // battle
                case 4:
                    SetCompleteTutorialEvent(AnalyticsManager.EventCompletedTutorialBattle);
                    break;
            }
        }

        private void SetCompleteTutorialEvent(string eventName)
        {
            TutorialDuration.FinishTimer();
            Dictionary<string, object> eventParameters = new Dictionary<string, object>();
            eventParameters.Add(AnalyticsManager.PropertyTutorialTimeToComplete, TutorialDuration.GetTimeDifference());
            _analyticsManager.SetEvent(eventName, eventParameters);
        }

        public SpecificTurnInfo GetCurrentTurnInfo()
        {
            if (!IsTutorial)
                return null;

            return CurrentTutorial.TutorialContent.ToGameplayContent().SpecificTurnInfos.Find(x => x.TurnIndex == _battlegroundController.CurrentTurn);
        }

        public bool IsCompletedActivitiesForThisTurn()
        {
            if (!IsTutorial)
                return true;

            if (GetCurrentTurnInfo() != null)
            {
                foreach (Enumerators.TutorialActivityAction activityAction in GetCurrentTurnInfo().RequiredActivitiesToDoneDuringTurn)
                {
                    if (!_activitiesDoneDuringThisTurn.Contains(activityAction))
                        return false;
                }
            }

            return true;
        }

        public void ReportActivityAction(Enumerators.TutorialActivityAction activityAction, IBoardObject sender, string tag = "")
        {
            if (!IsTutorial && Constants.UsingCardTooltips)
            {
                HandleNonTutorialActions(activityAction, sender, tag);
            }
        }

        public void ReportActivityAction(Enumerators.TutorialActivityAction action, int sender = 0)
        {
            if (!IsTutorial)
                return;

            if (action == Enumerators.TutorialActivityAction.TapOnScreen)
            {
                HideAllActiveDescriptionTooltip();
            }

            if (CurrentTutorial.TutorialContent.ActionActivityHandlers != null)
            {
                foreach (ActionActivityHandler activityHandler in CurrentTutorial.TutorialContent.ActionActivityHandlers)
                {
                    if (activityHandler.TutorialActivityAction == action && !activityHandler.HasSpecificConnection)
                    {
                        DoActionByActivity(activityHandler);
                        break;
                    }
                }
            }

            if (CurrentTutorial.IsGameplayTutorial())
            {
                if (_battlegroundController.CurrentTurn > 1)
                {
                    SpecificTurnInfo specificTurnInfo = GetCurrentTurnInfo();

                    if (specificTurnInfo != null)
                    {
                        if (specificTurnInfo.ActionActivityHandlers != null)
                        {
                            foreach (ActionActivityHandler activity in specificTurnInfo.ActionActivityHandlers)
                            {
                                if (!_activitiesDoneDuringThisTurn.Contains(activity.ConnectedTutorialActivityAction) &&
                                    activity.TutorialActivityAction == action)
                                {
                                    DoActionByActivity(activity);
                                    break;
                                }
                            }
                        }

                        if (specificTurnInfo.RequiredActivitiesToDoneDuringTurn != null)
                        {
                            if (specificTurnInfo.RequiredActivitiesToDoneDuringTurn.Contains(action) &&
                               action != CurrentTutorialStep.ActionToEndThisStep)
                            {

                                List<TutorialStep> steps = CurrentTutorial.TutorialContent.TutorialSteps.FindAll(x =>
                                                             x.ToGameplayStep().ConnectedTurnIndex == specificTurnInfo.TurnIndex);

                                foreach (TutorialStep step in steps)
                                {
                                    if (step.ActionToEndThisStep == action && !step.IsDone)
                                    {
                                        if (step.ToGameplayStep().TutorialObjectIdStepOwner == 0 || step.ToGameplayStep().TutorialObjectIdStepOwner == sender)
                                        {
                                            step.IsDone = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _overlordsChatController.UpdatePopupsByReportActivityAction(action);
            }

            if (CurrentTutorialStep.ConnectedActivities != null)
            {
                ActionActivityHandler handler;
                foreach (int id in CurrentTutorialStep.ConnectedActivities)
                {
                    handler = CurrentTutorial.TutorialContent.ActionActivityHandlers.Find(x => x.Id == id && x.HasSpecificConnection);

                    if (handler != null)
                    {
                        if (handler.TutorialActivityAction == action)
                        {
                            DoActionByActivity(handler);
                            break;
                        }
                    }
                }
            }

            CheckTooltips(action, sender);

            if (CurrentTutorial.IsGameplayTutorial())
            {
                _activitiesDoneDuringThisTurn.Add(action);
            }

            if (CurrentTutorialStep != null && action == CurrentTutorialStep.ActionToEndThisStep)
            {
                MoveToNextStep();
            }
        }

        private void HandleNonTutorialActions(Enumerators.TutorialActivityAction activityAction, IBoardObject sender, string tag = "")
        {
            ResetInGameTutorialActiveTooltips();

            if (activityAction == Enumerators.TutorialActivityAction.TapOnScreen)
                return;

            foreach (InGameTutorialData item in _ingameTutorials)
            {
                if (item.ActivityAction == activityAction && item.IsEnabled &&
                   _dataManager.CachedUserLocalData.TutorialTooltipsPassed != null &&
                   !_dataManager.CachedUserLocalData.TutorialTooltipsPassed.Contains(item.Id))
                {
                    if ((!string.IsNullOrEmpty(item.Tag) && item.Tag.ToLowerInvariant() == tag.ToLowerInvariant()) ||
                        string.IsNullOrEmpty(item.Tag))
                    {
                        InternalTools.DoActionDelayed(() =>
                        {
                            TutorialDescriptionTooltipItem tooltipItem = new TutorialDescriptionTooltipItem(item.Id,
                                                                                            item.Description,
                                                                                            item.Align,
                                                                                            item.Owner,
                                                                                            (Vector3)item.Position,
                                                                                            true,
                                                                                            false,
                                                                                            layer: Enumerators.TutorialObjectLayer.Default,
                                                                                            boardObjectOwner: sender);
                            _ingameTutorialActiveTooltips.Add(tooltipItem);

                            if (item.AppearOnce)
                            {
                                _dataManager.CachedUserLocalData.TutorialTooltipsPassed.Add(item.Id);
                                _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                            }
                        }, item.AppearDelay);
                    }
                }
            }
        }

        private void ResetInGameTutorialActiveTooltips()
        {
            if (_ingameTutorialActiveTooltips.Count > 0)
            {
                foreach (TutorialDescriptionTooltipItem item in _ingameTutorialActiveTooltips)
                {
                    item.Dispose();
                }
                _ingameTutorialActiveTooltips.Clear();
            }
        }

        private void CheckTooltips(Enumerators.TutorialActivityAction action, int sender = 0)
        {
            Enumerators.TutorialObjectOwner owner;
            switch (action)
            {
                case Enumerators.TutorialActivityAction.BattleframeSelected:
                    SetTooltipsStateIfHas(sender, true);
                    break;
                case Enumerators.TutorialActivityAction.EnemyOverlordSelected:
                case Enumerators.TutorialActivityAction.PlayerOverlordSelected:
                    {
                        owner = action == Enumerators.TutorialActivityAction.PlayerOverlordSelected ?
                            Enumerators.TutorialObjectOwner.PlayerOverlord :
                            Enumerators.TutorialObjectOwner.EnemyOverlord;
                        SetTooltipsByOwnerIfHas(owner);
                    }
                    break;
                case Enumerators.TutorialActivityAction.PlayerManaBarSelected:
                    if (action == Enumerators.TutorialActivityAction.PlayerManaBarSelected)
                    {
                        SetTooltipsByOwnerIfHas(Enumerators.TutorialObjectOwner.PlayerGooBottles);
                    }
                    break;
                case Enumerators.TutorialActivityAction.PlayerCardInHandSelected:
                    SetTooltipsByOwnerIfHas(Enumerators.TutorialObjectOwner.PlayerCardInHand);
                    break;
                case Enumerators.TutorialActivityAction.IncorrectButtonTapped:
                    SetTooltipsByOwnerIfHas(Enumerators.TutorialObjectOwner.IncorrectButton);
                    break;
                default:
                    break;
            }
        }

        private void SetIncorrectButtonTooltip()
        {
            List<TutorialDescriptionTooltipItem> tooltips = _tutorialDescriptionTooltipItems.FindAll(x => x.OwnerType == Enumerators.TutorialObjectOwner.IncorrectButton);
            foreach (TutorialDescriptionTooltipItem tooltip in tooltips)
            {

            }
        }

        private void MoveToNextStep()
        {
            if (CurrentTutorialStep != null)
            {
                _handPointerController.ResetAll();
                ClearOverlordSaysPopupSequences();
                CurrentTutorialStep.IsDone = true;
            }

            if (_currentTutorialStepIndex + 1 >= _tutorialSteps.Count)
            {
                ClearToolTips();

                if (!CurrentTutorial.IsGameplayTutorial())
                {
                    StopTutorial();
                }
            }
            else
            {
                CurrentTutorialStep = GetNextNotDoneStep();

                EnableStepContent(CurrentTutorialStep);
            }
        }

        private TutorialStep GetNextNotDoneStep()
        {
            for (int i = _currentTutorialStepIndex + 1; i < _tutorialSteps.Count; i++)
            {
                if (!_tutorialSteps[i].IsDone)
                {
                    _currentTutorialStepIndex = i;
                    return _tutorialSteps[i];
                }
            }
            return _tutorialSteps[_currentTutorialStepIndex];
        }

        private async void EnableStepContent(TutorialStep step)
        {
            HideAllActiveDescriptionTooltip();

            _handPointerController.ResetAll();

            if (step.HandPointers != null)
            {
                foreach (HandPointerInfo handPointer in step.HandPointers)
                {
                    DrawPointer(handPointer.TutorialHandPointerType,
                                handPointer.TutorialHandPointerOwner,
                                (Vector3)handPointer.StartPosition,
                                (Vector3)handPointer.EndPosition,
                                handPointer.AppearDelay,
                                handPointer.AppearOnce,
                                handPointer.TutorialObjectIdStepOwner,
                                handPointer.TargetTutorialObjectId,
                                handPointer.AdditionalObjectIdOwners,
                                handPointer.AdditionalObjectIdTargets,
                                handPointer.TutorialHandLayer,
                                handPointer.HandPointerSpeed,
                                handPointer.TutorialUIElementOwnerName,
                                handPointer.Rotation);
                }
            }

            if (step.TutorialDescriptionTooltipsToActivate != null)
            {
                foreach (int tooltipId in step.TutorialDescriptionTooltipsToActivate)
                {
                    TutorialDescriptionTooltip tooltip = CurrentTutorial.TutorialContent.TutorialDescriptionTooltips.Find(x => x.Id == tooltipId);

                    DrawDescriptionTooltip(tooltip.Id,
                                           tooltip.Description,
                                           tooltip.TutorialTooltipAlign,
                                           tooltip.TutorialTooltipOwner,
                                           tooltip.TutorialTooltipOwnerId,
                                           (Vector3)tooltip.Position,
                                           tooltip.Resizable,
                                           tooltip.AppearDelay,
                                           tooltip.DynamicPosition,
                                           tooltip.TutorialTooltipLayer,
                                           tooltip.MinimumShowTime,
                                           tooltip.TutorialUIElementOwnerName);
                }
            }

            if (step.TutorialDescriptionTooltipsToDeactivate != null)
            {
                foreach (int tooltipId in step.TutorialDescriptionTooltipsToDeactivate)
                {
                    DeactivateDescriptionTooltip(tooltipId);
                }
            }

            if (step.TutorialAvatar != null)
            {
                DrawAvatar(step.TutorialAvatar.Description, step.TutorialAvatar.DescriptionTooltipCloseText, step.TutorialAvatar.Pose, step.TutorialAvatar.AboveUI);
            }

            if (!string.IsNullOrEmpty(step.SoundToPlay))
            {
                PlayTutorialSound(step.SoundToPlay, step.SoundToPlayBeginDelay);
            }

            BlockedButtons.Clear();

            if (step.BlockedButtons != null)
            {
                BlockedButtons.AddRange(step.BlockedButtons);
            }

            switch (step)
            {
                case TutorialGameplayStep gameStep:
                    if (gameStep.OverlordSayTooltips != null)
                    {
                        foreach (OverlordSayTooltipInfo tooltip in gameStep.OverlordSayTooltips)
                        {
                            DrawOverlordSayPopup(tooltip.Description,
                                                tooltip.TutorialTooltipAlign,
                                                tooltip.TutorialTooltipOwner,
                                                tooltip.AppearDelay,
                                                true,
                                                tooltip.Duration,
                                                tooltip.SoundToPlay,
                                                tooltip.SoundToPlayBeginDelay,
                                                tooltip.ActionToHideThisPopup,
                                                tooltip.MinimumShowTime);
                        }
                    }

                    if(gameStep.LaunchGameplayManually)
                    {
                        _battlegroundController.StartGameplayTurns();
                    }

                    if (gameStep.PlayerOverlordAbilityShouldBeUnlocked)
                    {
                        if (!CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.DisabledInitialization &&
                            CurrentTutorial.TutorialContent.ToGameplayContent().
                            SpecificBattlegroundInfo.PlayerInfo.PrimarySkill != Enumerators.Skill.NONE)
                        {
                            _gameplayManager.GetController<SkillsController>().PlayerPrimarySkill.SetCoolDown(0);
                        }
                    }

                    if (gameStep.MatchShouldBePaused)
                    {
                        Time.timeScale = 0;
                    }
                    else
                    {
                        if (Time.timeScale == 0)
                        {
                            Time.timeScale = 1;
                        }
                    }

                    if (gameStep.LaunchAIBrain || (!gameStep.AIShouldBePaused && _gameplayManager.GetController<AIController>().AIPaused))
                    {
                        await _gameplayManager.GetController<AIController>().LaunchAIBrain();
                    }

                    if(_gameplayManager.GetController<AIController>().IsBrainWorking)
                    {
                        await _gameplayManager.GetController<AIController>().SetTutorialStep();
                    }

                    if (gameStep.ActionToEndThisStep == Enumerators.TutorialActivityAction.YouWonPopupOpened)
                    {
                        GameClient.Get<IGameplayManager>().EndGame(Enumerators.EndGameType.WIN, 0);
                    }

                    if (CurrentTutorial.TutorialContent.ToGameplayContent().GameplayFlowBeginsManually && gameStep.BeginGameplayFlowManually)
                    {
                        (_gameplayManager as GameplayManager).TutorialStartAction?.Invoke();
                    }

                    if (!gameStep.PlayerOrderScreenCloseManually && _playerOrderScreenCloseManually)
                    {
                        InternalTools.DoActionDelayed(() =>
                        {
                            _uiManager.GetPopup<PlayerOrderPopup>().AnimationEnded();
                        }, Time.deltaTime);
                    }
                    _playerOrderScreenCloseManually = gameStep.PlayerOrderScreenCloseManually;

                    break;
                case TutorialMenuStep menuStep:
                    if (!string.IsNullOrEmpty(menuStep.OpenScreen))
                    {
                        if (menuStep.OpenScreen.EndsWith("Popup"))
                        {
                            _uiManager.DrawPopupByName(menuStep.OpenScreen);
                        }
                        else if (menuStep.OpenScreen.EndsWith("Page"))
                        {
                            _uiManager.SetPageByName(menuStep.OpenScreen);
                        }
                    }

                    if(menuStep.BattleShouldBeWonBlocker && !PlayerWon)
                    {
                        BattleShouldBeWonBlocker = true;
                    }

                    OnMenuStepUpdated?.Invoke();

                    break;
            }
        }

        public void SetStatusOfButtonsByNames(List<string> buttons, bool status)
        {
            GameObject buttonObject;
            Button buttonComponent;

            foreach (string button in buttons)
            {
                buttonObject = GameObject.Find(button);

                if (buttonObject != null)
                {
                    buttonComponent = buttonObject.GetComponent<Button>();

                    if (buttonComponent != null && buttonComponent)
                    {
                        buttonComponent.interactable = status;

                        if (!status)
                        {
                            _buttonsWasDeactivatedPreviousStep.Add(button);
                        }
                    }
                    else
                    {
                        MenuButtonNoGlow menuButtonNoGlow = buttonObject.GetComponent<MenuButtonNoGlow>();
                        if (menuButtonNoGlow != null && menuButtonNoGlow)
                        {
                            menuButtonNoGlow.enabled = status;
                            if (!status)
                            {
                                _buttonsWasDeactivatedPreviousStep.Add(button);
                            }
                        }
                        else
                        {
                            buttonObject.SetActive(status);
                            if (!status)
                            {
                                _buttonsWasDeactivatedPreviousStep.Add(button);
                            }
                        }
                    }
                }
            }
        }

        public string GetCardNameByTutorialObjectId(int id)
        {
            SpecificBattlegroundInfo battleInfo = CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo;

            List<SpecificBattlegroundInfo.OverlordCardInfo> cards = new List<SpecificBattlegroundInfo.OverlordCardInfo>();



            cards.AddRange(battleInfo.PlayerInfo.CardsInDeck);
            cards.AddRange(battleInfo.PlayerInfo.CardsInHand);
            cards.AddRange(battleInfo.PlayerInfo.CardsOnBoard.Select((info) => new SpecificBattlegroundInfo.OverlordCardInfo()
            {
                Name = info.Name,
                TutorialObjectId = info.TutorialObjectId
            })
            .ToList());
            cards.AddRange(battleInfo.OpponentInfo.CardsInDeck);
            cards.AddRange(battleInfo.OpponentInfo.CardsInHand);

            return cards.Find(x => x.TutorialObjectId == id)?.Name;
        }

        public void SetTooltipsStateIfHas(int ownerId, bool isActive)
        {
            if (ownerId == 0)
                return;

            TutorialStep step;

            List<TutorialDescriptionTooltip> tooltips = CurrentTutorial.TutorialContent.TutorialDescriptionTooltips.FindAll(tooltip => tooltip.TutorialTooltipOwnerId == ownerId &&
                (tooltip.TutorialTooltipOwner == Enumerators.TutorialObjectOwner.EnemyBattleframe ||
                tooltip.TutorialTooltipOwner == Enumerators.TutorialObjectOwner.PlayerBattleframe));

            foreach (TutorialDescriptionTooltip tooltip in tooltips)
            {
                step = CurrentTutorial.TutorialContent.TutorialSteps.Find(info => info.ToGameplayStep().TutorialDescriptionTooltipsToActivate.Exists(id => id == tooltip.Id));
                if (step != null && (step.IsDone || step == CurrentTutorialStep))
                {
                    ActivateDescriptionTooltip(tooltip.Id);
                }
            }
        }

        public void SetTooltipsByOwnerIfHas(Enumerators.TutorialObjectOwner owner)
        {
            if (_gameplayManager.GetController<BoardArrowController>().CurrentBoardArrow != null)
                return;

            List<TutorialDescriptionTooltipItem> tooltips = _tutorialDescriptionTooltipItems.FindAll(x => x.OwnerType == owner);

            if (tooltips.Count > 0)
            {
                foreach (TutorialDescriptionTooltipItem tooltip in tooltips)
                {
                    ActivateDescriptionTooltip(tooltip.Id);
                }
            }
        }

        public void SetupBattleground(SpecificBattlegroundInfo specificBattleground)
        {
            _battlegroundController.SetupBattlegroundAsSpecific(specificBattleground);
        }

        public void FillTutorialDeck()
        {
            _gameplayManager.CurrentPlayerDeck =
                         new Deck(new DeckId(0), CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.PlayerInfo.OverlordId,
                         "TutorialDeck", new List<DeckCardData>(),
                         CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.PlayerInfo.PrimarySkill,
                         CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.PlayerInfo.SecondarySkill);

            _gameplayManager.OpponentPlayerDeck =
                        new Deck(new DeckId(0), CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.OpponentInfo.OverlordId,
                        "TutorialDeckOpponent", new List<DeckCardData>(),
                        CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.OpponentInfo.PrimarySkill,
                        CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.OpponentInfo.SecondarySkill);
        }

        public void PlayTutorialSound(string sound, float delay = 0f)
        {
            InternalTools.DoActionDelayed(() =>
            {
                _soundManager.PlaySound(Enumerators.SoundType.TUTORIAL, 0, sound, Constants.TutorialSoundVolume, false);
            }, delay);
        }

        public void DrawAvatar(string description, string hideAvatarButtonText, Enumerators.TutorialAvatarPose pose, bool aboveUI)
        {
            _uiManager.DrawPopup<TutorialAvatarPopup>(new object[]
            {
                description,
                hideAvatarButtonText,
                pose,
                aboveUI
            });
        }

        public void DrawPointer(Enumerators.TutorialHandPointerType type,
                                Enumerators.TutorialObjectOwner owner,
                                Vector3 begin,
                                Vector3? end = null,
                                float appearDelay = 0,
                                bool appearOnce = false,
                                int tutorialObjectIdStepOwner = 0,
                                int targetTutorialObjectId = 0,
                                List<int> additionalObjectIdOwners = null,
                                List<int> additionalObjectIdTargets = null,
                                Enumerators.TutorialObjectLayer handLayer = Enumerators.TutorialObjectLayer.Default,
                                float handPointerSpeed = Constants.HandPointerSpeed,
                                string tutorialUIElementOwnerName = Constants.Empty,
                                float rotation = 0)
        {
            _handPointerController.DrawPointer(type,
                                               owner,
                                               begin,
                                               end,
                                               appearDelay,
                                               appearOnce,
                                               tutorialObjectIdStepOwner,
                                               targetTutorialObjectId,
                                               additionalObjectIdOwners,
                                               additionalObjectIdTargets,
                                               handLayer,
                                               handPointerSpeed,
                                               tutorialUIElementOwnerName,
                                               rotation);
        }

        public void DrawDescriptionTooltip(int id,
                                           string description,
                                           Enumerators.TooltipAlign align,
                                           Enumerators.TutorialObjectOwner owner,
                                           int ownerId,
                                           Vector3 position,
                                           bool resizable,
                                           float appearDelay,
                                           bool dynamicPosition,
                                           Enumerators.TutorialObjectLayer layer = Enumerators.TutorialObjectLayer.Default,
                                           float minimumShowTime = Constants.DescriptionTooltipMinimumShowTime,
                                           string tutorialUIElementOwnerName = Constants.Empty)
        {
            if (appearDelay > 0)
            {
                InternalTools.DoActionDelayed(() =>
                {
                    TutorialDescriptionTooltipItem tooltipItem = new TutorialDescriptionTooltipItem(id,
                                                                                                    description,
                                                                                                    align,
                                                                                                    owner,
                                                                                                    position,
                                                                                                    resizable,
                                                                                                    dynamicPosition,
                                                                                                    ownerId,
                                                                                                    layer,
                                                                                                    minimumShowTime: minimumShowTime,
                                                                                                    tutorialUIElementOwnerName: tutorialUIElementOwnerName);

                    _tutorialDescriptionTooltipItems.Add(tooltipItem);
                }, appearDelay);
            }
            else
            {
                TutorialDescriptionTooltipItem tooltipItem = new TutorialDescriptionTooltipItem(id,
                                                                                                description,
                                                                                                align,
                                                                                                owner,
                                                                                                position,
                                                                                                resizable,
                                                                                                dynamicPosition,
                                                                                                ownerId,
                                                                                                layer,
                                                                                                minimumShowTime: minimumShowTime,
                                                                                                tutorialUIElementOwnerName: tutorialUIElementOwnerName);

                _tutorialDescriptionTooltipItems.Add(tooltipItem);
            }
        }

        public void ActivateDescriptionTooltip(int id)
        {
            TutorialDescriptionTooltipItem tooltip = _tutorialDescriptionTooltipItems.Find(x => x.Id == id);

            if (tooltip == null)
            {
                TutorialDescriptionTooltip tooltipInfo = CurrentTutorial.TutorialContent.TutorialDescriptionTooltips.Find(x => x.Id == id);

                DrawDescriptionTooltip(tooltipInfo.Id,
                                       tooltipInfo.Description,
                                       tooltipInfo.TutorialTooltipAlign,
                                       tooltipInfo.TutorialTooltipOwner,
                                       tooltipInfo.TutorialTooltipOwnerId,
                                       (Vector3)tooltipInfo.Position,
                                       tooltipInfo.Resizable,
                                       tooltipInfo.AppearDelay,
                                       tooltipInfo.DynamicPosition,
                                       tooltipInfo.TutorialTooltipLayer,
                                       tooltipInfo.MinimumShowTime,
                                       tooltipInfo.TutorialUIElementOwnerName);
            }
            else
            {
                tooltip.Show();
            }
        }

        public void ActivateDescriptionTooltipByOwner(Enumerators.TutorialObjectOwner owner, Vector3 position)
        {
            TutorialDescriptionTooltipItem tooltip = _tutorialDescriptionTooltipItems.Find(x => x.OwnerType == owner);

            if (tooltip == null)
            {
                TutorialDescriptionTooltip tooltipInfo = CurrentTutorial.TutorialContent.TutorialDescriptionTooltips.Find(x => x.TutorialTooltipOwner == owner);

                if (tooltipInfo == null)
                    return;

                DrawDescriptionTooltip(tooltipInfo.Id,
                                       tooltipInfo.Description,
                                       tooltipInfo.TutorialTooltipAlign,
                                       tooltipInfo.TutorialTooltipOwner,
                                       tooltipInfo.TutorialTooltipOwnerId,
                                       position,
                                       tooltipInfo.Resizable,
                                       tooltipInfo.AppearDelay,
                                       tooltipInfo.DynamicPosition,
                                       tooltipInfo.TutorialTooltipLayer,
                                       tooltipInfo.MinimumShowTime,
                                       tooltipInfo.TutorialUIElementOwnerName);
            }
            else
            {
                tooltip.Show(position);
            }
        }

        public TutorialDescriptionTooltipItem GetDescriptionTooltip(int id)
        {
            return _tutorialDescriptionTooltipItems.Find(x => x.Id == id);
        }

        public void HideDescriptionTooltip(int id)
        {
            _tutorialDescriptionTooltipItems.Find(x => x.Id == id)?.Hide();
        }

        public void HideAllActiveDescriptionTooltip()
        {
            for (int i = 0; i < _tutorialDescriptionTooltipItems.Count; i++)
            {
                _tutorialDescriptionTooltipItems[i]?.Hide();
            }
        }

        public void DeactivateDescriptionTooltip(int id)
        {
            TutorialDescriptionTooltipItem tooltip = _tutorialDescriptionTooltipItems.Find(x => x.Id == id);

            if (tooltip != null)
            {
                tooltip.Dispose();
                _tutorialDescriptionTooltipItems.Remove(tooltip);
            }
        }

        private void ClearToolTips()
        {
            foreach (TutorialDescriptionTooltipItem tooltip in _tutorialDescriptionTooltipItems)
            {
                tooltip.Dispose();
            }
            _tutorialDescriptionTooltipItems.Clear();
        }

        public void DrawOverlordSayPopup(string description,
                                        Enumerators.TooltipAlign align,
                                        Enumerators.TutorialObjectOwner owner,
                                        float appearDelay,
                                        bool ofStep = false,
                                        float duration = Constants.OverlordTalkingPopupDuration,
                                        string soundToPlay = Constants.Empty,
                                        float soundToPlayBeginDelay = 0,
                                        Enumerators.TutorialActivityAction actionToHideThisPopup = Enumerators.TutorialActivityAction.Undefined,
                                        float minimumShowTime = Constants.OverlordTalkingPopupMinimumShowTime)
        {
            Sequence sequence = InternalTools.DoActionDelayed(() =>
            {
                _overlordsChatController.DrawOverlordSayPopup(description, align, owner, duration, soundToPlay, soundToPlayBeginDelay, actionToHideThisPopup, minimumShowTime);
            }, appearDelay);

            if (ofStep)
            {
                _overlordSaysPopupSequences.Add(sequence);
            }
        }

        private void ClearOverlordSaysPopupSequences()
        {
            foreach (Sequence sequence in _overlordSaysPopupSequences)
            {
                sequence?.Kill();
            }
            _overlordSaysPopupSequences.Clear();
        }

        public void ActivateSelectHandPointer(Enumerators.TutorialObjectOwner owner)
        {
            _handPointerController.ChangeVisibilitySelectHandPointer(owner, true);
        }

        public void DeactivateSelectHandPointer(Enumerators.TutorialObjectOwner owner)
        {
            _handPointerController.ChangeVisibilitySelectHandPointer(owner, false);
        }

        private void DoActionByActivity(ActionActivityHandler activity)
        {
            switch (activity.TutorialActivityActionHandler)
            {
                case Enumerators.TutorialActivityActionHandler.OverlordSayTooltip:
                    {
                        OverlordSayTooltipInfo data = activity.TutorialActivityActionHandlerData as OverlordSayTooltipInfo;
                        DrawOverlordSayPopup(data.Description,
                                            data.TutorialTooltipAlign,
                                            data.TutorialTooltipOwner,
                                            data.AppearDelay,
                                            duration: data.Duration,
                                            soundToPlay: data.SoundToPlay,
                                            soundToPlayBeginDelay: data.SoundToPlayBeginDelay,
                                            actionToHideThisPopup: data.ActionToHideThisPopup,
                                            minimumShowTime: data.MinimumShowTime);
                    }
                    break;
                case Enumerators.TutorialActivityActionHandler.DrawDescriptionTooltips:
                    {
                        DrawDescriptionTooltipsInfo data = activity.TutorialActivityActionHandlerData as DrawDescriptionTooltipsInfo;
                        foreach (int id in data.TutorialDescriptionTooltipsToActivate)
                        {
                            ActivateDescriptionTooltip(id);
                        }
                    }
                    break;
            }
        }

        public List<Card> GetSpecificCardsBySet(Enumerators.Faction faction)
        {
            List<Card> cards = null;
            if(CurrentTutorial != null && CurrentTutorial.TutorialContent.ToMenusContent() != null)
            {
                cards = CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.CardsForArmy
                    .Select(cardInfo => _dataManager.CachedCardsLibraryData.GetCardByName(cardInfo.CardName))
                    .ToList()
                    .FindAll(card => card.Faction == faction)
                    .OrderBy(sort => sort.Cost)
                    .ToList();
            }
            return cards;
        }

        public SpecificHordeCardData GetCardData(string id)
        {
            SpecificHordeCardData cardData = null;
            if (CurrentTutorial != null && CurrentTutorial.TutorialContent.ToMenusContent() != null)
            {
                cardData = CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.CardsForArmy
                    .Find(info => info.CardName == id);
            }
            return cardData;
        }

        public List<Card> GetCardForCardPack(int count)
        {
            List<Card> cards = new List<Card>();
            if (_cardsForOpenPack == null || _cardsForOpenPack.Count == 0)
            {
                _cardsForOpenPack = CurrentTutorial.TutorialContent.TutorialReward.CardPackReward
                    .Select(card => _dataManager.CachedCardsLibraryData.GetCardByName(card.Name))
                    .ToList();
            }

            for (int i = 0; i < count; i++)
            {
                cards.Add(_cardsForOpenPack[0]);
                _cardsForOpenPack.Remove(_cardsForOpenPack[0]);
            }

            return cards;
        }

        public bool BlockAndReport(string buttonName)
        {
            if (IsButtonBlockedInTutorial(buttonName))
            {
                ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return true;
            }
            return false;
        }

        public bool CheckAvailableTooltipByOwnerId(int ownerId)
        {
            if (_tutorialDescriptionTooltipItems.FindAll(tooltip => tooltip.OwnerId == ownerId).Count > 0)
                return false;

            return true;
        }

        public void ApplyReward()
        {
            for (int i = 0; i < CurrentTutorial.TutorialContent.ToGameplayContent().RewardCardPackCount; i++)
            {
                //get card pack
            }
        }

        public void SkipTutorial()
        {
            _dataManager.CachedUserLocalData.CurrentTutorialId = _tutorials.Count;
            CompletelyFinishTutorial();
            StopTutorial(true);
            _handPointerController.ResetAll();
        }

        // FIXME: unused
        private async void CreateStarterDeck()
        {
            List<DeckCardData> cards = GetCardsForStarterDeck();

            Deck savedTutorialDeck = _dataManager.CachedUserLocalData.TutorialSavedDeck;
            if (savedTutorialDeck != null && _dataManager.CachedDecksData.Decks.Exists(deck => deck.Id == savedTutorialDeck.Id))
            {
                if(savedTutorialDeck.GetNumCards() < Constants.DeckMaxSize)
                {
                    savedTutorialDeck.Cards = cards;
                    await _backendFacade.EditDeck(_backendDataControlMediator.UserDataModel.UserId, savedTutorialDeck);
                }
            }
            else
            {
                string nameOfDeck = "HORDE " + _dataManager.CachedDecksData.Decks.Count;
                savedTutorialDeck = new Deck(new DeckId(-1), new OverlordId(4), nameOfDeck, cards, 0, 0);

                long newDeckId = await _backendFacade.AddDeck(_backendDataControlMediator.UserDataModel.UserId, savedTutorialDeck);
                savedTutorialDeck.Id = new DeckId(2);
                _dataManager.CachedDecksData.Decks.Add(savedTutorialDeck);
            }
            _dataManager.CachedUserLocalData.TutorialSavedDeck = savedTutorialDeck;
            _dataManager.CachedUserLocalData.LastSelectedDeckId = savedTutorialDeck.Id;
            await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
        }

        private async void RemoveTutorialDeck(Deck currentDeck = null)
        {
            if (currentDeck == null && _dataManager.CachedUserLocalData.TutorialSavedDeck != null)
            {
                currentDeck = _dataManager.CachedDecksData.Decks.Find(deck => deck.Id == _dataManager.CachedUserLocalData.TutorialSavedDeck.Id);

            }
            if (currentDeck == null)
                return;

            try
            {
                await _networkActionManager.EnqueueNetworkTask(async () =>
                    {
                        _dataManager.CachedDecksData.Decks.Remove(currentDeck);
                        _dataManager.CachedUserLocalData.LastSelectedDeckId = new DeckId(-1);
                        // TODO : Merge fixes
                        //_uiManager.GetPage<HordeSelectionWithNavigationPage>().AssignSelectedDeck(_dataManager.CachedDecksData.Decks[0]);
                        await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);

                        await _backendFacade.DeleteDeck(
                            _backendDataControlMediator.UserDataModel.UserId,
                            currentDeck.Id
                        );

                        Log.Info($" ====== Delete Deck {currentDeck.Id} Successfully ==== ");
                    },
                    onUnknownExceptionCallbackFunc: exception =>
                    {
                        _uiManager.DrawPopup<WarningPopup>($"Not able to Delete Deck {currentDeck.Id}: " + exception.Message);
                        return Task.CompletedTask;
                    }
                );
            }
            catch
            {
                // No additional handling
            }
        }

        private List<DeckCardData> GetCardsForStarterDeck()
        {
            List<DeckCardData> cards =
                _tutorials[_tutorials.Count - 2].TutorialContent.ToMenusContent().SpecificHordeInfo.CardsForArmy
                    .Select(data => new DeckCardData(_dataManager.CachedCardsLibraryData.GetCardByName(data.CardName).CardKey, data.Amount))
                    .ToList()
                    .FindAll(card => _dataManager.CachedCardsLibraryData.GetCardByCardKey(card.CardKey).Faction != Enumerators.Faction.FIRE);

            List<DeckCardData> filteredCards = new List<DeckCardData>();
            int countCards = 0;
            foreach (DeckCardData data in cards)
            {
                if (countCards >= Constants.DeckMaxSize)
                    break;

                if (countCards + data.Amount > Constants.DeckMaxSize)
                {
                    data.Amount = (int)Constants.DeckMaxSize - countCards;
                }
                countCards += data.Amount;
                filteredCards.Add(data);
            }

            return filteredCards;
        }

        private void ResetTutorialDeck()
        {
            Deck deck = null;
            if (_dataManager.CachedUserLocalData.TutorialSavedDeck != null)
            {
                deck = _dataManager.CachedDecksData.Decks.Find(cachedDeck => cachedDeck.Id == _dataManager.CachedUserLocalData.TutorialSavedDeck.Id);
            }
            DeckCardData cardInDeck = null;

            int maxCount = CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount;
            if (deck != null && deck.GetNumCards() >= maxCount)
            {
                foreach (CardRewardInfo rewardCard in CurrentTutorial.TutorialContent.ToMenusContent().TutorialReward.CardPackReward)
                {
                    cardInDeck = deck.Cards.Find(card => card.CardKey == _dataManager.CachedCardsLibraryData.GetCardByName(rewardCard.Name).CardKey);
                    if (cardInDeck != null)
                    {
                        cardInDeck.Amount -= 1;
                        if (cardInDeck.Amount < 1)
                        {
                            deck.Cards.Remove(cardInDeck);
                        }
                    }
                }

                if (deck.Cards.Count == 0)
                {
                    RemoveTutorialDeck(deck);
                }
            }
        }
    }
}
