using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class BoardSkill : IOwnableBoardObject, ISkillIdOwner
    {
        public event Action<BoardSkill, List<ParametrizedAbilityBoardObject>> SkillUsed;

        public BattleBoardArrow FightTargetingArrow;

        public GameObject SelfObject;

        public OverlordSkillPrototype Skill;

        public List<Enumerators.UnitSpecialStatus> BlockedUnitStatusTypes;

        private readonly ILoadObjectsManager _loadObjectsManager;

        private readonly IGameplayManager _gameplayManager;

        private readonly ITutorialManager _tutorialManager;

        private IOverlordExperienceManager _overlordExperienceManager;

        private readonly PlayerController _playerController;

        private readonly SkillsController _skillsController;

        private readonly BoardArrowController _boardArrowController;

        private readonly BattlegroundController _battlegroundController;

        private ActionsQueueController _actionsQueueController;

        private readonly GameObject _glowObject;

        private readonly GameObject _fightTargetingArrowPrefab;

        private readonly Animator _shutterAnimator;

        private readonly PointerEventSolver _pointerEventSolver;

        private int _cooldown;

        private bool _usedInThisTurn;

        private bool _isOpen;

        private bool _isAlreadyUsed;

        private OnBehaviourHandler _behaviourHandler;

        private OverlordAbilityInfoObject _currentOverlordAbilityInfoObject;

        private SkillCoolDownTimer _coolDownTimer;

        public SkillId SkillId { get; }

        public Player OwnerPlayer { get; }

        public BoardSkill(GameObject obj, Player player, OverlordSkillPrototype skillPrototype, bool isPrimary)
        {
            SelfObject = obj;
            Skill = skillPrototype;
            OwnerPlayer = player;
            IsPrimary = isPrimary;

            _cooldown = skillPrototype.Cooldown;

            BlockedUnitStatusTypes = new List<Enumerators.UnitSpecialStatus>();

            if(Skill.Skill == Enumerators.Skill.FREEZE)
            {
                BlockedUnitStatusTypes.Add(Enumerators.UnitSpecialStatus.FROZEN);
            }

            _coolDownTimer = new SkillCoolDownTimer(SelfObject, _cooldown);

            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _overlordExperienceManager = GameClient.Get<IOverlordExperienceManager>();

            _playerController = _gameplayManager.GetController<PlayerController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();

            _glowObject = SelfObject.transform.Find("OverlordAbilitySelection").gameObject;
            _glowObject.SetActive(false);

            string name = isPrimary ? Constants.OverlordRegularNeckR : Constants.OverlordRegularNeckL;

            _shutterAnimator = SelfObject.transform.parent.transform
                .Find("OverlordArea/RegularModel/RegularPosition/OverlordRegular/Shutters/" + name).GetComponent<Animator>();

            SkillId = new SkillId(isPrimary ? 0 : 1);

            OwnerPlayer.TurnStarted += TurnStartedHandler;
            OwnerPlayer.TurnEnded += TurnEndedHandler;

            _behaviourHandler = SelfObject.GetComponent<OnBehaviourHandler>();
            {
                _pointerEventSolver = new PointerEventSolver();
                _pointerEventSolver.DragStarted += PointerSolverDragStartedHandler;
                _pointerEventSolver.Clicked += PointerEventSolverClickedHandler;
                _pointerEventSolver.Ended += PointerEventSolverEndedHandler;
            }

            _coolDownTimer.SetAngle(_cooldown);

            _fightTargetingArrowPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");

            _isOpen = false;
        }

        public bool IsSkillReady => _cooldown == 0 && (!Skill.SingleUse || !_isAlreadyUsed);

        public bool IsUsing { get; private set; }

        public bool IsLocal { get; private set; }

        public bool IsPrimary { get; }

        public void CancelTargetingArrows()
        {
            if (FightTargetingArrow != null)
            {
                FightTargetingArrow.Dispose();
            }

            FightTargetingArrow = null;
        }

        public void BlockSkill()
        {
            _usedInThisTurn = true;
            SetHighlightingEnabled(false);
        }

        public void UnBlockSkill()
        {
            _usedInThisTurn = false;
        }

        public void SetCoolDown(int coolDownValue)
        {
            if (_isAlreadyUsed && Skill.SingleUse)
                return;

            _cooldown = coolDownValue;
            _coolDownTimer.SetAngle(_cooldown);

            SetHighlightingEnabled(IsSkillReady);
            _usedInThisTurn = false;
        }

        public void UseSkillFromEvent(List<ParametrizedAbilityBoardObject> parametrizedAbilityObjects)
        {
            StartDoSkill();

            if (parametrizedAbilityObjects.Count > 0)
            {
                if (Skill.CanSelectTarget)
                {
                    GameplayActionQueueAction skillUsageAction = _actionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.OverlordSkillUsageBlocker, blockQueue: true);

                    IBoardObject target = parametrizedAbilityObjects[0].BoardObject;

                    Action callback = () =>
                    {
                        switch (target)
                        {
                            case Player player:
                                FightTargetingArrow.SelectedPlayer = player;
                                break;
                            case CardModel cardModel:
                                FightTargetingArrow.SelectedCard = _gameplayManager.GetController<BattlegroundController>().GetCardViewByModel<BoardUnitView>(cardModel);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(target), target.GetType(), null);
                        }

                        EndDoSkill(parametrizedAbilityObjects);
                        skillUsageAction.TriggerActionExternally();
                    };

                    FightTargetingArrow = _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(SelfObject.transform, target, action: callback);
                }
                else
                {
                    EndDoSkill(parametrizedAbilityObjects);
                }
            }
            else
            {
                EndDoSkill(null);
            }
        }

        public void StartDoSkill(bool localPlayerOverride = false)
        {
            if (!IsSkillCanUsed())
                return;

            if (OwnerPlayer.IsLocalPlayer && !localPlayerOverride)
            {
                if (Skill.CanSelectTarget)
                {
                    if (_tutorialManager.IsTutorial)
                    {
                        if ((IsPrimary && !_tutorialManager.GetCurrentTurnInfo().UseOverlordSkillsSequence.Exists(info => info.SkillType == Enumerators.SkillType.PRIMARY)) ||
                            (!IsPrimary && !_tutorialManager.GetCurrentTurnInfo().UseOverlordSkillsSequence.Exists(info => info.SkillType == Enumerators.SkillType.SECONDARY)))
                        {
                            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordTriedToUseWrongBattleframe);
                            return;
                        }

                        _tutorialManager.DeactivateSelectHandPointer(Enumerators.TutorialObjectOwner.PlayerOverlordAbility);
                    }

                    FightTargetingArrow =
                        Object.Instantiate(_fightTargetingArrowPrefab).AddComponent<BattleBoardArrow>();
                    FightTargetingArrow.BoardCards = _gameplayManager.CurrentPlayer == OwnerPlayer ?
                        _gameplayManager.OpponentPlayer.CardsOnBoard :
                        _gameplayManager.CurrentPlayer.CardsOnBoard;
                    FightTargetingArrow.TargetsType = Skill.SkillTargets;
                    FightTargetingArrow.ElementType = Skill.TargetFactions;
                    FightTargetingArrow._targetUnitSpecialStatusType = Skill.TargetUnitSpecialStatus;
                    FightTargetingArrow.BlockedUnitStatusTypes = BlockedUnitStatusTypes;
                    FightTargetingArrow.IgnoreHeavy = true;

                    FightTargetingArrow.Begin(SelfObject.transform.position);
                }
            }

            IsUsing = true;
        }

        public GameplayActionQueueAction EndDoSkill(List<ParametrizedAbilityBoardObject> targets, bool isLocal = false)
        {
            if (!IsSkillCanUsed() || !IsUsing)
            {                
                CancelTargetingArrows();
                return null;
            }
            
            IsLocal = isLocal;

            return _gameplayManager
                .GetController<ActionsQueueController>()
                .EnqueueAction(
                    completeCallback =>
                    {
                        _battlegroundController.IsOnShorterTime = false;
                        DoOnUpSkillAction(completeCallback, targets);
                        IsUsing = false;
                        CancelTargetingArrows();
                    },
                    Enumerators.QueueActionType.OverlordSkillUsage, startupTime:0f);
        }

        public void UseSkill()
        {
            SetHighlightingEnabled(false);
            _cooldown = Skill.InitialCooldown;
            _usedInThisTurn = true;
            _coolDownTimer.SetAngle(_cooldown, true);
            _isAlreadyUsed = true;



            if (OwnerPlayer.IsLocalPlayer)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordAbilityUsed);
            }

            _overlordExperienceManager.ReportExperienceAction(
                Enumerators.ExperienceActionType.UseOverlordAbility,
                OwnerPlayer.IsLocalPlayer ? _overlordExperienceManager.PlayerMatchMatchExperienceInfo : _overlordExperienceManager.OpponentMatchMatchExperienceInfo
            );

            if (_gameplayManager.UseInifiniteAbility)
            {
                _usedInThisTurn = false;
                SetCoolDown(0);
            }

            if(Skill.SingleUse)
            {
                _coolDownTimer.Close();
            }
        }

        public void SkillUsedAction(List<ParametrizedAbilityBoardObject> targets)
        {
            SkillUsed?.Invoke(this, targets);
        }

        public void Hide()
        {
            SelfObject.SetActive(false);
        }

        public void Update()
        {
            if (!_gameplayManager.IsGameplayReady())
                return;
            {
                _pointerEventSolver.Update();

                if (Input.GetMouseButtonDown(0))
                {
                    if (_currentOverlordAbilityInfoObject != null)
                    {
                        GameClient.Get<ICameraManager>().FadeOut(level: 1);

                        _currentOverlordAbilityInfoObject.Dispose();
                        _currentOverlordAbilityInfoObject = null;
                    }
                }
            }
        }

        public void OnMouseDownEventHandler()
        {
            if (_boardArrowController.IsBoardArrowNowInTheBattle ||
                !_gameplayManager.CanDoDragActions ||
                _gameplayManager.IsGameplayInputBlocked ||
                _battlegroundController.TurnWaitingForEnd)
                return;

            if (!_gameplayManager.IsGameplayReady())
                return;

            _pointerEventSolver.PushPointer();
        }

        public void OnMouseUpEventHandler()
        {
            if (!_gameplayManager.IsGameplayReady())
                return;

            _pointerEventSolver.PopPointer();
        }

        private void PointerSolverDragStartedHandler()
        {
            if (Skill.CanSelectTarget)
            {
                if (OwnerPlayer.IsLocalPlayer)
                {
                    StartDoSkill();
                }
            }
            else
            {
                DrawAbilityTooltip();
            }
        }

        private void PointerEventSolverClickedHandler()
        {
            if (Skill.CanSelectTarget)
            {
                DrawAbilityTooltip();
            }
            else
            {
                if ((IsSkillReady && !_usedInThisTurn) && OwnerPlayer.IsLocalPlayer)
                {
                    StartDoSkill();
                }
                else
                {
                    DrawAbilityTooltip();
                }
            }
        }

        private void PointerEventSolverEndedHandler()
        {
            if (OwnerPlayer.IsLocalPlayer)
            {
                EndDoSkill(null, true);
            }
        }

        private void TurnStartedHandler()
        {
            if (_gameplayManager.CurrentTurnPlayer != OwnerPlayer)
                return;

            if (OwnerPlayer.IsStunned)
            {
                BlockSkill();
            }
            else
            {
                if (IsSkillReady)
                {
                    SetHighlightingEnabled(true);
                }
            }

            if (!Skill.SingleUse || !_isAlreadyUsed)
            {
                _coolDownTimer.SetAngle(_cooldown);
            }
        }

        private void TurnEndedHandler()
        {
            if (_gameplayManager.CurrentTurnPlayer != OwnerPlayer)
                return;

            SetHighlightingEnabled(false);
            if (Constants.DevModeEnabled)
            {
                _cooldown = 0;
            }
            if (!_usedInThisTurn)
            {
                _cooldown = Mathf.Clamp(_cooldown - 1, 0, Skill.InitialCooldown);
            }

            _usedInThisTurn = false;

            // rewrite
            CancelTargetingArrows();
        }

        private void SetHighlightingEnabled(bool isActive)
        {
            _glowObject.SetActive(isActive);

            if (_isOpen != isActive)
            {
                _isOpen = isActive;
                _shutterAnimator.SetTrigger((isActive ? Enumerators.ShutterState.Open : Enumerators.ShutterState.Close).ToString());
            }
        }

        private void DoOnUpSkillAction(Action completeCallback, List<ParametrizedAbilityBoardObject> targets)
        {
            if(OwnerPlayer.IsLocalPlayer && _tutorialManager.IsTutorial)
            {
                _tutorialManager.ActivateSelectHandPointer(Enumerators.TutorialObjectOwner.PlayerOverlordAbility);
            }

            if (!Skill.CanSelectTarget)
            {
                if(targets == null || targets.Count == 0)
                {
                    targets = new List<ParametrizedAbilityBoardObject>()
                    {
                        new ParametrizedAbilityBoardObject(OwnerPlayer)
                    };
                }

                _skillsController.DoSkillAction(this, completeCallback, targets);
            }
            else
            {
                if (OwnerPlayer.IsLocalPlayer)
                {
                    if (FightTargetingArrow != null)
                    {
                        _playerController.IsCardSelected = false;

                        if (_tutorialManager.IsTutorial)
                        {
                            if (FightTargetingArrow.SelectedPlayer != null)
                            {
                                if (!_tutorialManager.GetCurrentTurnInfo().UseOverlordSkillsSequence.Exists(info =>
                                    (info.Target == Enumerators.SkillTarget.PLAYER && FightTargetingArrow.SelectedPlayer.IsLocalPlayer) ||
                                    (info.Target == Enumerators.SkillTarget.OPPONENT && !FightTargetingArrow.SelectedPlayer.IsLocalPlayer)))
                                {
                                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordTriedToUseUnsequentionalBattleframe);
                                    CancelTargetingArrows();
                                    completeCallback?.Invoke();
                                    return;
                                }
                            }
                            else if (FightTargetingArrow.SelectedCard != null)
                            {
                                if (!_tutorialManager.GetCurrentTurnInfo().UseOverlordSkillsSequence.Exists(info => info.TargetTutorialObjectId == FightTargetingArrow.SelectedCard.Model.TutorialObjectId &&
                                    (info.Target == Enumerators.SkillTarget.OPPONENT_CARD || info.Target == Enumerators.SkillTarget.PLAYER_CARD)))
                                {
                                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordTriedToUseUnsequentionalBattleframe);
                                    CancelTargetingArrows();
                                    completeCallback?.Invoke();
                                    return;
                                }
                            }
                        }

                        _skillsController.DoSkillAction(this, completeCallback, targets);
                    }
                    else
                    {
                        completeCallback?.Invoke();
                    }
                }
                else
                {
                    _skillsController.DoSkillAction(this, completeCallback, targets);
                }
            }
        }

        private bool IsSkillCanUsed()
        {
            if (!IsSkillReady || _gameplayManager.CurrentTurnPlayer != OwnerPlayer || _usedInThisTurn || (OwnerPlayer.IsLocalPlayer && _actionsQueueController.RootQueue.GetChildCount() > 0) ||
                (_tutorialManager.IsTutorial && !_tutorialManager.GetCurrentTurnInfo().RequiredActivitiesToDoneDuringTurn.Contains(Enumerators.TutorialActivityAction.PlayerOverlordAbilityUsed)))
            {
                return false;
            }

            return true;
        }

        private void DrawAbilityTooltip()
        {
            if (_gameplayManager.IsTutorial)
                return;

            if (_currentOverlordAbilityInfoObject != null)
                return;

            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            Vector3 position;

            if (OwnerPlayer.IsLocalPlayer)
            {
                if (IsPrimary)
                {
                    position = new Vector3(4f, 0.5f, 0);
                }
                else
                {
                    position = new Vector3(-4f, 0.5f, 0);
                }
            }
            else
            {
                if (IsPrimary)
                {
                    position = new Vector3(4f, -1.15f, 0);
                }
                else
                {
                    position = new Vector3(-4f, -1.15f, 0);
                }
            }

            _currentOverlordAbilityInfoObject = new OverlordAbilityInfoObject(Skill, SelfObject.transform, position);
        }

        public class OverlordAbilityInfoObject
        {
            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly GameObject _selfObject;

            private readonly SpriteRenderer _buffIconPicture;

            private readonly TextMeshPro _triggerText;

            private readonly TextMeshPro _descriptionText;

            public OverlordAbilityInfoObject(OverlordSkillPrototype skill, Transform parent, Vector3 position)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                _selfObject =
                    Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>(
                            "Prefabs/Gameplay/Tooltips/Tooltip_OverlordAbilityInfo"), parent, false);

                Transform.localPosition = position;

                _triggerText = _selfObject.transform.Find("Text_Title").GetComponent<TextMeshPro>();
                _descriptionText = _selfObject.transform.Find("Text_Description").GetComponent<TextMeshPro>();

                _buffIconPicture = _selfObject.transform.Find("Image_IconBackground/Image_Icon")
                    .GetComponent<SpriteRenderer>();

                _triggerText.text = skill.Title.ToUpperInvariant();
                _descriptionText.text = "    " + skill.Description;

                _buffIconPicture.sprite =
                    _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/Icons/" +
                        skill.IconPath.Replace(" ", string.Empty));
            }

            public Transform Transform => _selfObject.transform;

            public void Dispose()
            {
                Object.Destroy(_selfObject);
            }
        }
    }
}
