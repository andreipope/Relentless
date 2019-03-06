using System;
using System.Collections.Generic;
using DG.Tweening;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.View;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using ZombieBattleground.Editor.Runtime;
#endif

namespace Loom.ZombieBattleground
{
    public class BoardUnitView : IFightSequenceHandler, IView
    {
        private static readonly ILog Log = Logging.GetLog(nameof(BoardUnitView));

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

        private readonly UniqueAnimationsController _uniqueAnimationsController;

        private readonly ActionsQueueController _actionsQueueController;

        private readonly GameObject _fightTargetingArrowPrefab;

        private readonly SpriteRenderer _pictureSprite;

        private readonly SpriteRenderer _frozenSprite;

        private readonly GameObject _shieldSprite;

        private readonly TextMeshPro _attackText;

        private readonly TextMeshPro _healthText;

        private readonly ParticleSystem _sleepingParticles;

        private readonly GameObject _unitContentObject;

        private GameObject _battleframeObject;

        private GameObject _glowObj;

        private GameObject _glowSelectedObject;

        private GameObject _arrivalModelObject;

        private GameObject _arrivaVfxObject;

        private GameObject _distractObject;

        private SpriteRenderer _cardMechanicsPicture;

        private Vector3 _initialScale = new Vector3(0.9f, 0.9f, 0.9f);

        private bool _ignoreArrivalEndEvents;

        private bool _arrivalDone;

        private BattleBoardArrow _fightTargetingArrow;

        private const string _orangeGlow = "Orange";

        private const string _greenGlow = "Green";

        private int _currentEffectIndexCrossfading = 0;

        private bool _crossfadingEffectsOnUnit = false;

        private const float _effectsOnUnitFadeDuration = 0.5f;

        private const float _effectsOnUnitFadeCrossfadingDelay = 3f;

        private bool _crossfadingSequenceEnded = true;

        private List<Enumerators.GameMechanicDescriptionType> _filteredEffectsToShow;

        public Action ArrivalEndCallback;

        public Vector3 PositionOfBoard { get; set; }

        public Animator battleframeAnimator { get; private set; }

        public BattleBoardArrow FightTargetingArrow => _fightTargetingArrow;

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
            _uniqueAnimationsController = _gameplayManager.GetController<UniqueAnimationsController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();

            GameObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/BoardCreature"));
            GameObject.transform.SetParent(parent, false);

            _fightTargetingArrowPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");

            _pictureSprite = GameObject.transform.Find("CreaturePicture").GetComponent<SpriteRenderer>();
            _frozenSprite = GameObject.transform.Find("Other/Frozen").GetComponent<SpriteRenderer>();
            _shieldSprite = GameObject.transform.Find("Other/Shield").gameObject;
            _cardMechanicsPicture = GameObject.transform.Find("Other/Picture_CardMechanics").GetComponent<SpriteRenderer>();

            _distractObject = GameObject.transform.Find("Other/ZB_ANM_Distract").gameObject;

            _attackText = GameObject.transform.Find("Other/AttackAndDefence/AttackText").GetComponent<TextMeshPro>();
            _healthText = GameObject.transform.Find("Other/AttackAndDefence/DefenceText").GetComponent<TextMeshPro>();

            _sleepingParticles = GameObject.transform.Find("Other/SleepingParticles").GetComponent<ParticleSystem>();

            _unitContentObject = GameObject.transform.Find("Other").gameObject;
            _unitContentObject.SetActive(false);

            _inputController.UnitSelectedEvent += UnitSelectedEventHandler;
            _inputController.UnitDeselectedEvent += UnitDeselectedEventHandler;

            SetObjectInfo();

#if UNITY_EDITOR
            MainApp.Instance.OnDrawGizmosCalled += OnDrawGizmos;
#endif
        }

        public BoardUnitModel Model { get; }

        public Transform Transform => GameObject?.transform;

        public GameObject GameObject { get; private set; }

        public bool WasDestroyed { get; set; }

