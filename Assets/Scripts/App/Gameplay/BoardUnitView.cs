using System;
using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.View;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class BoardUnitView : IFightSequenceHandler, IView
    {
        private readonly IGameplayManager _gameplayManager;

        private readonly ITutorialManager _tutorialManager;

        private readonly ILoadObjectsManager _loadObjectsManager;

        private readonly ISoundManager _soundManager;

        private readonly ITimerManager _timerManager;

        private readonly AnimationsController _animationsController;

        private readonly VfxController _vfxController;

        private readonly CardsController _cardsController;

        private readonly InputController _inputController;

        private readonly PlayerController _playerController;

        private readonly BoardArrowController _boardArrowController;

        private readonly BattlegroundController _battlegroundController;

        private readonly RanksController _ranksController;

        private readonly GameObject _fightTargetingArrowPrefab;

        private readonly SpriteRenderer _pictureSprite;

        private readonly SpriteRenderer _frozenSprite;

        private readonly GameObject _shieldSprite;

        private readonly TextMeshPro _attackText;

        private readonly TextMeshPro _healthText;

        private readonly ParticleSystem _sleepingParticles;

        private readonly ParticleSystem _toxicPowerGlowParticles;

        private readonly GameObject _unitContentObject;

        private GameObject _battleframeObject;

        private GameObject _glowObj;

        private GameObject _glowSelectedObject;

        private GameObject _arrivalModelObject;

        private GameObject _arrivaVfxObject;

        private GameObject _distractObject;

        private Vector3 _initialScale = new Vector3(0.9f, 0.9f, 0.9f);

        private bool _ignoreArrivalEndEvents;

        private bool _arrivalDone;

        private BattleBoardArrow _fightTargetingArrow;

        private const string _orangeGlow = "Orange";

        private const string _greenGlow = "Green";

        public Action ArrivalEndCallback;

        public BoardUnitView(BoardUnitModel model, Transform parent)
        {
            Model = model;

            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _animationsController = _gameplayManager.GetController<AnimationsController>();
            _vfxController = _gameplayManager.GetController<VfxController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _inputController = _gameplayManager.GetController<InputController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _playerController = _gameplayManager.GetController<PlayerController>();
            _ranksController = _gameplayManager.GetController<RanksController>();

            GameObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/BoardCreature"));
            GameObject.transform.SetParent(parent, false);

            _fightTargetingArrowPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");

            _pictureSprite = GameObject.transform.Find("CreaturePicture").GetComponent<SpriteRenderer>();
            _frozenSprite = GameObject.transform.Find("Other/Frozen").GetComponent<SpriteRenderer>();
            _shieldSprite = GameObject.transform.Find("Other/Shield").gameObject;

            _distractObject = GameObject.transform.Find("Other/ZB_ANM_Distract").gameObject;

            _attackText = GameObject.transform.Find("Other/AttackAndDefence/AttackText").GetComponent<TextMeshPro>();
            _healthText = GameObject.transform.Find("Other/AttackAndDefence/DefenceText").GetComponent<TextMeshPro>();

            _sleepingParticles = GameObject.transform.Find("Other/SleepingParticles").GetComponent<ParticleSystem>();
            _toxicPowerGlowParticles = GameObject.transform.Find("Other/ToxicPowerGlowVFX").GetComponent<ParticleSystem>();

            _unitContentObject = GameObject.transform.Find("Other").gameObject;
            _unitContentObject.SetActive(false);

            _inputController.UnitSelectedEvent += UnitSelectedEventHandler;
            _inputController.UnitDeselectedEvent += UnitDeselectedEventHandler;
        }

        public BoardUnitModel Model { get; }

        public Transform Transform => GameObject.transform;

        public GameObject GameObject { get; }

        public Sprite Sprite => _pictureSprite.sprite;

        public void Update()
        {
            CheckOnDie();
        }

        public void SetObjectInfo(WorkingCard card)
        {
            Model.SetObjectInfo(card);

            string setName = _cardsController.GetSetOfCard(card.LibraryCard);
            string rank = Model.Card.LibraryCard.CardRank.ToString().ToLowerInvariant();
            string picture = Model.Card.LibraryCard.Picture.ToLowerInvariant();

            string fullPathToPicture = string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLowerInvariant(), rank, picture);

            _pictureSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(fullPathToPicture);

            _pictureSprite.transform.localPosition = (Vector3)Model.Card.LibraryCard.CardViewInfo.Position;
            _pictureSprite.transform.localScale = (Vector3)Model.Card.LibraryCard.CardViewInfo.Scale;

            _attackText.text = Model.CurrentDamage.ToString();
            _healthText.text = Model.CurrentHp.ToString();

            Model.UnitDamageChanged += ModelOnUnitDamageChanged;
            Model.UnitHpChanged += ModelOnUnitHpChanged;
            Model.UnitDying += BoardUnitOnUnitDying;
            Model.TurnStarted += BoardUnitOnTurnStarted;
            Model.TurnEnded += BoardUnitOnTurnEnded;
            Model.Stunned += BoardUnitOnStunned;
            Model.CardTypeChanged += BoardUnitOnCardTypeChanged;
            Model.BuffApplied += BoardUnitOnBuffApplied;
            Model.BuffShieldStateChanged += BoardUnitOnBuffShieldStateChanged;
            Model.CreaturePlayableForceSet += BoardUnitOnCreaturePlayableForceSet;
            Model.UnitFromDeckRemoved += BoardUnitOnUnitFromDeckRemoved;
            Model.UnitDistractEffectStateChanged += BoardUnitDistractEffectStateChanged;

            Model.FightSequenceHandler = this;

            switch (Model.InitialUnitType)
            {
                case Enumerators.CardType.FERAL:
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
                        Model.OwnerPlayer.IsLocalPlayer ? 2.9f : 1.7f);

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
                        Model.OwnerPlayer.IsLocalPlayer ? 2.7f : 1.7f);
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
                        Model.OwnerPlayer.IsLocalPlayer ? 1.3f : 0.3f);

                    break;
            }

            SetNormalGlowFromUnitType();
            SetAttackGlowFromUnitType();
            SetHighlightingEnabled(false);
        }

        private void ModelOnUnitHpChanged()
        {
            UpdateUnitInfoText(_healthText, Model.CurrentHp, Model.InitialHp, Model.MaxCurrentHp);
            CheckOnDie();
        }

        private void ModelOnUnitDamageChanged()
        {
            UpdateUnitInfoText(_attackText, Model.CurrentDamage, Model.InitialDamage, Model.MaxCurrentDamage);
        }

        private void BoardUnitOnUnitFromDeckRemoved()
        {
            Object.Destroy(GameObject);
        }

        private void BoardUnitOnCreaturePlayableForceSet()
        {
            SetHighlightingEnabled(true);
        }

        private void BoardUnitOnBuffShieldStateChanged(bool status)
        {
            _shieldSprite.SetActive(status);
        }

        private void BoardUnitDistractEffectStateChanged(bool status)
        {
            _distractObject.SetActive(status);
        }


        private void BoardUnitOnBuffApplied(Enumerators.BuffType type)
        {
            switch (type)
            {
                case Enumerators.BuffType.GUARD:
                    BoardUnitOnBuffShieldStateChanged(true);
                    break;
                case Enumerators.BuffType.DEFENCE:
                    break;
                case Enumerators.BuffType.HEAVY:
                    break;
                case Enumerators.BuffType.WEAPON:
                    break;
                case Enumerators.BuffType.RUSH:
                    _sleepingParticles.gameObject.SetActive(false);
                    if (Model.HasBuffRush && Model.InitialUnitType != Enumerators.CardType.FERAL)
                    {
                        SetNormalGlowFromUnitType();
                    }
                    break;
                case Enumerators.BuffType.ATTACK:
                    break;
                case Enumerators.BuffType.FREEZE:
                    break;
                case Enumerators.BuffType.DAMAGE:
                    break;
                case Enumerators.BuffType.HEAL_ALLY:
                    break;
                case Enumerators.BuffType.DESTROY:
                    break;
                case Enumerators.BuffType.REANIMATE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void BoardUnitOnCardTypeChanged(Enumerators.CardType type)
        {
            _shieldSprite.SetActive(Model.HasBuffShield);

            bool currentHighlight = GetHighlightingEnabled();
            switch (type)
            {
                case Enumerators.CardType.WALKER:
                    ChangeTypeFrame(1.3f, 0.3f);
                    break;
                case Enumerators.CardType.FERAL:
                    ChangeTypeFrame(2.7f, 1.7f);

                    if (!Model.AttackedThisTurn && !Model.IsPlayable)
                    {
                        StopSleepingParticles();
                        currentHighlight = true;
                    }
                    break;
                case Enumerators.CardType.HEAVY:
                    ChangeTypeFrame(2.5f, 1.7f);
                    if (!Model.AttackedThisTurn && Model.NumTurnsOnBoard == 0)
                    {
                        currentHighlight = false;
                    }
                    else if (!Model.AttackedThisTurn && Model.IsPlayable && !Model.CantAttackInThisTurnBlocker)
                    {
                        StopSleepingParticles();
                    }
                    break;
                case Enumerators.CardType.NONE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            SetNormalGlowFromUnitType();
            SetAttackGlowFromUnitType();
            SetHighlightingEnabled(currentHighlight);
        }

        private void BoardUnitOnStunned(bool isStun)
        {
            if (isStun)
            {
                _frozenSprite.DOFade(1, 1);
                SetHighlightingEnabled(false);
            }
            else
            {
                _frozenSprite.DOFade(0, 1);
            }

        }

        private void BoardUnitOnTurnStarted()
        {
            StopSleepingParticles();

            if (!Model.IsStun)
            {
                _frozenSprite.DOFade(0, 1);
            }

            if (Model.OwnerPlayer != null && Model.IsPlayable && _gameplayManager.CurrentTurnPlayer.Equals(Model.OwnerPlayer))
            {
                if (Model.CurrentDamage > 0)
                {
                    SetHighlightingEnabled(true);
                }
            }
        }

        private void BoardUnitOnTurnEnded()
        {
            CancelTargetingArrows();
            if (Model.HasBuffRush && Model.InitialUnitType != Enumerators.CardType.FERAL)
            {
                SetNormalGlowFromUnitType();
            }
        }

        private void BoardUnitOnUnitDying()
        {
            Model.UnitDamageChanged -= ModelOnUnitDamageChanged;
            Model.UnitHpChanged -= ModelOnUnitHpChanged;
            Model.UnitDying -= BoardUnitOnUnitDying;
            Model.TurnStarted -= BoardUnitOnTurnStarted;
            Model.TurnEnded -= BoardUnitOnTurnEnded;
            Model.Stunned -= BoardUnitOnStunned;
            Model.CardTypeChanged -= BoardUnitOnCardTypeChanged;
            Model.BuffApplied -= BoardUnitOnBuffApplied;
            Model.BuffShieldStateChanged -= BoardUnitOnBuffShieldStateChanged;
            Model.CreaturePlayableForceSet -= BoardUnitOnCreaturePlayableForceSet;
            Model.UnitFromDeckRemoved -= BoardUnitOnUnitFromDeckRemoved;
        }

        public void PlayArrivalAnimation(bool firstAppear = true)
        {
            GameObject arrivalPrefab =
          _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/" + Model.InitialUnitType + "_Arrival_VFX");
            _battleframeObject = Object.Instantiate(arrivalPrefab, GameObject.transform, false).gameObject;
            _arrivalModelObject = _battleframeObject.transform.Find("Main_Model").gameObject;
            _arrivaVfxObject = _battleframeObject.transform.Find("VFX_All").gameObject;
            Transform spriteContainerTransform =
                _battleframeObject.transform.Find("Main_Model/Root/FangMain/SpriteContainer");
            Vector3 scale = spriteContainerTransform.transform.localScale;
            scale.x *= -1;
            spriteContainerTransform.transform.localScale = scale;
            _pictureSprite.transform.SetParent(spriteContainerTransform, false);

            if (firstAppear)
            {
                GameObject.transform.position += Vector3.back * 5f;
            }

        }

        public void ArrivalAnimationEventHandler()
        {
            if (_unitContentObject == null || !_unitContentObject)
                return;

            _unitContentObject.SetActive(true);
            if (Model.HasFeral || Model.NumTurnsOnBoard > 0 && !Model.CantAttackInThisTurnBlocker)
            {
                StopSleepingParticles();
            }

            if (!_ignoreArrivalEndEvents)
            {
                if (Model.HasFeral)
                {
                    if (Model.OwnerPlayer != null)
                    {
                        SetHighlightingEnabled(true);
                    }
                }
            }

            if (!_ignoreArrivalEndEvents)
            {
                if (Model.Card.LibraryCard.CardRank == Enumerators.CardRank.COMMANDER)
                {
                    _soundManager.PlaySound(Enumerators.SoundType.CARDS,

                    Model.Card.LibraryCard.Name.ToLowerInvariant() + "_" + Constants.CardSoundPlay + "1",
                    Constants.ZombiesSoundVolume, false, true);
                    _soundManager.PlaySound(Enumerators.SoundType.CARDS,
                    Model.Card.LibraryCard.Name.ToLowerInvariant() + "_" + Constants.CardSoundPlay + "2",
                    Constants.ZombiesSoundVolume / 2f, false, true);
                }
                else
                {
                    _soundManager.PlaySound(Enumerators.SoundType.CARDS,

                    Model.Card.LibraryCard.Name.ToLowerInvariant() + "_" + Constants.CardSoundPlay, Constants.ZombiesSoundVolume,
                    false, true);
                }

                if (Model.Card.LibraryCard.Name.Equals("Freezzee"))
                {
                    List<BoardUnitView> freezzees = Model.GetEnemyUnitsList(Model)
                    .FindAll(x => x.Model.Card.LibraryCard.Id == Model.Card.LibraryCard.Id);

                    if (freezzees.Count > 0)
                    {
                        foreach (BoardUnitView creature in freezzees)
                        {
                            creature.Model.Stun(Enumerators.StunType.FREEZE, 1);
                            CreateFrozenVfx(creature.Transform.position);
                        }
                    }
                }
            }

            _initialScale = GameObject.transform.localScale;

            _ignoreArrivalEndEvents = false;

            _arrivalDone = true;

            ArrivalEndCallback?.Invoke();
            ArrivalEndCallback = null;
        }

        public void SetSelectedUnit(bool status)
        {
            if (_glowSelectedObject != null)
            {
                _glowSelectedObject.SetActive(status);
            }

            if (status)
            {
                GameObject.transform.localScale = _initialScale + Vector3.one * 0.1f;
            }
            else
            {
                GameObject.transform.localScale = _initialScale;
            }
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
            if (!Model.UnitCanBeUsable())
            {
                enabled = false;
            }

            if (_glowObj)
            {
                _glowObj.SetActive(enabled);
            }
        }

        public bool GetHighlightingEnabled () {
            if (_glowObj) 
                return _glowObj.activeSelf;

            return false;
        }

        public void StopSleepingParticles()
        {
            if (_sleepingParticles != null)
            {
                _sleepingParticles.Stop();
            }
        }

        public void EnabledToxicPowerGlow()
        {
            _toxicPowerGlowParticles.Play();
        }

        public void ChangeModelVisibility(bool state)
        {
            _unitContentObject.SetActive(state);
            _arrivalModelObject.SetActive(state);
            _arrivaVfxObject.SetActive(state);
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
                Model.OwnerPlayer.IsLocalPlayer ? playerTime : opponentTime);
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
            GameObject particleObj = (GameObject)param[0];
            Object.Destroy(particleObj);
        }

        private void CheckOnDie()
        {
            if (Model.CurrentHp <= 0 && !Model.IsDead)
            {
                if (Model.IsAllAbilitiesResolvedAtStart && _arrivalDone)
                {
                    Model.Die();
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

        private void UnitSelectedEventHandler(BoardUnitView unit)
        {
            if (_boardArrowController.IsBoardArrowNowInTheBattle || !_gameplayManager.CanDoDragActions)
                return;

            if (unit == this)
            {
                OnMouseDown();
            }
        }

        private void UnitDeselectedEventHandler(BoardUnitView unit)
        {
            if (unit == this)
            {
                OnMouseUp();
            }
        }

        private void OnMouseDown()
        {
            if (_tutorialManager.IsTutorial && !_tutorialManager.CurrentTutorialDataStep.UnitsCanAttack)
                return;

            if (!_arrivalDone)
                return;

            if (Model.OwnerPlayer != null && Model.OwnerPlayer.IsLocalPlayer && _playerController.IsActive && Model.UnitCanBeUsable())
            {
                _fightTargetingArrow = _boardArrowController.BeginTargetingArrowFrom<BattleBoardArrow>(Transform);
                _fightTargetingArrow.TargetsType = new List<Enumerators.SkillTargetType>
                {
                    Enumerators.SkillTargetType.OPPONENT,
                    Enumerators.SkillTargetType.OPPONENT_CARD
                };
                _fightTargetingArrow.BoardCards = _gameplayManager.OpponentPlayer.BoardCards;
                _fightTargetingArrow.Owner = this;

                if (Model.AttackInfoType == Enumerators.AttackInfoType.ONLY_DIFFERENT)
                {
                    _fightTargetingArrow.IgnoreBoardObjectsList = Model.AttackedBoardObjectsThisTurn;
                }

                if (Model.OwnerPlayer.Equals(_gameplayManager.CurrentPlayer))
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
                    Model.Card.LibraryCard.Name.ToLowerInvariant() + "_" + Constants.CardSoundAttack, Constants.ZombiesSoundVolume,
                    false, true);
            }
        }

        private void OnMouseUp()
        {
            if (_fightTargetingArrow != null)
            {
                if (Model.OwnerPlayer != null && Model.OwnerPlayer.IsLocalPlayer && _playerController.IsActive && Model.UnitCanBeUsable())
                {
                    _fightTargetingArrow.End(this);

                    if (Model.OwnerPlayer.Equals(_gameplayManager.CurrentPlayer))
                    {
                        _playerController.IsCardSelected = false;
                    }
                }
                else
                {
                    _fightTargetingArrow.Dispose();
                    _fightTargetingArrow = null;
                }
            }
        }

        private void CheckIsCanDie(object[] param)
        {
            if (_arrivalDone)
            {
                _timerManager.StopTimer(CheckIsCanDie);

                Model.RemoveUnitFromBoard();
            }
        }

        [Serializable]
        public class UnitAnimatorInfo
        {
            public Enumerators.CardType CardType;

            public RuntimeAnimatorController Animator;
        }

        public void HandleAttackPlayer(Action completeCallback, Player targetPlayer, Action hitCallback, Action attackCompleteCallback)
        {
            _animationsController.DoFightAnimation(
                GameObject,
                targetPlayer.AvatarObject,
                0.1f,
                () =>
                {
                    Vector3 positionOfVfx = targetPlayer.AvatarObject.transform.position;
                    _vfxController.PlayAttackVfx(Model.Card.LibraryCard.CardType,
                        positionOfVfx,
                        Model.CurrentDamage);

                    hitCallback();

                    _fightTargetingArrow = null;
                    SetHighlightingEnabled(true);
                },
                () =>
                {
                    attackCompleteCallback();

                    completeCallback?.Invoke();
                }
                );
        }

        public void HandleAttackCard(Action completeCallback, BoardUnitModel targetCard, Action hitCallback, Action attackCompleteCallback)
        {
            BoardUnitView targetCardView = _battlegroundController.GetBoardUnitViewByModel(targetCard);
            _animationsController.DoFightAnimation(
                GameObject,
                targetCardView.Transform.gameObject,
                0.5f,
                () =>
                {
                    _vfxController.PlayAttackVfx(Model.Card.LibraryCard.CardType,
                        targetCardView.Transform.position, Model.CurrentDamage);

                    hitCallback();

                    _fightTargetingArrow = null;
                    SetHighlightingEnabled(true);
                },
                () =>
                {
                    attackCompleteCallback();

                    if (targetCardView.Model.CurrentHp <= 0)
                    {
                        targetCardView.Model.UnitDied += () =>
                        {
                            completeCallback?.Invoke();
                        };
                    }
                    else if(Model.CurrentHp <= 0)
                    {
                        Model.UnitDied += () =>
                        {
                            completeCallback?.Invoke();
                        };
                    }
                    else
                    {
                        completeCallback?.Invoke();
                    }
                }
            );
        }

        private void SetNormalGlowFromUnitType()
        {
            string color = Model.HasBuffRush ? _orangeGlow : _greenGlow;
            bool active = false;
            if (_glowObj != null)
            {
                active = Model.HasBuffRush ? true : _glowObj.activeInHierarchy;
                Object.Destroy(_glowObj);
            }
            string direction = "Prefabs/Gameplay/ActiveFramesCards/ZB_ANM_" + Model.InitialUnitType + "_ActiveFrame_" + color;
            _glowObj = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>(direction), _unitContentObject.transform, false);

            SetHighlightingEnabled(active);
        }

        private void SetAttackGlowFromUnitType()
        {
            if (_glowSelectedObject != null)
            {
                Object.Destroy(_glowSelectedObject);
            }
            string direction = "Prefabs/Gameplay/ActiveFramesCards/ZB_ANM_" + Model.InitialUnitType + "_ActiveFrame_Red";
            _glowSelectedObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>(direction), _unitContentObject.transform, false);
            _glowSelectedObject.SetActive(false);
        }
    }
}
