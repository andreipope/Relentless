// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;

using UnityEngine;
using UnityEngine.Rendering;

using DG.Tweening;
using TMPro;
using LoomNetwork.CZB.Common;
using System.Collections.Generic;
using LoomNetwork.CZB.Helpers;
using LoomNetwork.Internal;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class BoardUnit
    {
        public event Action UnitOnDieEvent;
        public event Action<object, int, bool> UnitOnAttackEvent;
        public event Action<object> UnitGotDamageEvent;

        public event Action UnitHPChangedEvent;
        public event Action UnitDamageChangedEvent;

        private Action damageChangedDelegate;
        private Action healthChangedDelegate;

        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;
        private ISoundManager _soundManager;
        private ITimerManager _timerManager;
        private ITutorialManager _tutorialManager;
        private PlayerController _playerController;
        private BattlegroundController _battlegroundController;
        private AnimationsController _animationsController;
        private BattleController _battleController;
        private ActionsQueueController _actionsQueueController;
        private VFXController _vfxController;
        private RanksController _ranksController;
        private AbilitiesController _abilitiesController;

        private GameObject _fightTargetingArrowPrefab;

        private GameObject _selfObject;

        private SpriteRenderer _pictureSprite;
        private SpriteRenderer _frozenSprite;
        private SpriteRenderer _glowSprite;
        private SpriteRenderer _frameSprite;
        private SpriteRenderer _animationSprite;
        private GameObject _shieldSprite;

        private GameObject _glowSelectedObjectSprite;

        private TextMeshPro _attackText;
        private TextMeshPro _healthText;

        private ParticleSystem _sleepingParticles;

        private GameObject _feralFrame,
                           _heavyFrame;

        private Vector3 _initialScale = new Vector3(0.9f, 0.9f, 0.9f);

        private Enumerators.CardType _initialUnitType;

        private int _currentDamage;
        private int _currentHealth;

        private int _stunTurns = 0;

        private bool _readyForBuffs = false;

        private bool _ignoreArrivalEndEvents = false;

        private BoardArrow abilitiesTargetingArrow;
        private BattleBoardArrow fightTargetingArrow;

        private AnimationEventTriggering arrivalAnimationEventHandler;

        private OnBehaviourHandler _onBehaviourHandler;

        private GameObject unitContentObject;

        private Animator unitAnimator;

        private List<Enumerators.BuffType> _buffsOnUnit;

        private bool _dead = false;

        private bool _arrivalDone = false;

        public bool AttackedThisTurn = false;

        public bool hasFeral;
        public bool hasHeavy;
        public int numTurnsOnBoard;

        public int initialDamage;
        public int initialHP;

        public Player ownerPlayer;

        public List<UnitAnimatorInfo> animatorControllers;

        public List<object> attackedBoardObjectsThisTurn;

        public Enumerators.AttackInfoType attackInfoType = Enumerators.AttackInfoType.ANY;


        public int MaxCurrentDamage { get { return initialDamage + BuffedDamage; } }
        public int BuffedDamage { get; set; }


        public int CurrentDamage
        {
            get
            {
                return _currentDamage;
            }
            set
            {
                var oldDamage = _currentDamage;

                _currentDamage = Mathf.Clamp(value, 0, 99999);
               // if (oldDamage != _currentDamage)
                    UnitDamageChangedEvent?.Invoke();
            }
        }

        public int MaxCurrentHP { get { return initialHP + BuffedHP; } }
        public int BuffedHP { get; set; }

        public int CurrentHP
        {
            get
            {
                return _currentHealth;
            }
            set
            {
                var oldHealth = _currentHealth;

                _currentHealth = Mathf.Clamp(value, 0, 99);
             //   if (oldHealth != _currentHealth)
                    UnitHPChangedEvent?.Invoke();
            }
        }

        public Transform transform { get { return _selfObject.transform; } }
        public GameObject gameObject { get { return _selfObject; } }
        public Sprite sprite { get { return _pictureSprite.sprite; } }

        public bool IsPlayable { get; set; }

        public WorkingCard Card { get; private set; }

        public int InstanceId { get; private set; }

        public bool IsStun
        {
            get { return (_stunTurns > 0 ? true : false); }
        }

        public bool IsCreatedThisTurn { get; private set; }

        public List<Enumerators.BuffType> BuffsOnUnit { get { return _buffsOnUnit; } }

        public bool HasBuffRush { get; set; }
        public bool HasBuffHeavy { get; set; }
        public bool HasBuffShield { get; set; }
        public bool TakeFreezeToAttacked { get; set; }
        public int AdditionalDamage { get; set; }
        public int AdditionalAttack { get; set; }
        public int AdditionalDefense { get; set; }

        public int DamageDebuffUntillEndOfTurn { get; private set; }
        public int HPDebuffUntillEndOfTurn { get; private set; }

        public bool IsAttacking { get; private set; }

        public bool IsAllAbilitiesResolvedAtStart { get; set; }

        public bool IsReanimated { get; set; }
        public bool AttackAsFirst { get; set; }

        public Enumerators.UnitStatusType UnitStatus { get; set; }

        public BoardUnit(Transform parent)
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _timerManager = GameClient.Get<ITimerManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _playerController = _gameplayManager.GetController<PlayerController>();
            _animationsController = _gameplayManager.GetController<AnimationsController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _vfxController = _gameplayManager.GetController<VFXController>();
            _ranksController = _gameplayManager.GetController<RanksController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();

            _selfObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/BoardCreature"));
            _selfObject.transform.SetParent(parent, false);

            _fightTargetingArrowPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");

            _pictureSprite = _selfObject.transform.Find("GraphicsAnimation/PictureRoot/CreaturePicture").GetComponent<SpriteRenderer>();
            _frozenSprite = _selfObject.transform.Find("Other/Frozen").GetComponent<SpriteRenderer>();
            _glowSprite = _selfObject.transform.Find("Other/Glow").GetComponent<SpriteRenderer>();
            _frameSprite = _selfObject.transform.Find("GraphicsAnimation").GetComponent<SpriteRenderer>();
            _animationSprite = _selfObject.transform.Find("GraphicsAnimation").GetComponent<SpriteRenderer>();
            _shieldSprite = _selfObject.transform.Find("Other/Shield").gameObject;

            _glowSelectedObjectSprite = _selfObject.transform.Find("Other/GlowSelectedObject").gameObject;

            _feralFrame = _selfObject.transform.Find("Other/object_feral_frame").gameObject;
            _heavyFrame = _selfObject.transform.Find("Other/object_heavy_frame").gameObject;

            _attackText = _selfObject.transform.Find("Other/AttackAndDefence/AttackText").GetComponent<TextMeshPro>();
            _healthText = _selfObject.transform.Find("Other/AttackAndDefence/DefenceText").GetComponent<TextMeshPro>();

            _sleepingParticles = _selfObject.transform.Find("Other/SleepingParticles").GetComponent<ParticleSystem>();

            unitAnimator = _selfObject.transform.Find("GraphicsAnimation").GetComponent<Animator>();

            unitContentObject = _selfObject.transform.Find("Other").gameObject;
            unitContentObject.SetActive(false);

            arrivalAnimationEventHandler = _selfObject.transform.Find("GraphicsAnimation").GetComponent<AnimationEventTriggering>();

            _onBehaviourHandler = _selfObject.GetComponent<OnBehaviourHandler>();

            arrivalAnimationEventHandler.OnAnimationEvent += ArrivalAnimationEventHandler;

            _onBehaviourHandler.OnMouseUpEvent += OnMouseUp;
            _onBehaviourHandler.OnMouseDownEvent += OnMouseDown;
            _onBehaviourHandler.OnTriggerEnter2DEvent += OnTriggerEnter2D;
            _onBehaviourHandler.OnTriggerExit2DEvent += OnTriggerExit2D;

            animatorControllers = new List<UnitAnimatorInfo>();
            for (int i = 0; i < Enum.GetNames(typeof(Enumerators.CardType)).Length; i++)
            {
                animatorControllers.Add(new UnitAnimatorInfo()
                {
                    animator = _loadObjectsManager.GetObjectByPath<RuntimeAnimatorController>("Animators/" + ((Enumerators.CardType)i).ToString() + "ArrivalController"),
                    cardType = (Enumerators.CardType)i
                });
            }

            _buffsOnUnit = new List<Enumerators.BuffType>();
            attackedBoardObjectsThisTurn = new List<object>();

            _glowSprite.gameObject.SetActive(true);
            _glowSprite.enabled = false;

            IsCreatedThisTurn = true;

            UnitStatus = Enumerators.UnitStatusType.NONE;

            IsAllAbilitiesResolvedAtStart = true;
        }

        public bool IsHeavyUnit()
        {
            return HasBuffHeavy || hasHeavy;
        }

        public bool IsFeralUnit()
        {
            return hasFeral;
        }


        public void Update()
        {
            CheckOnDie();
        }

        public void Reset()
        {
          
        }

        public void Die(bool returnToHand = false)
        {
            _timerManager.StopTimer(CheckIsCanDie);

            UnitHPChangedEvent -= healthChangedDelegate;
            UnitDamageChangedEvent -= damageChangedDelegate;

            _dead = true;

            if (!returnToHand)
                _battlegroundController.KillBoardCard(this);
        }

        public void DebuffDamage(int value)
        {
            Debug.Log(value);

            if (value == 0)
                return;
            DamageDebuffUntillEndOfTurn = value;
            if (CurrentDamage + DamageDebuffUntillEndOfTurn < 0)
                DamageDebuffUntillEndOfTurn += CurrentDamage + DamageDebuffUntillEndOfTurn;
            CurrentDamage += DamageDebuffUntillEndOfTurn;
            Debug.Log(DamageDebuffUntillEndOfTurn);
        }

        public void DebuffHealth(int value)
        {
            if (value == 0)
                return;

            HPDebuffUntillEndOfTurn = value;
            CurrentHP += HPDebuffUntillEndOfTurn;
        }

        public void BuffUnit(Enumerators.BuffType type)
        {
            if (!_readyForBuffs)
                return;
            UnityEngine.Debug.Log(Card.libraryCard.name + " Buffed " + type);
            _buffsOnUnit.Add(type);
        }

        public void RemoveBuff(Enumerators.BuffType type)
        {
            if (!_readyForBuffs)
                return;

            _buffsOnUnit.Remove(type);
        }

        public void ClearBuffs()
        {
            if (!_readyForBuffs)
                return;

            int damageToDelete = 0;
            int attackToDelete = 0;
            int defenseToDelete = 0;

            foreach (var buff in _buffsOnUnit)
            {
                switch (buff)
                {
                    case Enumerators.BuffType.ATTACK:
                        attackToDelete++;
                        break;
                    case Enumerators.BuffType.DAMAGE:
                        damageToDelete++;
                        break;
                    case Enumerators.BuffType.DEFENCE:
                        defenseToDelete++;
                        break;
                    case Enumerators.BuffType.FREEZE:
                        TakeFreezeToAttacked = false;
                        break;
                    case Enumerators.BuffType.HEAVY:
                        HasBuffHeavy = false;
                        break;
                    case Enumerators.BuffType.RUSH:
                        if(!IsPlayable && HasBuffRush && IsCreatedThisTurn && !AttackedThisTurn)
                            _sleepingParticles.gameObject.SetActive(true);

                        HasBuffRush = false;
                        // IsPlayable = _attacked;
                        break;
                    case Enumerators.BuffType.SHIELD:
                        HasBuffShield = false;
                        break;
                    default: break;
                }
            }

            _buffsOnUnit.Clear();

            AdditionalDefense -= defenseToDelete;
            AdditionalAttack -= attackToDelete;
            AdditionalDamage -= damageToDelete;
            BuffedHP -= defenseToDelete;
            CurrentHP -= defenseToDelete;
            BuffedDamage -= attackToDelete;
            CurrentDamage -= attackToDelete;

            UpdateFrameByType();
        }

        public void ApplyBuff(Enumerators.BuffType type)
        {
            if (!_readyForBuffs)
                return;


            //foreach (var buff in _buffsOnUnit)
            //{
            switch (type)
            {
                case Enumerators.BuffType.ATTACK:
                    CurrentDamage++;
                    break;
                case Enumerators.BuffType.DAMAGE:
                    //AdditionalDamage++;
                    break;
                case Enumerators.BuffType.DEFENCE:
                    CurrentHP++;
                    break;
                case Enumerators.BuffType.FREEZE:
                    TakeFreezeToAttacked = true;
                    break;
                case Enumerators.BuffType.HEAVY:
                    HasBuffHeavy = true;
                    break;
                case Enumerators.BuffType.RUSH:
                    HasBuffRush = true;
                    // IsPlayable = !_attacked;
                    _sleepingParticles.gameObject.SetActive(false);
                    break;
                case Enumerators.BuffType.SHIELD:
                    HasBuffShield = true;
                    break;
                case Enumerators.BuffType.REANIMATE_UNIT:
                    _abilitiesController.BuffUnitByAbility(Enumerators.AbilityType.REANIMATE_UNIT, this, Card.libraryCard, ownerPlayer);
                    break;
                case Enumerators.BuffType.DESTROY_TARGET_UNIT_AFTER_ATTACK:
                    _abilitiesController.BuffUnitByAbility(Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, this, Card.libraryCard, ownerPlayer);
                    break;
                default: break;
            }
            //}

            //BuffedHP += AdditionalDefense;
            //CurrentHP += BuffedHP;
            //BuffedDamage += AdditionalAttack;
            //CurrentDamage += BuffedDamage;

            UpdateFrameByType();
        }

        public void UseShieldFromBuff()
        {
            HasBuffShield = false;
            _buffsOnUnit.Remove(Enumerators.BuffType.SHIELD);
            _shieldSprite.SetActive(HasBuffShield);
        }

        public void UpdateFrameByType()
        {
            _shieldSprite.SetActive(HasBuffShield);

            if (HasBuffHeavy)
                SetAsHeavyUnit(true);
            else
            {
                switch(_initialUnitType)
                {
                    case Enumerators.CardType.WALKER:
                        SetAsWalkerUnit();
                        break;
                    case Enumerators.CardType.FERAL:
                        SetAsFeralUnit();
                        break;
                    case Enumerators.CardType.HEAVY:
                        SetAsHeavyUnit();
                        break;
                    default: break;
                }
            }
        }

        public void SetAsHeavyUnit(bool buff = false)
        {
            if (hasHeavy || HasBuffHeavy)
                return;

            if (!buff)
            {
                hasHeavy = true;
                hasFeral = false;
                _initialUnitType = Enumerators.CardType.HEAVY;
            }

            _ignoreArrivalEndEvents = true;

            unitContentObject.SetActive(false);
            unitAnimator.runtimeAnimatorController = animatorControllers.Find(x => x.cardType == Enumerators.CardType.HEAVY).animator;
            unitAnimator.StopPlayback();
            unitAnimator.Play(0);
            unitAnimator.SetTrigger("Active");

            _readyForBuffs = true;
        }

        public void SetAsWalkerUnit(bool buff = false)
        {
            if (!hasHeavy && !hasFeral && !HasBuffHeavy)
                return;

            if (!buff)
            {
                hasHeavy = false;
                hasFeral = false;
                HasBuffHeavy = false;
                _initialUnitType = Enumerators.CardType.WALKER;
            }

            _ignoreArrivalEndEvents = true;

            unitContentObject.SetActive(false);
            unitAnimator.runtimeAnimatorController = animatorControllers.Find(x => x.cardType == Enumerators.CardType.WALKER).animator;
            unitAnimator.StopPlayback();
            unitAnimator.Play(0);
            unitAnimator.SetTrigger("Active");

            _readyForBuffs = true;
        }

        public void SetAsFeralUnit(bool buff = false)
        {
            if (hasFeral)
                return;

            if (!buff)
            {
                hasHeavy = false;
                HasBuffHeavy = false;
                hasFeral = true;
                _initialUnitType = Enumerators.CardType.FERAL;
            }

            _ignoreArrivalEndEvents = true;

            unitContentObject.SetActive(false);
            unitAnimator.runtimeAnimatorController = animatorControllers.Find(x => x.cardType == Enumerators.CardType.FERAL).animator;
            unitAnimator.StopPlayback();
            unitAnimator.Play(0);
            unitAnimator.SetTrigger("Active");

            _readyForBuffs = true;

            if (!AttackedThisTurn && !IsPlayable)
            {
                IsPlayable = true;
                SetHighlightingEnabled(true);
            }
        }

        public void BuffShield()
        {
            BuffUnit(Enumerators.BuffType.SHIELD);
            HasBuffShield = true;
            _shieldSprite.SetActive(true);
        }

        public void ArrivalAnimationEventHandler(string param)
        {
            if (param.Equals("ArrivalAnimationDone"))
            {
                unitContentObject.SetActive(true);

                if (!_ignoreArrivalEndEvents)
                {
                    if (hasFeral)
                    {
                        //  frameSprite.sprite = frameSprites[1];
                        StopSleepingParticles();
                        if (ownerPlayer != null)
                            SetHighlightingEnabled(true);
                    }
                }

                InternalTools.SetLayerRecursively(_selfObject, 0, new List<string>() { _sleepingParticles.name, _shieldSprite.name });
                if (!ownerPlayer.IsLocalPlayer)
                    _shieldSprite.transform.position = new Vector3(_shieldSprite.transform.position.x, _shieldSprite.transform.position.y, -_shieldSprite.transform.position.z);

                if (!_ignoreArrivalEndEvents)
                {
                    if (Card.libraryCard.cardRank == Enumerators.CardRank.COMMANDER)
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.CARDS, Card.libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_PLAY + "1", Constants.ZOMBIES_SOUND_VOLUME, false, true);
                        _soundManager.PlaySound(Enumerators.SoundType.CARDS, Card.libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_PLAY + "2", Constants.ZOMBIES_SOUND_VOLUME / 2f, false, true);
                    }
                    else
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.CARDS, Card.libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_PLAY, Constants.ZOMBIES_SOUND_VOLUME, false, true);
                    }


                    if (Card.libraryCard.name.Equals("Freezzee"))
                    {
                        var freezzees = GetEnemyUnitsList(this).FindAll(x => x.Card.libraryCard.id == Card.libraryCard.id);

                        if (freezzees.Count > 0)
                        {
                            foreach (var creature in freezzees)
                            {
                                creature.Stun(Enumerators.StunType.FREEZE, 1);
                                CreateFrozenVFX(creature.transform.position);
                            }
                        }
                    }


                    _readyForBuffs = true;
                    _ranksController.UpdateRanksBuffs(ownerPlayer, Card.libraryCard.cardRank);
                }
            }
            else if (param.Equals("ArrivalAnimationHeavySetLayerUnderBattleFrame"))
            {
                InternalTools.SetLayerRecursively(gameObject, 0, new List<string>() { _sleepingParticles.name, _shieldSprite.name });

                _animationSprite.sortingOrder = -_animationSprite.sortingOrder;
                _pictureSprite.sortingOrder = -_pictureSprite.sortingOrder;
            }

            _initialScale = _selfObject.transform.localScale;

            _ignoreArrivalEndEvents = false;

            _arrivalDone = true;
        }

        private void CreateFrozenVFX(Vector3 pos)
        {
            var _frozenVFX = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX"));
            _frozenVFX.transform.position = Utilites.CastVFXPosition(pos + Vector3.forward);
            DestroyCurrentParticle(_frozenVFX);
        }

        private void DestroyCurrentParticle(GameObject currentParticle, bool isDirectly = false, float time = 5f)
        {
            if (isDirectly)
                DestroyParticle(new object[] { currentParticle });
            else
                _timerManager.AddTimer(DestroyParticle, new object[] { currentParticle }, time, false);
        }

        private void DestroyParticle(object[] param)
        {
            GameObject particleObj = param[0] as GameObject;
            MonoBehaviour.Destroy(particleObj);
        }

        private List<BoardUnit> GetEnemyUnitsList(BoardUnit unit)
        {
            if (_gameplayManager.CurrentPlayer.BoardCards.Contains(unit))
                return _gameplayManager.OpponentPlayer.BoardCards;
            return _gameplayManager.CurrentPlayer.BoardCards;
        }

        public void SetObjectInfo(WorkingCard card, string setName = "")
        {
            Card = card;

            // hack for top zombies
            if (!ownerPlayer.IsLocalPlayer)
                _sleepingParticles.transform.localPosition = new Vector3(_sleepingParticles.transform.localPosition.x, _sleepingParticles.transform.localPosition.y, 3f);

            _pictureSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), Card.libraryCard.cardRank.ToString().ToLower(), Card.libraryCard.picture.ToLower()));

            _pictureSprite.transform.localPosition = MathLib.FloatVector3ToVector3(Card.libraryCard.cardViewInfo.position);
            _pictureSprite.transform.localScale = MathLib.FloatVector3ToVector3(Card.libraryCard.cardViewInfo.scale);

            unitAnimator.runtimeAnimatorController = animatorControllers.Find(x => x.cardType == Card.libraryCard.cardType).animator;
            if (Card.type == Enumerators.CardType.WALKER)
            {
                _sleepingParticles.transform.position += Vector3.up * 0.7f;
            }

            CurrentDamage = card.damage;
            CurrentHP = card.health;

            BuffedDamage = 0;
            BuffedHP = 0;

            initialDamage = CurrentDamage;
            initialHP = CurrentHP;

            _attackText.text = CurrentDamage.ToString();
            _healthText.text = CurrentHP.ToString();

            damageChangedDelegate = () =>
            {
                UpdateUnitInfoText(_attackText, CurrentDamage, initialDamage);
            };

            UnitDamageChangedEvent += damageChangedDelegate;

            healthChangedDelegate = () =>
            {
                UpdateUnitInfoText(_healthText, CurrentHP, initialHP);
                CheckOnDie();
            };

            UnitHPChangedEvent += healthChangedDelegate;

            _initialUnitType = Card.libraryCard.cardType;

            switch (_initialUnitType)
            {
                case Enumerators.CardType.FERAL:
                    hasFeral = true;
                    IsPlayable = true;
                    _timerManager.AddTimer((x)=>
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.FERAL_ARRIVAL, Constants.ARRIVAL_SOUND_VOLUME, false, false, true);
                    }, null, .55f, false);
                    
                    break;
                case Enumerators.CardType.HEAVY:
                    _timerManager.AddTimer((x) =>
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.HEAVY_ARRIVAL, Constants.ARRIVAL_SOUND_VOLUME, false, false, true);
                    }, null, 1f, false);

                    hasHeavy = true;
                    break;
                case Enumerators.CardType.WALKER:
                default:
                    _timerManager.AddTimer((x) =>
                    { 
                    _soundManager.PlaySound(Enumerators.SoundType.WALKER_ARRIVAL, Constants.ARRIVAL_SOUND_VOLUME, false, false, true);
                    }, null, .6f, false);

                    break;
            }

            if (hasHeavy)
            {
                //   glowSprite.gameObject.SetActive(false);
                //  pictureMaskTransform.localScale = new Vector3(50, 55, 1);
                // frameSprite.sprite = frameSprites[2];
            }
            SetHighlightingEnabled(false);

            unitAnimator.StopPlayback();
            unitAnimator.Play(0);
        }

        private void CheckOnDie()
        {
            if (CurrentHP <= 0 && !_dead)
            {
                Debug.Log(IsAllAbilitiesResolvedAtStart + " | " + _arrivalDone);

                if (IsAllAbilitiesResolvedAtStart && _arrivalDone)
                    Die();
            }
        }

        public void PlayArrivalAnimation()
        {
            unitAnimator.SetTrigger("Active");
        }

        public void OnStartTurn()
        {
            attackedBoardObjectsThisTurn.Clear();
            numTurnsOnBoard++;
            StopSleepingParticles();

            if (ownerPlayer != null && IsPlayable && _gameplayManager.CurrentTurnPlayer.Equals(ownerPlayer))
            {
                if (CurrentDamage > 0)
                    SetHighlightingEnabled(true);

                AttackedThisTurn = false;

                IsCreatedThisTurn = false;
            } 
        }

        public void OnEndTurn()
        {
            if (_stunTurns > 0)
                _stunTurns--;
            if (_stunTurns == 0)
            {
                IsPlayable = true;
                _frozenSprite.DOFade(0, 1);
                UnitStatus = Enumerators.UnitStatusType.NONE;
            }

            HasBuffRush = false;

            CancelTargetingArrows();

            if (DamageDebuffUntillEndOfTurn != 0)
            {
                CurrentDamage -= DamageDebuffUntillEndOfTurn;
                DamageDebuffUntillEndOfTurn = 0;
            }
            if (HPDebuffUntillEndOfTurn != 0)
            {
                CurrentHP -= HPDebuffUntillEndOfTurn;
                HPDebuffUntillEndOfTurn = 0;
            }       
        }


        public void SetSelectedUnit(bool status)
        {
            _glowSelectedObjectSprite.SetActive(status);

            if (status)
                _selfObject.transform.localScale = _initialScale + Vector3.one * 0.1f;
            else
                _selfObject.transform.localScale = _initialScale;
        }


        public void Stun(Enumerators.StunType stunType, int turns)
        {
            if (turns > _stunTurns)
                _stunTurns = turns;
            IsPlayable = false;

            _frozenSprite.DOFade(1, 1);

            UnitStatus = Enumerators.UnitStatusType.FROZEN;
            //sleepingParticles.Play();
        }

        public void CancelTargetingArrows()
        {
            if (abilitiesTargetingArrow != null)
            {
                MonoBehaviour.Destroy(abilitiesTargetingArrow.gameObject);
            }
            if (fightTargetingArrow != null)
            {
                MonoBehaviour.Destroy(fightTargetingArrow.gameObject);
            }
        }

        private void UpdateUnitInfoText(TextMeshPro text, int stat, int initialStat)
        {
            if (text == null || !text)
                return;

            text.text = stat.ToString();

            if (stat > initialStat)
                text.color = Color.green;
            else if (stat < initialStat)
                text.color = Color.red;
            else
            {
                text.color = Color.white;
            }
            var sequence = DOTween.Sequence();
            sequence.Append(text.transform.DOScale(new Vector3(1.4f, 1.4f, 1.0f), 0.4f));
            sequence.Append(text.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.2f));
            sequence.Play();
        }

        public void SetHighlightingEnabled(bool enabled)
        {
            if (!UnitCanBeUsable())
                enabled = false;

            if (_glowSprite)
                _glowSprite.enabled = enabled;
        }

        public void StopSleepingParticles()
        {
            if (_sleepingParticles != null)
                _sleepingParticles.Stop();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.transform.parent != null)
            {
                var targetingArrow = collider.transform.parent.GetComponent<BoardArrow>();
                if (targetingArrow != null)
                {
                    targetingArrow.OnCardSelected(this);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.transform.parent != null)
            {
                var targetingArrow = collider.transform.parent.GetComponent<BoardArrow>();
                if (targetingArrow != null)
                {
                    targetingArrow.OnCardUnselected(this);
                }
            }
        }

        private void OnMouseDown(GameObject obj)
        {
            //if (fightTargetingArrowPrefab == null)
            //    return;

            //Debug.LogError(IsPlayable + " | " + ownerPlayer.isActivePlayer + " | " + ownerPlayer);

            if (_gameplayManager.IsTutorial && _gameplayManager.TutorialStep == 18)
                return;

            if (ownerPlayer != null && ownerPlayer.IsLocalPlayer && _playerController.IsActive && UnitCanBeUsable())
            {
                fightTargetingArrow = MonoBehaviour.Instantiate(_fightTargetingArrowPrefab).AddComponent<BattleBoardArrow>();
                fightTargetingArrow.targetsType = new List<Enumerators.SkillTargetType>() { Enumerators.SkillTargetType.OPPONENT, Enumerators.SkillTargetType.OPPONENT_CARD };
                fightTargetingArrow.BoardCards = _gameplayManager.OpponentPlayer.BoardCards;
                fightTargetingArrow.owner = this;
                fightTargetingArrow.Begin(transform.position);

                if (attackInfoType == Enumerators.AttackInfoType.ONLY_DIFFERENT)
                    fightTargetingArrow.ignoreBoardObjectsList = attackedBoardObjectsThisTurn;

                if (ownerPlayer.Equals(_gameplayManager.CurrentPlayer))
                {
                    _battlegroundController.DestroyCardPreview();
                    _playerController.IsCardSelected = true;

                    if (_tutorialManager.IsTutorial)
                        _tutorialManager.DeactivateSelectTarget();
                }

                _soundManager.StopPlaying(Enumerators.SoundType.CARDS);
                _soundManager.PlaySound(Enumerators.SoundType.CARDS, Card.libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);
            }
        }

        private void OnMouseUp(GameObject obj)
        {
            if (ownerPlayer != null && ownerPlayer.IsLocalPlayer && _playerController.IsActive && UnitCanBeUsable())
            {
                if (fightTargetingArrow != null)
                {
                    fightTargetingArrow.End(this);

                    if (ownerPlayer.Equals(_gameplayManager.CurrentPlayer))
                    {
                        _playerController.IsCardSelected = false;
                    }
                }
            }
        }

        public void ForceSetCreaturePlayable()
        {
            if (IsStun)
                return;

            SetHighlightingEnabled(true);
            IsPlayable = true;
        }

        public void DoCombat(object target)
        {
            if (target == null)
            {
                if (_tutorialManager.IsTutorial)
                    _tutorialManager.ActivateSelectTarget();
                return;
            }

            IsAttacking = true;

            var sortingGroup = _selfObject.GetComponent<SortingGroup>();

            if (target is Player)
            {
                var targetPlayer = target as Player;
                SetHighlightingEnabled(false);
                IsPlayable = false;
                AttackedThisTurn = true;

                // GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);

                //sortingGroup.sortingOrder = 100;

                _actionsQueueController.AddNewActionInToQueue((Action<object, Action>)((parameter, completeCallback) =>
                {
                    attackedBoardObjectsThisTurn.Add(targetPlayer);

                    _animationsController.DoFightAnimation(_selfObject, targetPlayer.AvatarObject, 0.1f, (Action)(() =>
                    {

                        Vector3 positionOfVFX = targetPlayer.AvatarObject.transform.position;
                        // positionOfVFX.y = 4.45f; // was used only for local player

                        _vfxController.PlayAttackVFX(Card.libraryCard.cardType, positionOfVFX, CurrentDamage);

                        _battleController.AttackPlayerByUnit(this, targetPlayer);
                    }),
                    () =>
                    {
                        //sortingGroup.sortingOrder = 0;
                        fightTargetingArrow = null;
                        IsAttacking = false;
                       // completeCallback?.Invoke();
                    });

                    _timerManager.AddTimer((x) =>
                    {
                        completeCallback?.Invoke();
                    }, null, 1.5f);
                }));
            }
            else if (target is BoardUnit)
            {
                var targetCard = target as BoardUnit;
                SetHighlightingEnabled(false);
                IsPlayable = false;

                _actionsQueueController.AddNewActionInToQueue(((parameter, completeCallback) =>
                {
                    attackedBoardObjectsThisTurn.Add(targetCard);

                    //sortingGroup.sortingOrder = 100;

                    // play sound when target creature attack more than our
                   // if (targetCard.CurrentDamage > CurrentDamage)
                    //    _soundManager.PlaySound(Enumerators.SoundType.CARDS, targetCard.Card.libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_ATTACK, Constants.ZOMBIES_SOUND_VOLUME, false, true);

                    _animationsController.DoFightAnimation(_selfObject, targetCard.transform.gameObject, 0.5f, (Action)(() =>
                    {
                        _vfxController.PlayAttackVFX(Card.libraryCard.cardType, targetCard.transform.position, CurrentDamage);

                        _battleController.AttackUnitByUnit(this, targetCard, AdditionalDamage);

                        if(TakeFreezeToAttacked)
                            targetCard.Stun(Enumerators.StunType.FREEZE, 1);
                    }),
                    () =>
                    {
                        //sortingGroup.sortingOrder = 0;
                        fightTargetingArrow = null;
                        IsAttacking = false;
                    });

                    _timerManager.AddTimer((x) =>
                    {
                        completeCallback?.Invoke();
                    }, null, 1.5f);
                }));
            }
        }

        public bool UnitCanBeUsable()
        {
            if (CurrentHP <= 0 || CurrentDamage <= 0 || IsStun)
                return false;

            if (IsPlayable)
            {
                if (IsFeralUnit())
                    return true;

                if (numTurnsOnBoard >= 1)
                    return true;
            }
            else if (!AttackedThisTurn && HasBuffRush)
                return true;

            return false;
        }

        public void ThrowEventGotDamage(object from)
        {
            UnitGotDamageEvent?.Invoke(from);
        }

        public void MoveUnitFromBoardToDeck()
        {
            try
            {
                Die(true);

                if (_arrivalDone)
                    MonoBehaviour.Destroy(gameObject);
                else
                {
                    _timerManager.AddTimer(CheckIsCanDie, null, Time.deltaTime, true);
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public void ThrowOnAttackEvent(object target, int damage, bool isAttacker)
        {
            UnitOnAttackEvent?.Invoke(target, damage, isAttacker);
        }
        public void ThrowOnDieEvent()
        {
            UnitOnDieEvent?.Invoke();
        }

      
        private void CheckIsCanDie(object[] param)
        {
            if(_arrivalDone)
            {
                _timerManager.StopTimer(CheckIsCanDie);
                MonoBehaviour.Destroy(gameObject);
            }
        }
    }

    [Serializable]
    public class UnitAnimatorInfo
    {
        public Enumerators.CardType cardType;
        public RuntimeAnimatorController animator;
    }
}