        public Sprite Sprite => _pictureSprite.sprite;

        public bool ArrivalDone => _arrivalDone;

        public void Update()
        {
            CheckOnDie();
        }

        public void DisposeGameObject()
        {
            Log.Info($"GameObject of BoardUnitView was disposed");

            Transform.DOKill();
            Object.Destroy(GameObject);
        }

        public void ForceSetGameObject(GameObject overrideObject)
        {
            Log.Info($"GameObject of BoardUnitView was overrided. from: {GameObject} on: {overrideObject}");

            GameObject = overrideObject;
        }

        private void SetObjectInfo()
        {
            Model.GameMechanicDescriptionsOnUnitChanged += BoardUnitGameMechanicDescriptionsOnUnitChanged;

            Enumerators.SetType setType = _cardsController.GetSetOfCard(Model.Card.Prototype);
            string rank = Model.Card.Prototype.CardRank.ToString().ToLowerInvariant();

            _pictureSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>($"Images/Cards/Illustrations/{Model.Card.Prototype.Picture.ToLowerInvariant()}");

            _pictureSprite.transform.localPosition = (Vector3)Model.Card.Prototype.CardViewInfo.Position;
            _pictureSprite.transform.localScale = (Vector3)Model.Card.Prototype.CardViewInfo.Scale;

            _attackText.text = Model.CurrentDamage.ToString();
            _healthText.text = Model.CurrentHp.ToString();

            Model.UnitDamageChanged += ModelOnUnitDamageChanged;
            Model.UnitHpChanged += ModelOnUnitHpChanged;
            Model.UnitDying += BoardUnitOnUnitDying;
            Model.UnitDied += BoardUnitOnUnitDied;
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

            if (!_uniqueAnimationsController.HasUniqueAnimation(Model.Card))
            {
                switch (Model.InitialUnitType)
                {
                    case Enumerators.CardType.FERAL:
                        InternalTools.DoActionDelayed(() =>
                        {
                            _soundManager.PlaySound(Enumerators.SoundType.FERAL_ARRIVAL, Constants.ArrivalSoundVolume,
                                    false, false, true);
                        }, 0.55f);

                        InternalTools.DoActionDelayed(ArrivalAnimationEventHandler, Model.OwnerPlayer.IsLocalPlayer ? 2.9f : 1.7f);

                        break;
                    case Enumerators.CardType.HEAVY:
                        InternalTools.DoActionDelayed(() =>
                        {
                            _soundManager.PlaySound(Enumerators.SoundType.HEAVY_ARRIVAL, Constants.ArrivalSoundVolume,
                                    false, false, true);
                        }, 1f);

                        InternalTools.DoActionDelayed(ArrivalAnimationEventHandler, Model.OwnerPlayer.IsLocalPlayer ? 2.9f : 1.7f);

                        break;
                    case Enumerators.CardType.WALKER:
                    default:
                        InternalTools.DoActionDelayed(() =>
                        {
                            _soundManager.PlaySound(Enumerators.SoundType.WALKER_ARRIVAL, Constants.ArrivalSoundVolume,
                                    false, false, true);
                        }, .6f);

                        InternalTools.DoActionDelayed(ArrivalAnimationEventHandler, Model.OwnerPlayer.IsLocalPlayer ? 1.3f : 0.3f);
                        break;
                }
            }

            SetNormalGlowFromUnitType();
            SetAttackGlowFromUnitType();
            SetHighlightingEnabled(false);

            if(Model.Card.Owner.IsLocalPlayer)
            {
                PositionOfBoard = _battlegroundController.PlayerBoardObject.transform.position - Vector3.up * 1.7f;
            }
            else
            {
                PositionOfBoard = _battlegroundController.OpponentBoardObject.transform.position;
            }

        }
        private void ModelOnUnitHpChanged(int oldValue, int newValue)
        {
            UpdateUnitInfoText(_healthText, Model.CurrentHp, Model.Card.Prototype.Health, Model.MaxCurrentHp);
            CheckOnDie();
        }

