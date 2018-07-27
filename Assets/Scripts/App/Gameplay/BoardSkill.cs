// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using UnityEngine;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

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

            _cooldownText.text = _cooldown.ToString();

            fightTargetingArrowPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");
        }

        private void OnStartTurnEventHandler()
        {
            if (!_gameplayManager.CurrentTurnPlayer.Equals(owner))
                return;

            if (IsSkillReady)
                SetHighlightingEnabled(true);

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
                if (owner.SelfHero.heroElement != Enumerators.SetType.EARTH)
                {
                    fightTargetingArrow = MonoBehaviour.Instantiate(fightTargetingArrowPrefab).AddComponent<BattleBoardArrow>();
                    fightTargetingArrow.BoardCards = _gameplayManager.CurrentPlayer == owner ? _gameplayManager.OpponentPlayer.BoardCards : _gameplayManager.CurrentPlayer.BoardCards;
                    fightTargetingArrow.targetsType = new System.Collections.Generic.List<Enumerators.SkillTargetType>()
                {
                    Enumerators.SkillTargetType.PLAYER,
                    Enumerators.SkillTargetType.OPPONENT,
                    Enumerators.SkillTargetType.OPPONENT_CARD,
                    Enumerators.SkillTargetType.PLAYER_CARD
                };
                    //skill.skillTargetType;

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
            if (!IsSkillCanUsed())
                return;

            DoOnUpSkillAction();

            IsUsing = false;
        }

        private void DoOnUpSkillAction()
        {
            if (owner.IsLocalPlayer && _tutorialManager.IsTutorial)
                _tutorialManager.ActivateSelectTarget();

            if (owner.SelfHero.heroElement == Enumerators.SetType.EARTH)
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
            if (_tutorialManager.IsTutorial && _tutorialManager.CurrentStep == 29)
                return true;

            if (!IsSkillReady || !_gameplayManager.CurrentTurnPlayer.Equals(owner))
                return false;

            return true;
        }
    }
}