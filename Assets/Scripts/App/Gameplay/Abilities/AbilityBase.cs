using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class AbilityBase
    {
        protected event Action PermanentInputEndEvent;

        private readonly Player playerAvatar;

        private readonly Player opponenentAvatar;

        public ulong activityId;

        public Enumerators.AbilityActivityType abilityActivityType;

        public Enumerators.AbilityCallType abilityCallType;

        public Enumerators.AbilityType abilityType;

        public Enumerators.AffectObjectType affectObjectType;

        public Enumerators.AbilityEffectType abilityEffectType;

        public Enumerators.CardType targetCardType = Enumerators.CardType.NONE;

        public Enumerators.UnitStatusType targetUnitStatusType = Enumerators.UnitStatusType.NONE;

        public List<Enumerators.AbilityTargetType> abilityTargetTypes;

        public Enumerators.CardKind cardKind;

        public Card cardOwnerOfAbility;

        public WorkingCard mainWorkingCard;

        public BoardUnit abilityUnitOwner;

        public Player playerCallerOfAbility;

        public BoardSpell boardSpell;

        public BoardCard boardCard;

        public BoardUnit targetUnit;

        public Player targetPlayer;

        public Player selectedPlayer;

        protected AbilitiesController _abilitiesController;

        protected ParticlesController _particlesController;

        protected BattleController _battleController;

        protected ActionsQueueController _actionsQueueController;

        protected BattlegroundController _battlegroundController;

        protected CardsController _cardsController;

        protected RanksController _ranksController;

        protected ILoadObjectsManager _loadObjectsManager;

        protected IGameplayManager _gameplayManager;

        protected IDataManager _dataManager;

        protected ITimerManager _timerManager;

        protected ISoundManager _soundManager;

        protected AbilityBoardArrow _targettingArrow;

        protected GameObject _vfxObject;

        protected bool _isAbilityResolved;

        protected Action OnObjectSelectedByTargettingArrowCallback;

        protected Action OnObjectSelectFailedByTargettingArrowCallback;

        protected AbilityData abilityData;

        protected List<ulong> _particleIds;

        public AbilityBase(Enumerators.CardKind cardKind, AbilityData ability)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _soundManager = GameClient.Get<ISoundManager>();

            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _particlesController = _gameplayManager.GetController<ParticlesController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _ranksController = _gameplayManager.GetController<RanksController>();

            abilityData = ability;
            this.cardKind = cardKind;
            abilityType = ability.abilityType;
            abilityActivityType = ability.abilityActivityType;
            abilityCallType = ability.abilityCallType;
            abilityTargetTypes = ability.abilityTargetTypes;
            abilityEffectType = ability.abilityEffectType;
            playerAvatar = _gameplayManager.CurrentPlayer;
            opponenentAvatar = _gameplayManager.OpponentPlayer;

            PermanentInputEndEvent += OnInputEndEventHandler;

            _particleIds = new List<ulong>();
        }

        public AbilityBoardArrow TargettingArrow => _targettingArrow;

        public AbilityData AbilityData => abilityData;

        public void ActivateSelectTarget(List<Enumerators.SkillTargetType> targetsType = null, Action callback = null, Action failedCallback = null)
        {
            OnObjectSelectedByTargettingArrowCallback = callback;
            OnObjectSelectFailedByTargettingArrowCallback = failedCallback;

            _targettingArrow = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object")).AddComponent<AbilityBoardArrow>();
            _targettingArrow.possibleTargets = abilityTargetTypes;
            _targettingArrow.selfBoardCreature = abilityUnitOwner;
            _targettingArrow.targetUnitType = targetCardType;
            _targettingArrow.targetUnitStatusType = targetUnitStatusType;

            if (cardKind == Enumerators.CardKind.CREATURE)
            {
                _targettingArrow.Begin(abilityUnitOwner.transform.position);
            } else if (cardKind == Enumerators.CardKind.SPELL)
            {
                _targettingArrow.Begin(selectedPlayer.AvatarObject.transform.position); // (boardSpell.transform.position);
            } else
            {
                _targettingArrow.Begin(playerCallerOfAbility.AvatarObject.transform.position);
            }

            _targettingArrow.OnCardSelectedEvent += OnCardSelectedEventHandler;
            _targettingArrow.OnCardUnselectedevent += OnCardUnselectedeventHandler;
            _targettingArrow.OnPlayerSelectedEvent += OnPlayerSelectedHandler;
            _targettingArrow.OnPlayerUnselectedEvent += OnPlayerUnselectedHandler;
            _targettingArrow.OnInputEndEvent += OnInputEndEventHandler;
            _targettingArrow.OnInputCancelEvent += OnInputCancelEventHandler;
        }

        public void DeactivateSelectTarget()
        {
            if (_targettingArrow != null)
            {
                _targettingArrow.OnCardSelectedEvent -= OnCardSelectedEventHandler;
                _targettingArrow.OnCardUnselectedevent -= OnCardUnselectedeventHandler;
                _targettingArrow.OnPlayerSelectedEvent -= OnPlayerSelectedHandler;
                _targettingArrow.OnPlayerUnselectedEvent -= OnPlayerUnselectedHandler;
                _targettingArrow.OnInputEndEvent -= OnInputEndEventHandler;
                _targettingArrow.OnInputCancelEvent -= OnInputCancelEventHandler;

                _targettingArrow.Dispose();
                _targettingArrow = null;
            }
        }

        public AbilityBase Clone()
        {
            return (AbilityBase)MemberwiseClone();
        }

        public virtual void Activate()
        {
            playerCallerOfAbility.OnEndTurnEvent += OnEndTurnEventHandler;
            playerCallerOfAbility.OnStartTurnEvent += OnStartTurnEventHandler;

            if ((cardKind == Enumerators.CardKind.CREATURE) && (abilityUnitOwner != null))
            {
                abilityUnitOwner.UnitOnDieEvent += UnitOnDieEventHandler;
                abilityUnitOwner.UnitOnAttackEvent += UnitOnAttackEventHandler;
                abilityUnitOwner.UnitHPChangedEvent += UnitHPChangedEventHandler;
                abilityUnitOwner.UnitGotDamageEvent += UnitGotDamageEventHandler;

                if (abilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
                {
                    // boardCreature.Card.ConnectAbility((uint)abilityType);
                }
            } else if ((cardKind == Enumerators.CardKind.SPELL) && (boardSpell != null))
            {
                boardSpell.SpellOnUsedEvent += SpellOnUsedEventHandler;
            }

            if (playerCallerOfAbility.IsLocalPlayer)
            {
                selectedPlayer = playerAvatar;
            } else
            {
                selectedPlayer = opponenentAvatar;
            }
        }

        public virtual void Update()
        {
        }

        public virtual void Dispose()
        {
            playerCallerOfAbility.OnEndTurnEvent -= OnEndTurnEventHandler;
            playerCallerOfAbility.OnStartTurnEvent -= OnStartTurnEventHandler;

            DeactivateSelectTarget();
            ClearParticles();
        }

        public virtual void SelectedTargetAction(bool callInputEndBefore = false)
        {
            if (callInputEndBefore)
            {
                PermanentInputEndEvent?.Invoke();
                return;
            }

            if (targetUnit != null)
            {
                affectObjectType = Enumerators.AffectObjectType.CHARACTER;
            } else if (targetPlayer != null)
            {
                affectObjectType = Enumerators.AffectObjectType.PLAYER;
            } else
            {
                affectObjectType = Enumerators.AffectObjectType.NONE;
            }

            if (affectObjectType != Enumerators.AffectObjectType.NONE)
            {
                _isAbilityResolved = true;

                if (affectObjectType == Enumerators.AffectObjectType.CHARACTER)
                {
                    // targetCreature.Card.ConnectAbility((uint)abilityType);
                }

                OnObjectSelectedByTargettingArrowCallback?.Invoke();
                OnObjectSelectedByTargettingArrowCallback = null;
            } else
            {
                OnObjectSelectFailedByTargettingArrowCallback?.Invoke();
                OnObjectSelectFailedByTargettingArrowCallback = null;
            }
        }

        public virtual void Action(object info = null)
        {
        }

        protected virtual void OnCardSelectedEventHandler(BoardUnit obj)
        {
            targetUnit = obj;

            targetPlayer = null;
        }

        protected virtual void OnCardUnselectedeventHandler(BoardUnit obj)
        {
            targetUnit = null;
        }

        protected virtual void OnPlayerSelectedHandler(Player obj)
        {
            targetPlayer = obj;

            targetUnit = null;
        }

        protected virtual void OnPlayerUnselectedHandler(Player obj)
        {
            targetPlayer = null;
        }

        protected virtual void CreateVFX(Vector3 pos, bool autoDestroy = false, float duration = 3f, bool justPosition = false)
        {
            // todo make it async
            if (_vfxObject != null)
            {
                _vfxObject = Object.Instantiate(_vfxObject);

                if (!justPosition)
                {
                    _vfxObject.transform.position = (pos - Constants.VFX_OFFSET) + Vector3.forward;
                } else
                {
                    _vfxObject.transform.position = pos;
                }

                ulong id = _particlesController.RegisterParticleSystem(_vfxObject, autoDestroy, duration);

                if (!autoDestroy)
                {
                    _particleIds.Add(id);
                }
            }
        }

        protected virtual void OnInputEndEventHandler()
        {
            SelectedTargetAction();
            DeactivateSelectTarget();
        }

        protected virtual void OnInputCancelEventHandler()
        {
            OnObjectSelectFailedByTargettingArrowCallback?.Invoke();
            OnObjectSelectFailedByTargettingArrowCallback = null;

            DeactivateSelectTarget();
        }

        protected virtual void OnEndTurnEventHandler()
        {
            if (_targettingArrow != null)
            {
                OnInputEndEventHandler();
            }
        }

        protected virtual void OnStartTurnEventHandler()
        {
        }

        protected virtual void UnitOnDieEventHandler()
        {
            // if(targetCreature != null)
            // targetCreature.Card.DisconnectAbility((uint)abilityType);
            abilityUnitOwner.UnitOnDieEvent -= UnitOnDieEventHandler;
            abilityUnitOwner.UnitHPChangedEvent -= UnitHPChangedEventHandler;
            abilityUnitOwner.UnitGotDamageEvent -= UnitGotDamageEventHandler;

            _abilitiesController.DeactivateAbility(activityId);
            Dispose();
        }

        protected virtual void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
        }

        protected virtual void UnitHPChangedEventHandler()
        {
        }

        protected virtual void UnitGotDamageEventHandler(object from)
        {
        }

        protected void SpellOnUsedEventHandler()
        {
            boardSpell.SpellOnUsedEvent -= SpellOnUsedEventHandler;

            // _abilitiesController.DeactivateAbility(activityId);
        }

        protected void DestroyCurrentParticle(bool isDirectly = false, float time = 3f)
        {
            if (isDirectly)
            {
                DestroyParticle(null);
            } else
            {
                GameClient.Get<ITimerManager>().AddTimer(DestroyParticle, null, time, false);
            }
        }

        protected void ClearParticles()
        {
            foreach (ulong id in _particleIds)
            {
                _particlesController.DestoryParticle(id);
            }
        }

        protected object GetCaller()
        {
            return abilityUnitOwner != null?abilityUnitOwner:(object)boardSpell;
        }

        protected Player GetOpponentOverlord()
        {
            return playerCallerOfAbility.Equals(_gameplayManager.CurrentPlayer)?_gameplayManager.OpponentPlayer:_gameplayManager.CurrentPlayer;
        }

        private void DestroyParticle(object[] param)
        {
            Object.Destroy(_vfxObject);
        }
    }
}
