// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using UnityEngine;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using TMPro;
using LoomNetwork.CZB.Gameplay;

namespace LoomNetwork.CZB
{
    public class BoardSkill
    {
        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;
        private ITutorialManager _tutorialManager;

        private PlayerController _playerController;
        private BattleController _battleController;
        private BattlegroundController _battlegroundController;
        private SkillsController _skillsController;
        private VFXController _vfxController;

        private SpriteRenderer _glowObjectSprite;
        private TMPro.TextMeshPro _cooldownText;

        private GameObject fightTargetingArrowPrefab;

        private int _cooldown;
        private int _initialCooldown;

        private bool _usedInThisTurn = false;

        private OnBehaviourHandler _behaviourHandler;

        private OverlordAbilityInfoObject _currentOverlordAbilityInfoObject;

        private PointerEventSolver _pointerEventSolver;


        public BoardArrow abilitiesTargetingArrow;
        public BattleBoardArrow fightTargetingArrow;

        public GameObject selfObject;


        public Player owner;
        public HeroSkill skill;

        public bool IsSkillReady { get { return _cooldown == 0; } }

        public bool IsUsing { get; private set; }

        public bool IsPrimary { get; private set; }

        public BoardSkill(GameObject obj, Player player, HeroSkill skillInfo, int cooldown, bool isPrimary)
        {
            selfObject = obj;
            skill = skillInfo;
            owner = player;
            IsPrimary = isPrimary;

            _initialCooldown = cooldown;
            _cooldown = cooldown;
      

            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _playerController = _gameplayManager.GetController<PlayerController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _vfxController = _gameplayManager.GetController<VFXController>();

            _glowObjectSprite = selfObject.transform.Find("Glow").GetComponent<SpriteRenderer>();
            _glowObjectSprite.gameObject.SetActive(false);

            _cooldownText = selfObject.transform.Find("SpellCost/SpellCostText").GetComponent<TMPro.TextMeshPro>();

            owner.OnStartTurnEvent += OnStartTurnEventHandler;
            owner.OnEndTurnEvent += OnEndTurnEventHandler;

            _behaviourHandler = this.selfObject.GetComponent<OnBehaviourHandler>();

            //_behaviourHandler.OnTriggerEnter2DEvent += OnTriggerEnter2D;
            //   _behaviourHandler.OnTriggerExit2DEvent += OnTriggerExit2D;

            if (owner.IsLocalPlayer)
            {
                _pointerEventSolver = new PointerEventSolver();
                _pointerEventSolver.OnDragStartedEvent += PointerEventSolver_OnDragStartedEventHandler;
                _pointerEventSolver.OnClickEvent += PointerEventSolver_OnClickEventHandler;
                _pointerEventSolver.OnEndEvent += PointerEventSolver_OnEndEventHandler;
            }

            _cooldownText.text = _cooldown.ToString();

            fightTargetingArrowPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");
        }

        private void PointerEventSolver_OnDragStartedEventHandler()
        {
            StartDoSkill();
        }

        private void PointerEventSolver_OnClickEventHandler()
        {
            DrawAbilityTooltip();
        }

        private void PointerEventSolver_OnEndEventHandler()
        {
            EndDoSkill();
        }

        private void OnStartTurnEventHandler()
        {
            if (!_gameplayManager.CurrentTurnPlayer.Equals(owner))
                return;

            if (owner.IsStunned)
            {
                BlockSkill();
            }
            else
            {
                if (IsSkillReady)
                    SetHighlightingEnabled(true);
            }

            _cooldownText.text = _cooldown.ToString();
        }

        private void OnEndTurnEventHandler()
        {
            if (!_gameplayManager.CurrentTurnPlayer.Equals(owner))
                return;

            SetHighlightingEnabled(false);
#if UNITY_EDITOR
            if (Constants.DEV_MODE)
                _cooldown = 0;
            else
            {
#endif
                if(!_usedInThisTurn)
                    _cooldown = Mathf.Clamp(_cooldown - 1, 0, _initialCooldown);

#if UNITY_EDITOR
            }
#endif

            _usedInThisTurn = false;

            //rewrite
            CancelTargetingArrows();
        }

        private void SetHighlightingEnabled(bool isActive)
        {
            _glowObjectSprite.gameObject.SetActive(isActive);
        }

        public void CancelTargetingArrows()
        {
            if (fightTargetingArrow != null)
            {
                MonoBehaviour.Destroy(fightTargetingArrow.gameObject);
            }
        }

        public void BlockSkill()
        {
            _usedInThisTurn = true;
            SetHighlightingEnabled(false);
        }

        //public void OnTriggerEnter2D(Collider2D collider)
        //{
        //    if (collider.transform.parent != null)
        //    {
        //        var targetingArrow = collider.transform.parent.GetComponent<BoardArrow>();
        //        if (targetingArrow != null)
        //        {
        //            targetingArrow.OnCardSelected(null);
        //        }
        //    }
        //}

        //public void OnTriggerExit2D(Collider2D collider)
        //{
        //    if (collider.transform.parent != null)
        //    {
        //        var targetingArrow = collider.transform.parent.GetComponent<BoardArrow>();
        //        if (targetingArrow != null)
        //        {
        //            targetingArrow.OnCardUnselected(null);
        //        }
        //    }
        //}

