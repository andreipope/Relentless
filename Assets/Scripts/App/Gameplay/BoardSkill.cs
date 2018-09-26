using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class BoardSkill
    {
        public BattleBoardArrow FightTargetingArrow;

        public GameObject SelfObject;

        public Player Owner;

        public HeroSkill Skill;

        private readonly ILoadObjectsManager _loadObjectsManager;

        private readonly IGameplayManager _gameplayManager;

        private readonly ITutorialManager _tutorialManager;

        private readonly PlayerController _playerController;

        private readonly SkillsController _skillsController;

        private readonly BoardArrowController _boardArrowController;

        private readonly GameObject _glowObject;

        private readonly TextMeshPro _cooldownText;

        private readonly GameObject _fightTargetingArrowPrefab;

        private readonly int _initialCooldown;

        private readonly Animator _shutterAnimator;

        private readonly PointerEventSolver _pointerEventSolver;

        private int _cooldown;

        private bool _usedInThisTurn;

        private OnBehaviourHandler _behaviourHandler;

        private OverlordAbilityInfoObject _currentOverlordAbilityInfoObject;

        private SkillCoolDownTimer _coolDownTimer;

        public BoardSkill(GameObject obj, Player player, HeroSkill skillInfo, bool isPrimary)
        {
            SelfObject = obj;
            Skill = skillInfo;
            Owner = player;
            IsPrimary = isPrimary;

            _initialCooldown = skillInfo.InitialCooldown;
            _cooldown = skillInfo.Cooldown;

            _coolDownTimer = new SkillCoolDownTimer(SelfObject, _cooldown);

            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _playerController = _gameplayManager.GetController<PlayerController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();

            _glowObject = SelfObject.transform.Find("OverlordAbilitySelection").gameObject;
            _glowObject.SetActive(false);

            _cooldownText = SelfObject.transform.Find("SpellCost/SpellCostText").GetComponent<TextMeshPro>();

            string name = isPrimary ? "1stShutters" : "2ndtShutters";
            _shutterAnimator = SelfObject.transform.parent.transform
                .Find("OverlordArea/RegularModel/CZB_3D_Overlord_death_regular_LOD0/" + name).GetComponent<Animator>();
            _shutterAnimator.enabled = false;
            _shutterAnimator.StopPlayback();

            Owner.TurnStarted += TurnStartedHandler;
            Owner.TurnEnded += TurnEndedHandler;

            _behaviourHandler = SelfObject.GetComponent<OnBehaviourHandler>();
            {
                _pointerEventSolver = new PointerEventSolver();
                _pointerEventSolver.DragStarted += PointerSolverDragStartedHandler;
                _pointerEventSolver.Clicked += PointerEventSolverClickedHandler;
                _pointerEventSolver.Ended += PointerEventSolverEndedHandler;
            }

            _cooldownText.text = _cooldown.ToString();
            _coolDownTimer.SetAngle(_cooldown);

            _fightTargetingArrowPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");
        }

        public bool IsSkillReady => _cooldown == 0;

        public bool IsUsing { get; private set; }

        public bool IsPrimary { get; }

        public void CancelTargetingArrows()
        {
            if (FightTargetingArrow != null)
            {
                FightTargetingArrow.Dispose();
            }
        }

        public void BlockSkill()
        {
            _usedInThisTurn = true;
            SetHighlightingEnabled(false);
        }

        public void StartDoSkill()
        {
            if (!IsSkillCanUsed())
                return;

            if (Owner.IsLocalPlayer)
            {
                if (Skill.SkillTargetTypes.Count > 0)
                {
                    FightTargetingArrow =
                        Object.Instantiate(_fightTargetingArrowPrefab).AddComponent<BattleBoardArrow>();
                    FightTargetingArrow.BoardCards = _gameplayManager.CurrentPlayer == Owner ?
                        _gameplayManager.OpponentPlayer.BoardCards :
                        _gameplayManager.CurrentPlayer.BoardCards;
                    FightTargetingArrow.TargetsType = Skill.SkillTargetTypes;
                    FightTargetingArrow.ElementType = Skill.ElementTargetTypes;

                    FightTargetingArrow.IgnoreHeavy = true;

                    FightTargetingArrow.Begin(SelfObject.transform.position);

                    if (_tutorialManager.IsTutorial)
                    {
                        _tutorialManager.DeactivateSelectTarget();
                    }
                }
            }

            IsUsing = true;
        }

        public void EndDoSkill()
        {
            if (!IsSkillCanUsed() || !IsUsing)
                return;

            DoOnUpSkillAction();

            IsUsing = false;
        }

        private void UseSkill()
        {
            SetHighlightingEnabled(false);
            _cooldown = _initialCooldown;
            _usedInThisTurn = true;
            _cooldownText.text = _cooldown.ToString();
            _coolDownTimer.SetAngle(_cooldown, true);

            GameClient.Get<IOverlordManager>().ReportXPAction(Owner.SelfHero, Common.Enumerators.XPActionType.UseOverlordAbility);
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
            if (_boardArrowController.IsBoardArrowNowInTheBattle || !_gameplayManager.CanDoDragActions || _gameplayManager.IsGameplayInputBlocked)
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
            if (Skill.SkillTargetTypes.Count > 0)
            {
                if (Owner.IsLocalPlayer)
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
            if (Skill.SkillTargetTypes.Count > 0)
            {
                DrawAbilityTooltip();
            }
            else
            {
                if ((IsSkillReady && !_usedInThisTurn) && Owner.IsLocalPlayer)
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
            if (Owner.IsLocalPlayer)
            {
                EndDoSkill();
            }
        }

        private void TurnStartedHandler()
        {
            if (!_gameplayManager.CurrentTurnPlayer.Equals(Owner))
                return;

            if (Owner.IsStunned)
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

            _cooldownText.text = _cooldown.ToString();
            _coolDownTimer.SetAngle(_cooldown);
        }

        private void TurnEndedHandler()
        {
            if (!_gameplayManager.CurrentTurnPlayer.Equals(Owner))
                return;

            SetHighlightingEnabled(false);
#if DEV_MODE
            _cooldown = 0;
#endif
            if (!_usedInThisTurn)
            {
                _cooldown = Mathf.Clamp(_cooldown - 1, 0, _initialCooldown);
            }

            _usedInThisTurn = false;

            // rewrite
            CancelTargetingArrows();
        }

        private void SetHighlightingEnabled(bool isActive)
        {
            _glowObject.SetActive(isActive);

            _shutterAnimator.enabled = isActive ? true : false;
            _shutterAnimator.speed = isActive ? 1 : -1;
            _shutterAnimator.StartPlayback();
        }

        private void DoOnUpSkillAction()
        {
            if (Owner.IsLocalPlayer && _tutorialManager.IsTutorial)
            {
                _tutorialManager.ActivateSelectTarget();
            }

            if (Skill.SkillTargetTypes.Count == 0)
            {
                UseSkill();
                _skillsController.DoSkillAction(this, Owner);
            }
            else
            {
                if (Owner.IsLocalPlayer)
                {
                    if (FightTargetingArrow != null)
                    {
                        UseSkill();
                        _skillsController.DoSkillAction(this);
                        _playerController.IsCardSelected = false;
                    }
                }
                else
                {
                    UseSkill();
                    _skillsController.DoSkillAction(this);
                }
            }
        }

        private bool IsSkillCanUsed()
        {
            if (_tutorialManager.IsTutorial && _tutorialManager.CurrentTutorialDataStep.CanUseBoardSkill)
            {
                return true;
            }

            if (!IsSkillReady || _gameplayManager.CurrentTurnPlayer != Owner || _usedInThisTurn ||
                _tutorialManager.IsTutorial)
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

            if (Owner.IsLocalPlayer)
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

            private readonly TextMeshPro _callTypeText;

            private readonly TextMeshPro _descriptionText;

            public OverlordAbilityInfoObject(HeroSkill skill, Transform parent, Vector3 position)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                _selfObject =
                    Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>(
                            "Prefabs/Gameplay/Tooltips/Tooltip_OverlordAbilityInfo"), parent, false);

                Transform.localPosition = position;

                _callTypeText = _selfObject.transform.Find("Text_Title").GetComponent<TextMeshPro>();
                _descriptionText = _selfObject.transform.Find("Text_Description").GetComponent<TextMeshPro>();

                _buffIconPicture = _selfObject.transform.Find("Image_IconBackground/Image_Icon")
                    .GetComponent<SpriteRenderer>();

                _callTypeText.text = skill.Title.ToUpper();
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
