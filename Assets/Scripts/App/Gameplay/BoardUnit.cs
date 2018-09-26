using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class BoardUnit
    {
        public bool AttackedThisTurn;

        public bool HasFeral;

        public bool HasHeavy;

        public int NumTurnsOnBoard;

        public int InitialDamage;

        public int InitialHp;

        public bool HasUsedBuffShield;

        public Player OwnerPlayer;

        public List<object> AttackedBoardObjectsThisTurn;

        public Enumerators.AttackInfoType AttackInfoType = Enumerators.AttackInfoType.ANY;

        private readonly ILoadObjectsManager _loadObjectsManager;

        private readonly IGameplayManager _gameplayManager;

        private readonly ISoundManager _soundManager;

        private readonly ITimerManager _timerManager;

        private readonly ITutorialManager _tutorialManager;

        private readonly PlayerController _playerController;

        private readonly BattlegroundController _battlegroundController;

        private readonly AnimationsController _animationsController;

        private readonly BattleController _battleController;

        private readonly ActionsQueueController _actionsQueueController;

        private readonly VfxController _vfxController;

        private readonly RanksController _ranksController;

        private readonly AbilitiesController _abilitiesController;

        private readonly CardsController _cardsController;

        private readonly InputController _inputController;

        private readonly BoardArrowController _boardArrowController;

        private readonly GameObject _fightTargetingArrowPrefab;

        private readonly SpriteRenderer _pictureSprite;

        private readonly SpriteRenderer _frozenSprite;

        private readonly GameObject _shieldSprite;

        private readonly TextMeshPro _attackText;

        private readonly TextMeshPro _healthText;

        private readonly ParticleSystem _sleepingParticles;

        private readonly GameObject _unitContentObject;

        private Action _damageChangedDelegate;

        private Action _healthChangedDelegate;

        private GameObject _battleframeObject;

        private GameObject _glowObj;

        private GameObject _glowSelectedObject;

        private Vector3 _initialScale = new Vector3(0.9f, 0.9f, 0.9f);

        private int _currentDamage;

        private int _currentHealth;

        private int _stunTurns;

        private bool _readyForBuffs;

        private bool _ignoreArrivalEndEvents;

        private BattleBoardArrow _fightTargetingArrow;

        private bool _dead;

        private bool _arrivalDone;

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
            _vfxController = _gameplayManager.GetController<VfxController>();
            _ranksController = _gameplayManager.GetController<RanksController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _inputController = _gameplayManager.GetController<InputController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();


            GameObject =
                Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/BoardCreature"));
            GameObject.transform.SetParent(parent, false);

            _fightTargetingArrowPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");

            _pictureSprite = GameObject.transform.Find("CreaturePicture").GetComponent<SpriteRenderer>();
            _frozenSprite = GameObject.transform.Find("Other/Frozen").GetComponent<SpriteRenderer>();
            //_glowObj = GameObject.transform.Find("Other/Glow").GetComponent<SpriteRenderer>();
            _shieldSprite = GameObject.transform.Find("Other/Shield").gameObject;

            //_glowSelectedObject = GameObject.transform.Find("Other/GlowSelectedObject").gameObject;

            _attackText = GameObject.transform.Find("Other/AttackAndDefence/AttackText").GetComponent<TextMeshPro>();
            _healthText = GameObject.transform.Find("Other/AttackAndDefence/DefenceText").GetComponent<TextMeshPro>();

            _sleepingParticles = GameObject.transform.Find("Other/SleepingParticles").GetComponent<ParticleSystem>();

            _unitContentObject = GameObject.transform.Find("Other").gameObject;
            _unitContentObject.SetActive(false);

            _inputController.UnitSelectedEvent += UnitSelectedEventHandler;
            _inputController.UnitDeselectedEvent += UnitDeselectedEventHandler;

            BuffsOnUnit = new List<Enumerators.BuffType>();
            AttackedBoardObjectsThisTurn = new List<object>();

            //_glowObj.gameObject.SetActive(false);
            //_glowObj.enabled = false;

            IsCreatedThisTurn = true;

            UnitStatus = Enumerators.UnitStatusType.NONE;

            IsAllAbilitiesResolvedAtStart = true;

            _gameplayManager.CanDoDragActions = false;
        }

        public event Action UnitDied;

        public event Action<object, int, bool> UnitAttacked;

        public event Action<object> UnitDamaged;

        public event Action UnitHpChanged;

        public event Action UnitDamageChanged;

        public Enumerators.CardType InitialUnitType { get; private set; }

        public int MaxCurrentDamage => InitialDamage + BuffedDamage;

        public int BuffedDamage { get; set; }

        public int CurrentDamage
        {
            get => _currentDamage;
            set
            {
                _currentDamage = Mathf.Clamp(value, 0, 99999);
                UnitDamageChanged?.Invoke();
            }
        }

        public int MaxCurrentHp => InitialHp + BuffedHp;

        public int BuffedHp { get; set; }

        public int CurrentHp
        {
            get => _currentHealth;
            set
            {
                _currentHealth = Mathf.Clamp(value, 0, 99);
                UnitHpChanged?.Invoke();
            }
        }

        public Transform Transform => GameObject.transform;

        public GameObject GameObject { get; }

        public Sprite Sprite => _pictureSprite.sprite;

        public bool IsPlayable { get; set; }

        public WorkingCard Card { get; private set; }

        public bool IsStun => _stunTurns > 0;

        public bool IsCreatedThisTurn { get; private set; }

        public List<Enumerators.BuffType> BuffsOnUnit { get; }

        public bool HasBuffRush { get; set; }

        public bool HasBuffHeavy { get; set; }

        public bool HasBuffShield { get; set; }

        public bool TakeFreezeToAttacked { get; set; }

        public int AdditionalDamage { get; set; }

        public int DamageDebuffUntillEndOfTurn { get; set; }

        public int HpDebuffUntillEndOfTurn { get; set; }

        public bool IsAttacking { get; private set; }

        public bool IsAllAbilitiesResolvedAtStart { get; set; }

        public bool IsReanimated { get; set; }

        public bool AttackAsFirst { get; set; }

        public Enumerators.UnitStatusType UnitStatus { get; set; }

        public bool CantAttackInThisTurnBlocker { get; set; } = false;

        public bool IsHeavyUnit()
        {
            return HasBuffHeavy || HasHeavy;
        }

        public bool IsFeralUnit()
        {
            return HasFeral;
        }

        public void Update()
        {
            CheckOnDie();
        }

        public void Die(bool returnToHand = false)
        {
            _timerManager.StopTimer(CheckIsCanDie);

            UnitHpChanged -= _healthChangedDelegate;
            UnitDamageChanged -= _damageChangedDelegate;

            _dead = true;
            if (!returnToHand)
            {
                _battlegroundController.KillBoardCard(this);
            }
            else
            {
                InvokeUnitDied();
            }
        }

        public void ResolveBuffShield () {
            if (HasUsedBuffShield) {
                HasUsedBuffShield = false;
                this.UseShieldFromBuff();
            }
        }

        public void BuffUnit(Enumerators.BuffType type)
        {
            if (!_readyForBuffs)
                return;

            BuffsOnUnit.Add(type);
        }

        public void ApplyBuff(Enumerators.BuffType type)
        {
            if (!_readyForBuffs)
                return;

            switch (type)
            {
                case Enumerators.BuffType.ATTACK:
                    CurrentDamage++;
                    break;
                case Enumerators.BuffType.DAMAGE:
                    break;
                case Enumerators.BuffType.DEFENCE:
                    CurrentHp++;
                    break;
                case Enumerators.BuffType.FREEZE:
                    TakeFreezeToAttacked = true;
                    break;
                case Enumerators.BuffType.HEAVY:
                    HasBuffHeavy = true;
                    break;
                case Enumerators.BuffType.RUSH:
                    if (NumTurnsOnBoard == 0)
                    {
                        HasBuffRush = true;
                        if (InitialUnitType != Enumerators.CardType.FERAL)
                            SetNormalGlowFromUnitType();
                    }

                    _sleepingParticles.gameObject.SetActive(false);
                    break;
                case Enumerators.BuffType.GUARD:
                    HasBuffShield = true;
                    break;
                case Enumerators.BuffType.REANIMATE:
                    _abilitiesController.BuffUnitByAbility(Enumerators.AbilityType.REANIMATE_UNIT, this,
                        Card.LibraryCard, OwnerPlayer);
                    break;
                case Enumerators.BuffType.DESTROY:
                    _abilitiesController.BuffUnitByAbility(Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK,
                        this, Card.LibraryCard, OwnerPlayer);
                    break;
            }

            UpdateFrameByType();
        }

        public void UseShieldFromBuff()
        {
            HasBuffShield = false;
            BuffsOnUnit.Remove(Enumerators.BuffType.GUARD);
            _shieldSprite.SetActive(HasBuffShield);
        }

        public void UpdateFrameByType()
        {
            _shieldSprite.SetActive(HasBuffShield);

            if (HasBuffHeavy)
            {
                SetAsHeavyUnit();
            }
            else
            {
                switch (InitialUnitType)
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
                }
            }
            SetNormalGlowFromUnitType();
            SetAttackGlowFromUnitType();
        }

        public void SetAsHeavyUnit()
        {
            if (HasHeavy)
                return;

            HasHeavy = true;
            HasFeral = false;
            InitialUnitType = Enumerators.CardType.HEAVY;

            ChangeTypeFrame(2.5f, 1.7f);
            if (!AttackedThisTurn && NumTurnsOnBoard == 0)
            {
                IsPlayable = false;
                SetHighlightingEnabled(false);
            }
            else if (!AttackedThisTurn && IsPlayable && !CantAttackInThisTurnBlocker)
            {
                StopSleepingParticles();
            }
        }

        public void SetAsWalkerUnit()
        {
            if (!HasHeavy && !HasFeral && !HasBuffHeavy)
                return;

            HasHeavy = false;
            HasFeral = false;
            HasBuffHeavy = false;
            InitialUnitType = Enumerators.CardType.WALKER;

            ChangeTypeFrame(1.3f, 0.3f);
        }

        public void SetAsFeralUnit()
        {
            if (HasFeral)
                return;

            HasHeavy = false;
            HasBuffHeavy = false;
            HasFeral = true;
            InitialUnitType = Enumerators.CardType.FERAL;

            ChangeTypeFrame(2.7f, 1.7f);

            if (!AttackedThisTurn && !IsPlayable)
            {
                StopSleepingParticles();
                IsPlayable = true;
                SetHighlightingEnabled(true);
            }
        }

        public void BuffShield()
        {
            BuffUnit(Enumerators.BuffType.GUARD);
            HasBuffShield = true;
            _shieldSprite.SetActive(true);
        }

        public void ArrivalAnimationEventHandler()
        {
            _unitContentObject.SetActive(true);
            if (HasFeral || NumTurnsOnBoard > 0 && !CantAttackInThisTurnBlocker)
            {
                StopSleepingParticles();
            }

            if (!_ignoreArrivalEndEvents)
            {
                if (HasFeral)
                {
                    if (OwnerPlayer != null)
                    {
                        SetHighlightingEnabled(true);
                    }
                }
            }

            if (!_ignoreArrivalEndEvents)
            {
                if (Card.LibraryCard.CardRank == Enumerators.CardRank.COMMANDER)
                {
                    _soundManager.PlaySound(Enumerators.SoundType.CARDS,
                        Card.LibraryCard.Name.ToLower() + "_" + Constants.CardSoundPlay + "1",
                        Constants.ZombiesSoundVolume, false, true);
                    _soundManager.PlaySound(Enumerators.SoundType.CARDS,
                        Card.LibraryCard.Name.ToLower() + "_" + Constants.CardSoundPlay + "2",
                        Constants.ZombiesSoundVolume / 2f, false, true);
                }
                else
                {
                    _soundManager.PlaySound(Enumerators.SoundType.CARDS,
                        Card.LibraryCard.Name.ToLower() + "_" + Constants.CardSoundPlay, Constants.ZombiesSoundVolume,
                        false, true);
                }

                if (Card.LibraryCard.Name.Equals("Freezzee"))
                {
                    List<BoardUnit> freezzees = GetEnemyUnitsList(this)
                        .FindAll(x => x.Card.LibraryCard.Id == Card.LibraryCard.Id);

                    if (freezzees.Count > 0)
                    {
                        foreach (BoardUnit creature in freezzees)
                        {
                            creature.Stun(Enumerators.StunType.FREEZE, 1);
                            CreateFrozenVfx(creature.Transform.position);
                        }
                    }
                }

                _readyForBuffs = true;
                _ranksController.UpdateRanksByElements(OwnerPlayer.BoardCards, Card.LibraryCard);
            }

            _initialScale = GameObject.transform.localScale;

            _ignoreArrivalEndEvents = false;

            _arrivalDone = true;
        }

        public void SetObjectInfo(WorkingCard card)
        {
            Card = card;

            // hack for top zombies
            if (!OwnerPlayer.IsLocalPlayer)
            {
                _sleepingParticles.transform.localPosition = new Vector3(_sleepingParticles.transform.localPosition.x,
                    _sleepingParticles.transform.localPosition.y, 3f);
            }

            string setName = _cardsController.GetSetOfCard(card.LibraryCard);
            string rank = Card.LibraryCard.CardRank.ToString().ToLower();
            string picture = Card.LibraryCard.Picture.ToLower();

            string fullPathToPicture =
                string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rank, picture);

            _pictureSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(fullPathToPicture);

            // DEBUG FOR FIDNING PROBLEM WITH PICTURE NOT FOUND
            if (_pictureSprite.sprite == null)
            {
                string data = string.Empty;

                data += "---------- BEGIN: " + Time.time + "----------------" + Environment.NewLine;
                data += card.LibraryCard.Name + Environment.NewLine;
                data += rank + " | " + picture + setName + Environment.NewLine;
                data += fullPathToPicture + Environment.NewLine;
                data += "---------- END: " + Time.time + "----------------";

                Debug.LogError(data);

                string pathToLogFolder = Application.persistentDataPath + "/BOARD_UNIT_" + card.LibraryCard.Name +
                    "_PICTURE_ERROR.txt";
                File.WriteAllText(pathToLogFolder, data);
                Process.Start(pathToLogFolder);
            }

            _pictureSprite.transform.localPosition = (Vector3) Card.LibraryCard.CardViewInfo.Position;
            _pictureSprite.transform.localScale = (Vector3) Card.LibraryCard.CardViewInfo.Scale;

            if (Card.Type == Enumerators.CardType.WALKER)
            {
                _sleepingParticles.transform.position += Vector3.up * 0.7f;
            }

            CurrentDamage = card.Damage;
            CurrentHp = card.Health;

            BuffedDamage = 0;
            BuffedHp = 0;

            InitialDamage = CurrentDamage;
            InitialHp = CurrentHp;

            _attackText.text = CurrentDamage.ToString();
            _healthText.text = CurrentHp.ToString();

            _damageChangedDelegate = () =>
            {
                UpdateUnitInfoText(_attackText, CurrentDamage, InitialDamage, MaxCurrentDamage);
            };

            UnitDamageChanged += _damageChangedDelegate;

            _healthChangedDelegate = () =>
            {
                UpdateUnitInfoText(_healthText, CurrentHp, InitialHp, MaxCurrentHp);
                CheckOnDie();
            };

            UnitHpChanged += _healthChangedDelegate;

            InitialUnitType = Card.LibraryCard.CardType;

            switch (InitialUnitType)
            {
                case Enumerators.CardType.FERAL:
                    HasFeral = true;
                    IsPlayable = true;
                    _timerManager.AddTimer(
                        x =>
                        {
                            _soundManager.PlaySound(Enumerators.SoundType.FERAL_ARRIVAL, Constants.ArrivalSoundVolume,
                                false, false, true);
                        },
                        null,
                        .55f);

                    _timerManager.AddTimer(
                        x =>
                        {
                            ArrivalAnimationEventHandler();
                        },
                        null,
                        OwnerPlayer.IsLocalPlayer ? 2.9f : 1.7f);

                    break;
                case Enumerators.CardType.HEAVY:
                    _timerManager.AddTimer(
                        x =>
                        {
                            _soundManager.PlaySound(Enumerators.SoundType.HEAVY_ARRIVAL, Constants.ArrivalSoundVolume,
                                false, false, true);
                        });

                    _timerManager.AddTimer(
                        x =>
                        {
                            ArrivalAnimationEventHandler();
                        },
                        null,
                        OwnerPlayer.IsLocalPlayer ? 2.7f : 1.7f);
                    HasHeavy = true;
                    break;
                case Enumerators.CardType.WALKER:
                default:
                    _timerManager.AddTimer(
                        x =>
                        {
                            _soundManager.PlaySound(Enumerators.SoundType.WALKER_ARRIVAL, Constants.ArrivalSoundVolume,
                                false, false, true);
                        },
                        null,
                        .6f);
                    _timerManager.AddTimer(
                        x =>
                        {
                            ArrivalAnimationEventHandler();
                        },
                        null,
                        OwnerPlayer.IsLocalPlayer ? 1.3f : 0.3f);

                    break;
            }
            SetNormalGlowFromUnitType();
            SetAttackGlowFromUnitType();
            SetHighlightingEnabled(false);
        }

        public void PlayArrivalAnimation(bool firstAppear = true)
        {
            GameObject arrivalPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/" + InitialUnitType + "_Arrival_VFX");
            _battleframeObject = Object.Instantiate(arrivalPrefab, GameObject.transform, false).gameObject;
            Transform spriteContainerTransform =
                _battleframeObject.transform.Find("Main_Model/Root/FangMain/SpriteContainer");
            Vector3 scale = spriteContainerTransform.transform.localScale;
            scale.x *= -1;
            spriteContainerTransform.transform.localScale = scale;
            _pictureSprite.transform.SetParent(spriteContainerTransform, false);
            if(firstAppear)
                GameObject.transform.position += Vector3.back * 5f;
        }

        public void OnStartTurn()
        {
            Debug.Log("OnStartTurn");
            AttackedBoardObjectsThisTurn.Clear();
            NumTurnsOnBoard++;
            StopSleepingParticles();

            if (_stunTurns > 0)
            {
                _stunTurns--;
            }

            if (_stunTurns == 0)
            {
                IsPlayable = true;
                _frozenSprite.DOFade(0, 1);
                UnitStatus = Enumerators.UnitStatusType.NONE;
            }

            if (OwnerPlayer != null && IsPlayable && _gameplayManager.CurrentTurnPlayer.Equals(OwnerPlayer))
            {
                if (CurrentDamage > 0)
                {
                    SetHighlightingEnabled(true);
                }

                AttackedThisTurn = false;

                IsCreatedThisTurn = false;
            }
        }

        public void OnEndTurn()
        {
            if (HasBuffRush)
            {
                HasBuffRush = false;
                if (InitialUnitType != Enumerators.CardType.FERAL)
                {
                    SetNormalGlowFromUnitType();
                }
            }

            CantAttackInThisTurnBlocker = false;

            CancelTargetingArrows();
        }

        public void SetSelectedUnit(bool status)
        {
            _glowSelectedObject.SetActive(status);

            if (status)
            {
                GameObject.transform.localScale = _initialScale + Vector3.one * 0.1f;
            }
            else
            {
                GameObject.transform.localScale = _initialScale;
            }
        }

        public void Stun(Enumerators.StunType stunType, int turns)
        {
            if (AttackedThisTurn || NumTurnsOnBoard == 0)
                turns++;

            if (turns > _stunTurns)
            {
                _stunTurns = turns;
            }

            IsPlayable = false;

            _frozenSprite.DOFade(1, 1);

            SetHighlightingEnabled(false);

            UnitStatus = Enumerators.UnitStatusType.FROZEN;
        }

        public void CancelTargetingArrows()
        {
            if (_fightTargetingArrow != null)
            {
                _fightTargetingArrow.Dispose();
            }
        }

        public void SetHighlightingEnabled(bool enabled)
        {
            if (!UnitCanBeUsable())
            {
                enabled = false;
            }

            if (_glowObj)
            {
                _glowObj.SetActive(enabled);
            }
        }

        public void StopSleepingParticles()
        {
            if (_sleepingParticles != null)
            {
                _sleepingParticles.Stop();
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
                {
                    _tutorialManager.ActivateSelectTarget();
                }

                return;
            }

            IsAttacking = true;

            switch (target)
            {
                case Player targetPlayer:
                    SetHighlightingEnabled(false);
                    IsPlayable = false;
                    AttackedThisTurn = true;

                    _actionsQueueController.AddNewActionInToQueue(
                        (parameter, completeCallback) =>
                        {
                            AttackedBoardObjectsThisTurn.Add(targetPlayer);

                            _animationsController.DoFightAnimation(
                                GameObject,
                                targetPlayer.AvatarObject,
                                0.1f,
                                () =>
                                {
                                    Vector3 positionOfVfx = targetPlayer.AvatarObject.transform.position;
                                    _vfxController.PlayAttackVfx(Card.LibraryCard.CardType, positionOfVfx,
                                        CurrentDamage);

                                    _battleController.AttackPlayerByUnit(this, targetPlayer);
                                },
                                () =>
                                {
                                    _fightTargetingArrow = null;
                                    IsAttacking = false;

                                    SetHighlightingEnabled(true);
                                });

                            _timerManager.AddTimer(
                                x =>
                                {
                                    completeCallback?.Invoke();
                                },
                                null,
                                1.5f);
                        });
                    break;
                case BoardUnit targetCard:
                    SetHighlightingEnabled(false);
                    IsPlayable = false;
                    AttackedThisTurn = true;

                    _actionsQueueController.AddNewActionInToQueue(
                        (parameter, completeCallback) =>
                        {
                            AttackedBoardObjectsThisTurn.Add(targetCard);

                            _animationsController.DoFightAnimation(
                                GameObject,
                                targetCard.Transform.gameObject,
                                0.5f,
                                () =>
                                {
                                    _vfxController.PlayAttackVfx(Card.LibraryCard.CardType,
                                        targetCard.Transform.position, CurrentDamage);

                                    _battleController.AttackUnitByUnit(this, targetCard, AdditionalDamage);

                                    if (TakeFreezeToAttacked && targetCard.CurrentHp > 0)
                                    {
                                        if (!targetCard.HasBuffShield)
                                        {
                                            targetCard.Stun(Enumerators.StunType.FREEZE, 1);
                                        } else {
                                            targetCard.HasUsedBuffShield = true;
                                        }
                                    }

                                    targetCard.ResolveBuffShield();
                                    this.ResolveBuffShield();
                                },
                                () =>
                                {
                                    _fightTargetingArrow = null;
                                    IsAttacking = false;

                                    SetHighlightingEnabled(true);
                                });

                            _timerManager.AddTimer(
                                x =>
                                {
                                    completeCallback?.Invoke();
                                },
                                null,
                                1.5f);
                        });
                    break;
            }
        }

        public bool UnitCanBeUsable()
        {
            if (CurrentHp <= 0 || CurrentDamage <= 0 || IsStun || CantAttackInThisTurnBlocker)
            {
                return false;
            }

            if (IsPlayable)
            {
                if (IsFeralUnit())
                {
                    return true;
                }

                if (NumTurnsOnBoard >= 1)
                {
                    return true;
                }
            }
            else if (!AttackedThisTurn && HasBuffRush)
            {
                return true;
            }

            return false;
        }

        public void MoveUnitFromBoardToDeck()
        {
            try
            {
                Die(true);

                if (_arrivalDone)
                {
                    RemoveUnitFromBoard();
                }
                else
                {
                    _timerManager.AddTimer(CheckIsCanDie, null, Time.deltaTime, true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public void InvokeUnitDamaged(object from)
        {
            UnitDamaged?.Invoke(from);
        }

        public void InvokeUnitAttacked(object target, int damage, bool isAttacker)
        {
            UnitAttacked?.Invoke(target, damage, isAttacker);
        }

        public void InvokeUnitDied()
        {
            UnitDied?.Invoke();
        }

        private void ChangeTypeFrame(float playerTime, float opponentTime)
        {
            _ignoreArrivalEndEvents = true;
            _unitContentObject.SetActive(false);

            _pictureSprite.transform.SetParent(GameObject.transform, false);
            _pictureSprite.gameObject.SetActive(false);
            Object.Destroy(_battleframeObject);
            PlayArrivalAnimation(false);
            _pictureSprite.gameObject.SetActive(true);
            _timerManager.AddTimer(
                x =>
                {
                    ArrivalAnimationEventHandler();
                },
                null,
                OwnerPlayer.IsLocalPlayer ? playerTime : opponentTime);

            _readyForBuffs = true;
        }

        private void CreateFrozenVfx(Vector3 pos)
        {
            GameObject frozenVfx =
                Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX"));
            frozenVfx.transform.position = Utilites.CastVfxPosition(pos + Vector3.forward);
            DestroyCurrentParticle(frozenVfx);
        }

        private void DestroyCurrentParticle(GameObject currentParticle, bool isDirectly = false, float time = 5f)
        {
            if (isDirectly)
            {
                DestroyParticle(new object[]
                {
                    currentParticle
                });
            }
            else
            {
                _timerManager.AddTimer(DestroyParticle, new object[]
                {
                    currentParticle
                }, time);
            }
        }

        private void DestroyParticle(object[] param)
        {
            GameObject particleObj = param[0] as GameObject;
            Object.Destroy(particleObj);
        }

        private List<BoardUnit> GetEnemyUnitsList(BoardUnit unit)
        {
            if (_gameplayManager.CurrentPlayer.BoardCards.Contains(unit))
            {
                return _gameplayManager.OpponentPlayer.BoardCards;
            }

            return _gameplayManager.CurrentPlayer.BoardCards;
        }

        private void CheckOnDie()
        {
            if (CurrentHp <= 0 && !_dead)
            {
                if (IsAllAbilitiesResolvedAtStart && _arrivalDone)
                {
                    Die();
                }
            }
        }

        private void UpdateUnitInfoText(TextMeshPro text, int stat, int initialStat, int maxCurrentStat)
        {
            if (text == null || !text)
                return;

            text.text = stat.ToString();

            if (stat > initialStat)
            {
                text.color = Color.green;
            }
            else if (stat < initialStat || stat < maxCurrentStat)
            {
                text.color = Color.red;
            }
            else
            {
                text.color = Color.white;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Append(text.transform.DOScale(new Vector3(1.4f, 1.4f, 1.0f), 0.4f));
            sequence.Append(text.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.2f));
            sequence.Play();
        }

        private void UnitSelectedEventHandler(BoardUnit unit)
        {
            if (_boardArrowController.IsBoardArrowNowInTheBattle || !_gameplayManager.CanDoDragActions)
                return;

            if (unit == this)
            {
                OnMouseDown();
            }
        }

        private void UnitDeselectedEventHandler(BoardUnit unit)
        {
            if (unit == this)
            {
                OnMouseUp();
            }
        }

        private void SetNormalGlowFromUnitType()
        {
            string color = HasBuffRush ? "Orange" : "Green";

            bool active = false;
            if (_glowObj != null)
            {
                active = HasBuffRush ? true : _glowObj.activeInHierarchy;
                Object.Destroy(_glowObj);
            }
            string type = InitialUnitType.ToString().First().ToString().ToUpper() + InitialUnitType.ToString().Substring(1).ToLower();
            _glowObj = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/ActiveFramesCards/ZB_ANM_" + type + "_ActiveFrame_" + color), _unitContentObject.transform, false);
            SetHighlightingEnabled(active);
        }

        private void SetAttackGlowFromUnitType()
        {
            if(_glowSelectedObject != null)
                Object.Destroy(_glowSelectedObject);

            string type = InitialUnitType.ToString().First().ToString().ToUpper() + InitialUnitType.ToString().Substring(1).ToLower();
            _glowSelectedObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/ActiveFramesCards/ZB_ANM_" + type + "_ActiveFrame_Red"), _unitContentObject.transform, false);
            _glowSelectedObject.SetActive(false);
        }

        private void OnMouseDown()
        {
            if (_tutorialManager.IsTutorial && !_tutorialManager.CurrentTutorialDataStep.UnitsCanAttack)
                return;

            if (OwnerPlayer != null && OwnerPlayer.IsLocalPlayer && _playerController.IsActive && UnitCanBeUsable())
            {
                _fightTargetingArrow = Object.Instantiate(_fightTargetingArrowPrefab).AddComponent<BattleBoardArrow>();
                _fightTargetingArrow.TargetsType = new List<Enumerators.SkillTargetType>
                {
                    Enumerators.SkillTargetType.OPPONENT,
                    Enumerators.SkillTargetType.OPPONENT_CARD
                };
                _fightTargetingArrow.BoardCards = _gameplayManager.OpponentPlayer.BoardCards;
                _fightTargetingArrow.Owner = this;
                _fightTargetingArrow.Begin(Transform.position);

                if (AttackInfoType == Enumerators.AttackInfoType.ONLY_DIFFERENT)
                {
                    _fightTargetingArrow.IgnoreBoardObjectsList = AttackedBoardObjectsThisTurn;
                }

                if (OwnerPlayer.Equals(_gameplayManager.CurrentPlayer))
                {
                    _battlegroundController.DestroyCardPreview();
                    _playerController.IsCardSelected = true;

                    if (_tutorialManager.IsTutorial)
                    {
                        _tutorialManager.DeactivateSelectTarget();
                    }
                }

                _soundManager.StopPlaying(Enumerators.SoundType.CARDS);
                _soundManager.PlaySound(Enumerators.SoundType.CARDS,
                    Card.LibraryCard.Name.ToLower() + "_" + Constants.CardSoundAttack, Constants.ZombiesSoundVolume,
                    false, true);
            }
        }

        private void OnMouseUp()
        {
            if (OwnerPlayer != null && OwnerPlayer.IsLocalPlayer && _playerController.IsActive && UnitCanBeUsable())
            {
                if (_fightTargetingArrow != null)
                {
                    _fightTargetingArrow.End(this);

                    if (OwnerPlayer.Equals(_gameplayManager.CurrentPlayer))
                    {
                        _playerController.IsCardSelected = false;
                    }
                }
            }
        }

        private void CheckIsCanDie(object[] param)
        {
            if (_arrivalDone)
            {
                _timerManager.StopTimer(CheckIsCanDie);

                RemoveUnitFromBoard();
            }
        }

        private void RemoveUnitFromBoard()
        {
            OwnerPlayer.BoardCards.Remove(this);
            OwnerPlayer.RemoveCardFromBoard(Card);
            OwnerPlayer.AddCardToGraveyard(Card);

            Object.Destroy(GameObject);
        }
    }

    [Serializable]
    public class UnitAnimatorInfo
    {
        public Enumerators.CardType CardType;

        public RuntimeAnimatorController Animator;
    }
}