        public void StartDoSkill()
        {
            if (!IsSkillCanUsed())
                return;

            if (owner.IsLocalPlayer)
            {
                if (skill.overlordSkill != Enumerators.OverlordSkill.DRAW &&
                    skill.overlordSkill != Enumerators.OverlordSkill.HARDEN &&
                    skill.overlordSkill != Enumerators.OverlordSkill.MEND)
                {
                    fightTargetingArrow = MonoBehaviour.Instantiate(fightTargetingArrowPrefab).AddComponent<BattleBoardArrow>();
                    fightTargetingArrow.BoardCards = _gameplayManager.CurrentPlayer == owner ? _gameplayManager.OpponentPlayer.BoardCards : _gameplayManager.CurrentPlayer.BoardCards;
                    fightTargetingArrow.targetsType = new System.Collections.Generic.List<Enumerators.SkillTargetType>();
                    switch (skill.overlordSkill)
                    {
                        case Enumerators.OverlordSkill.PUSH:
                        case Enumerators.OverlordSkill.STONE_SKIN:
                        case Enumerators.OverlordSkill.HARDEN:
                        case Enumerators.OverlordSkill.RABIES:
                        case Enumerators.OverlordSkill.TOXIC_POWER:
                            fightTargetingArrow.targetsType.Add(Enumerators.SkillTargetType.OPPONENT_CARD);
                            fightTargetingArrow.targetsType.Add(Enumerators.SkillTargetType.PLAYER_CARD);

                            break;
                        default:
                            fightTargetingArrow.targetsType.Add(Enumerators.SkillTargetType.OPPONENT_CARD);
                            fightTargetingArrow.targetsType.Add(Enumerators.SkillTargetType.PLAYER_CARD);
                            fightTargetingArrow.targetsType.Add(Enumerators.SkillTargetType.PLAYER);
                            fightTargetingArrow.targetsType.Add(Enumerators.SkillTargetType.OPPONENT);
                            break;
                    }

                    //if (owner.SelfHero.heroElement == Enumerators.SetType.AIR)
                        fightTargetingArrow.ignoreHeavy = true;

                        fightTargetingArrow.Begin(selfObject.transform.position);

                    if (_tutorialManager.IsTutorial)
                        _tutorialManager.DeactivateSelectTarget();
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

        private void DoOnUpSkillAction()
        {
            if (owner.IsLocalPlayer && _tutorialManager.IsTutorial)
                _tutorialManager.ActivateSelectTarget();

            if (skill.overlordSkill == Enumerators.OverlordSkill.DRAW ||
                skill.overlordSkill == Enumerators.OverlordSkill.HARDEN ||
                skill.overlordSkill == Enumerators.OverlordSkill.MEND)
                _skillsController.DoSkillAction(this, owner);
            else
            {
                if (owner.IsLocalPlayer)
                {
                    if (fightTargetingArrow != null)
                    {
                        _skillsController.DoSkillAction(this);
                        _playerController.IsCardSelected = false;
                    }
                }
                else
                    _skillsController.DoSkillAction(this);
            }
        }


        public void UseSkill(object target)
        {
            SetHighlightingEnabled(false);
            _cooldown = _initialCooldown;
            _usedInThisTurn = true;
            _cooldownText.text = _cooldown.ToString();
        }

        public void Hide()
        {
            selfObject.SetActive(false);
        }

        private bool IsSkillCanUsed()
        {
            if (_tutorialManager.IsTutorial && _tutorialManager.CurrentStep == 32)
                return true;

            if (!IsSkillReady || !_gameplayManager.CurrentTurnPlayer.Equals(owner) || _usedInThisTurn)
                return false;

            return true;
        }


        public void Update()
        {
            if (owner.IsLocalPlayer)
            {
                _pointerEventSolver.Update();

                if(Input.GetMouseButtonDown(0))
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
            _pointerEventSolver.PushPointer();
        }

        public void OnMouseUpEventHandler()
        {
            _pointerEventSolver.PopPointer(); 
        }

        private void DrawAbilityTooltip()
        {
            if (_currentOverlordAbilityInfoObject != null)
                return;

            GameClient.Get<ICameraManager>().FadeIn(0.65f, 1);

            Vector3 position = Vector3.zero;

            if (IsPrimary) position = new Vector3(4f, 0.5f, 0);
            else position = new Vector3(-4f, 0.5f, 0);

            _currentOverlordAbilityInfoObject = new OverlordAbilityInfoObject(skill, selfObject.transform, position);
        }


        public class OverlordAbilityInfoObject
        {
            private ILoadObjectsManager _loadObjectsManager;

            private GameObject _selfObject;

            private SpriteRenderer _buffIconPicture;

            private TextMeshPro _callTypeText,
                                _descriptionText;

            public Transform transform { get { return _selfObject.transform; } }

            public OverlordAbilityInfoObject(HeroSkill skill, Transform parent, Vector3 position)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                _selfObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Tooltips/Tooltip_OverlordAbilityInfo"), parent, false);

                transform.localPosition = position;

                _callTypeText = _selfObject.transform.Find("Text_Title").GetComponent<TextMeshPro>();
                _descriptionText = _selfObject.transform.Find("Text_Description").GetComponent<TextMeshPro>();

                _buffIconPicture = _selfObject.transform.Find("Image_IconBackground/Image_Icon").GetComponent<SpriteRenderer>();

                _callTypeText.text = skill.title.ToUpper();
                _descriptionText.text = "    " + skill.description;

                _buffIconPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/Icons/" + skill.iconPath);
            }

            public void Dispose()
            {
                MonoBehaviour.Destroy(_selfObject);
            }
        }
    }
}