        private void ModelOnUnitDamageChanged(int oldValue, int newValue)
        {
            UpdateUnitInfoText(_attackText, Model.CurrentDamage, Model.Card.Prototype.Damage, Model.MaxCurrentDamage);
            if(Model.MaxCurrentDamage == 0 && Model.UnitCanBeUsable())
            {
                SetNormalGlowFromUnitType();
            }
        }

        private void BoardUnitOnUnitFromDeckRemoved()
        {
            DisposeGameObject();
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

            if (status)
            {
                _soundManager.PlaySound(Enumerators.SoundType.DISTRACT_LOOP, Constants.SfxSoundVolume);
            }
            else
            {
                _soundManager.StopPlaying(Enumerators.SoundType.DISTRACT_LOOP);
            }
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
                case Enumerators.BuffType.BLITZ:
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
            SetNormalGlowFromUnitType();
        }

        private void BoardUnitGameMechanicDescriptionsOnUnitChanged()
        {
            _filteredEffectsToShow = Model.GameMechanicDescriptionsOnUnit.FindAll(effect =>
                                                                 effect == Enumerators.GameMechanicDescriptionType.Death ||
                                                                 effect == Enumerators.GameMechanicDescriptionType.Freeze ||
                                                                 effect == Enumerators.GameMechanicDescriptionType.Destroy ||
                                                                 effect == Enumerators.GameMechanicDescriptionType.Reanimate);

            if (_filteredEffectsToShow.Count == 0)
            {
                if (_cardMechanicsPicture.sprite != null)
                {
                    Sequence sequence = DOTween.Sequence();
                    sequence.Append(_cardMechanicsPicture.DOFade(0f, _effectsOnUnitFadeDuration));
                    sequence.AppendCallback(() =>
                    {
                        _cardMechanicsPicture.sprite = null;
                    });
                    sequence.Play();
                }
            }
            else
            {
                DrawCardMechanicIcons();
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
            Model.UnitDistractEffectStateChanged -= BoardUnitDistractEffectStateChanged;
            Model.GameMechanicDescriptionsOnUnitChanged -= BoardUnitGameMechanicDescriptionsOnUnitChanged;
        }

        private void BoardUnitOnUnitDied()
        {
            Model.UnitDied -= BoardUnitOnUnitDied;
            _soundManager.StopPlaying(Enumerators.SoundType.DISTRACT_LOOP);
        }

        public void PlayArrivalAnimation(bool firstAppear = true, bool playUniqueAnimation = false)
        {
            Action generalArrivalAnimationAction = () =>
            {
                GameObject arrivalPrefab =
              _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/" + Model.InitialUnitType + "_Arrival_VFX");
                _battleframeObject = Object.Instantiate(arrivalPrefab, GameObject.transform, false).gameObject;
                battleframeAnimator = _battleframeObject.GetComponent<Animator>();
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

                float delay = 0f;

                switch (Model.InitialUnitType)
                {
                    case Enumerators.CardType.FERAL:
                    case Enumerators.CardType.HEAVY:
                        delay = Model.OwnerPlayer.IsLocalPlayer ? 2.9f : 1.7f;
                        break;
                    case Enumerators.CardType.WALKER:
                    default:
                        delay = Model.OwnerPlayer.IsLocalPlayer ? 1.3f : 0.3f;
                        break;
                }

                if (_uniqueAnimationsController.HasUniqueAnimation(Model.Card) && (!playUniqueAnimation || !firstAppear))
                {
                    InternalTools.DoActionDelayed(ArrivalAnimationEventHandler, delay);
                }
            };

            if (firstAppear && _uniqueAnimationsController.HasUniqueAnimation(Model.Card) && playUniqueAnimation)
            {
                _uniqueAnimationsController.PlayUniqueArrivalAnimation(Model, Model.Card, startGeneralArrivalCallback: generalArrivalAnimationAction, endArrivalCallback: ArrivalAnimationEventHandler);
            }
            else
            {
                generalArrivalAnimationAction.Invoke();
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
                if (Model.Card.Prototype.CardRank == Enumerators.CardRank.COMMANDER)
                {
                    _soundManager.PlaySound(Enumerators.SoundType.CARDS,

                    Model.Card.Prototype.Name.ToLowerInvariant() + "_" + Constants.CardSoundPlay + "1",
                    Constants.ZombiesSoundVolume, false, true);
                    _soundManager.PlaySound(Enumerators.SoundType.CARDS,
                    Model.Card.Prototype.Name.ToLowerInvariant() + "_" + Constants.CardSoundPlay + "2",
                    Constants.ZombiesSoundVolume / 2f, false, true);
                }
                else
                {
                    _soundManager.PlaySound(Enumerators.SoundType.CARDS,

                    Model.Card.Prototype.Name.ToLowerInvariant() + "_" + Constants.CardSoundPlay, Constants.ZombiesSoundVolume,
                    false, true);
                }


                // FIXME: WTF we have logic based on card name?
                if (Model.Card.Prototype.Name.Equals("Freezzee"))
                {
                    IReadOnlyList<BoardUnitView> freezzees =
                        Model
                            .GetEnemyUnitsList(Model)
                            .FindAll(x => x.Model.Card.Prototype.MouldId == Model.Card.Prototype.MouldId);

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

        public bool GetHighlightingEnabled()
        {
            if (_glowObj)
                return _glowObj.activeSelf;

            return false;
        }

        public void StopSleepingParticles()
        {
            if (_sleepingParticles != null)
            {
                _sleepingParticles.Stop();
                _sleepingParticles.gameObject.SetActive(false);
            }
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

        public void StartAttackTargeting()
        {
            if (_tutorialManager.IsTutorial && !_tutorialManager.CurrentTutorialStep.ToGameplayStep().UnitsCanAttack)
                return;

            if(_tutorialManager.IsTutorial && _tutorialManager.CurrentTutorialStep != null &&
                _tutorialManager.CurrentTutorialStep.ToGameplayStep().TutorialObjectIdStepOwner != 0 &&
                _tutorialManager.CurrentTutorialStep.ToGameplayStep().TutorialObjectIdStepOwner != Model.TutorialObjectId &&
                Model.OwnerPlayer.IsLocalPlayer)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordTriedToUseWrongBattleframe);
                return;
            }

            if (!_arrivalDone)
                return;

            if (Model.OwnerPlayer != null && Model.OwnerPlayer.IsLocalPlayer && _playerController.IsActive && Model.UnitCanBeUsable())
            {
                _fightTargetingArrow = _boardArrowController.BeginTargetingArrowFrom<BattleBoardArrow>(Transform);
                _fightTargetingArrow.TargetsType = Model.AttackTargetsAvailability;
                _fightTargetingArrow.BoardCards = _gameplayManager.OpponentPlayer.BoardCards;
                _fightTargetingArrow.Owner = this;

                if (Model.AttackRestriction == Enumerators.AttackRestriction.ONLY_DIFFERENT)
                {
                    _fightTargetingArrow.IgnoreBoardObjectsList = Model.AttackedBoardObjectsThisTurn;
                }

                if (Model.OwnerPlayer.Equals(_gameplayManager.CurrentPlayer))
                {
                    _battlegroundController.DestroyCardPreview();
                    _playerController.IsCardSelected = true;

                    if(_tutorialManager.IsTutorial)
                    {
                        _tutorialManager.DeactivateSelectHandPointer(Enumerators.TutorialObjectOwner.PlayerBattleframe);
                    }
                }

                _soundManager.StopPlaying(Enumerators.SoundType.CARDS);
                _soundManager.PlaySound(Enumerators.SoundType.CARDS,
                    Model.Card.Prototype.Name.ToLowerInvariant() + "_" + Constants.CardSoundAttack, Constants.ZombiesSoundVolume,
                    false, true);
            }
        }

        public void FinishAttackTargeting()
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

        private void OnMouseDown()
        {
            StartAttackTargeting();
        }

        private void OnMouseUp()
        {
            FinishAttackTargeting();
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
                    _vfxController.PlayAttackVfx(Model, positionOfVfx);

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

            if(targetCardView == null || targetCardView.GameObject == null)
            {
                Model.ActionForDying = null;
                targetCard.ActionForDying = null;
                completeCallback?.Invoke();

                ExceptionReporter.LogException(Log, new Exception("target card is NULL. cancel ATTACK! targetCardView: " + targetCardView +
                    " | targetCardView.GameObject: " + targetCardView?.GameObject));
                return;
            }

            _animationsController.DoFightAnimation(
                GameObject,
                targetCardView.GameObject,
                0.5f,
                () =>
                {
                    _vfxController.PlayAttackVfx(Model,
                        targetCardView.Transform.position);

                    hitCallback();

                    _fightTargetingArrow = null;
                    SetHighlightingEnabled(true);
                },
                () =>
                {
                    attackCompleteCallback();

                    if (Model.CurrentHp > 0)
                    {
                        _actionsQueueController.ForceContinueAction(Model.ActionForDying);
                        Model.ActionForDying = null;
                    }

                    if (targetCard.CurrentHp > 0)
                    {
                        _actionsQueueController.ForceContinueAction(targetCard.ActionForDying);
                        targetCard.ActionForDying = null;
                    }

                    completeCallback?.Invoke();
                }
            );
        }

        private void SetNormalGlowFromUnitType()
        {
            string color = Model.HasBuffRush ? _orangeGlow : _greenGlow;
            bool active = Model.UnitCanBeUsable();
            if (_glowObj != null)
            {
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

        private void DrawCardMechanicIcons()
        {
            if (_filteredEffectsToShow.Count == 1)
            {
                _currentEffectIndexCrossfading = 0;
                _crossfadingEffectsOnUnit = false;
            }
            else
            {
                if (!_crossfadingEffectsOnUnit)
                {
                    _crossfadingEffectsOnUnit = true;
                }
            }

            ChangeCardMechanicIcon(_filteredEffectsToShow[_currentEffectIndexCrossfading].ToString().ToLowerInvariant());

            if (_filteredEffectsToShow.Count > 1)
            {
                _currentEffectIndexCrossfading++;

                if (_currentEffectIndexCrossfading >= _filteredEffectsToShow.Count)
                {
                    _currentEffectIndexCrossfading = 0;
                }
            }
        }

        private void ChangeCardMechanicIcon(string icon)
        {
            if (!_crossfadingSequenceEnded)
                return;

            _crossfadingSequenceEnded = false;
            string iconPath = "Images/BattlegroundIconsCardMechanics/battleground_mechanic_icon_";

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_cardMechanicsPicture.DOFade(0f, _effectsOnUnitFadeDuration));
            sequence.AppendCallback(() =>
            {
                Sprite sprite = _loadObjectsManager.GetObjectByPath<Sprite>(iconPath + icon);

                if (sprite == null)
                {
                    sprite = _loadObjectsManager.GetObjectByPath<Sprite>(iconPath + "blank");
                }

                _cardMechanicsPicture.sprite = sprite;
            });
            sequence.Append(_cardMechanicsPicture.DOFade(1f, _effectsOnUnitFadeDuration));
            sequence.AppendCallback(() =>
            {
                if (_crossfadingEffectsOnUnit)
                {
                    InternalTools.DoActionDelayed(DrawCardMechanicIcons, _effectsOnUnitFadeCrossfadingDelay);
                }

                _crossfadingSequenceEnded = true;
            });

            sequence.Play();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (GameObject == null)
            {
                MainApp.Instance.OnDrawGizmosCalled -= OnDrawGizmos;
                return;
            }

            if (Model.Card == null)
                return;

            DebugCardInfoDrawer.Draw(GameObject.transform.position, Model.Card.InstanceId.Id, Model.Card.Prototype.Name);
        }
#endif
    }
}
