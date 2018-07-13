// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class AbilityBase
    {
        protected event Action PermanentInputEndEvent;

        protected AbilitiesController _abilitiesController;
        protected ParticlesController _particlesController;
        protected BattleController _battleController;
        protected ActionsQueueController _actionsQueueController;
        protected BattlegroundController _battlegroundController;
        protected CardsController _cardsController;

        protected ILoadObjectsManager _loadObjectsManager;
        protected IGameplayManager _gameplayManager;
        protected IDataManager _dataManager;

        protected AbilityBoardArrow _targettingArrow;
        protected GameObject _vfxObject;

        protected bool _isAbilityResolved;

        protected Action OnObjectSelectedByTargettingArrowCallback;
        protected Action OnObjectSelectFailedByTargettingArrowCallback;

        public ulong activityId;
        
        public Enumerators.AbilityActivityType abilityActivityType;
        public Enumerators.AbilityCallType abilityCallType;
        public Enumerators.AbilityType abilityType;
        public Enumerators.AffectObjectType affectObjectType;
        public Enumerators.AbilityEffectType abilityEffectType;

        public List<Enumerators.AbilityTargetType> abilityTargetTypes;

        public Enumerators.CardKind cardKind;

        public Data.Card cardOwnerOfAbility;

        public BoardUnit abilityUnitOwner;
        public Player playerCallerOfAbility;
        public BoardSpell boardSpell;

        public BoardUnit targetUnit;
        public Player targetPlayer;
        public Player selectedPlayer;

        private Player playerAvatar;
        private Player opponenentAvatar;

        protected AbilityData abilityData;

        protected List<ulong> _particleIds;

        public AbilityBoardArrow TargettingArrow
        {
            get
            {
                return _targettingArrow;
            }
        }

        public AbilityBase(Enumerators.CardKind cardKind, AbilityData ability)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _particlesController = _gameplayManager.GetController<ParticlesController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _cardsController = _gameplayManager.GetController<CardsController>();

            abilityData = ability;
            this.cardKind = cardKind;
            this.abilityType = ability.abilityType;
            this.abilityActivityType = ability.abilityActivityType;
            this.abilityCallType = ability.abilityCallType;
            this.abilityTargetTypes = ability.abilityTargetTypes;
            this.abilityEffectType = ability.abilityEffectType;
            playerAvatar = _gameplayManager.CurrentPlayer;
            opponenentAvatar = _gameplayManager.OpponentPlayer;

            PermanentInputEndEvent += OnInputEndEventHandler;

            _particleIds = new List<ulong>();
        }

        public void ActivateSelectTarget(List<Enumerators.SkillTargetType> targetsType = null, Action callback = null, Action failedCallback = null)
        {
            OnObjectSelectedByTargettingArrowCallback = callback;
            OnObjectSelectFailedByTargettingArrowCallback = failedCallback;

            _targettingArrow = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object")).AddComponent<AbilityBoardArrow>();
            _targettingArrow.possibleTargets = abilityTargetTypes;
            _targettingArrow.selfBoardCreature = abilityUnitOwner;

            if (this.cardKind == Enumerators.CardKind.CREATURE)
                _targettingArrow.Begin(abilityUnitOwner.transform.position);
            else if (this.cardKind == Enumerators.CardKind.SPELL)
                _targettingArrow.Begin(selectedPlayer.AvatarObject.transform.position);//(boardSpell.transform.position);
            else
                _targettingArrow.Begin(playerCallerOfAbility.AvatarObject.transform.position);

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

                MonoBehaviour.Destroy(_targettingArrow.gameObject);
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

            if (this.cardKind == Enumerators.CardKind.CREATURE)
            {
				abilityUnitOwner.CreatureOnDieEvent += CreatureOnDieEventHandler;
                abilityUnitOwner.CreatureOnAttackEvent += UnitOnAttackEventHandler;

				if (abilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
                {
                  //  boardCreature.Card.ConnectAbility((uint)abilityType);
                }
            }
            else if (this.cardKind == Enumerators.CardKind.SPELL)
                boardSpell.SpellOnUsedEvent += SpellOnUsedEventHandler;

            if (playerCallerOfAbility.IsLocalPlayer)
                selectedPlayer = playerAvatar;
            else
                selectedPlayer = opponenentAvatar;
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

        protected virtual void CreateVFX(Vector3 pos, bool autoDestroy = false, float duration = 3f) //todo make it async
        {
            if (_vfxObject != null)
            {
                _vfxObject = MonoBehaviour.Instantiate(_vfxObject);
                _vfxObject.transform.position = (pos - Constants.VFX_OFFSET) + Vector3.forward;

                ulong id = _particlesController.RegisterParticleSystem(_vfxObject, autoDestroy, duration);
                
                if (!autoDestroy)
                    _particleIds.Add(id);
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

        public virtual void SelectedTargetAction(bool callInputEndBefore = false)
        {
            if (callInputEndBefore)
            {
                PermanentInputEndEvent?.Invoke();
                return;
            }

            if (targetUnit != null)
                affectObjectType = Enumerators.AffectObjectType.CHARACTER;
            else if (targetPlayer != null)
                affectObjectType = Enumerators.AffectObjectType.PLAYER;
            else
                affectObjectType = Enumerators.AffectObjectType.NONE;

            if (affectObjectType != Enumerators.AffectObjectType.NONE)
            {
                _isAbilityResolved = true;

                if (affectObjectType == Enumerators.AffectObjectType.CHARACTER)
                {
                   // targetCreature.Card.ConnectAbility((uint)abilityType);
                }

                OnObjectSelectedByTargettingArrowCallback?.Invoke();
                OnObjectSelectedByTargettingArrowCallback = null;
            }
            else
            {
                OnObjectSelectFailedByTargettingArrowCallback?.Invoke();
                OnObjectSelectFailedByTargettingArrowCallback = null;
            }
        }

        public virtual void Action(object info = null)
        {

        }

        protected virtual void OnEndTurnEventHandler()
        {
            if (_targettingArrow != null)
                OnInputEndEventHandler();
        }

        protected virtual void OnStartTurnEventHandler()
        {
         
        }

        protected virtual void CreatureOnDieEventHandler()
        {
          //  if(targetCreature != null)
            //    targetCreature.Card.DisconnectAbility((uint)abilityType);

            abilityUnitOwner.CreatureOnDieEvent -= CreatureOnDieEventHandler;
            _abilitiesController.DeactivateAbility(activityId);
            Dispose();
        }

		protected virtual void UnitOnAttackEventHandler(object info)
		{
			
		}

        protected void SpellOnUsedEventHandler()
        {
            boardSpell.SpellOnUsedEvent -= SpellOnUsedEventHandler;
            _abilitiesController.DeactivateAbility(activityId);
        }

        protected void DestroyCurrentParticle(bool isDirectly = false, float time = 3f)
        {
            if (isDirectly)
                DestroyParticle(null);
            else
                GameClient.Get<ITimerManager>().AddTimer(DestroyParticle, null, time, false);
        }


        private void DestroyParticle(object[] param)
        {
            MonoBehaviour.Destroy(_vfxObject);
        }

        protected void ClearParticles()
        {
            foreach (var id in _particleIds)
                _particlesController.DestoryParticle(id);
        }
    }
}