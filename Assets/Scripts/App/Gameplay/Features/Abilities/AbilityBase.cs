using CCGKit;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class AbilityBase
    {
        protected AbilitiesController _abilitiesController;
        protected ILoadObjectsManager _loadObjectsManager;
        protected AbilityTargetingArrow _targettingArrow;
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

        public BoardCreature boardCreature;
        public DemoHumanPlayer cardCaller;
        public BoardSpell boardSpell;

        public BoardCreature targetCreature;
        public PlayerAvatar targetPlayer;

        public AbilityTargetingArrow TargettingArrow
        {
            get
            {
                return _targettingArrow;
            }
        }

        public AbilityBase(Enumerators.CardKind cardKind, AbilityData ability)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            this.cardKind = cardKind;
            this.abilityType = ability.abilityType;
            this.abilityActivityType = ability.abilityActivityType;
            this.abilityCallType = ability.abilityCallType;
            this.abilityTargetTypes = ability.abilityTargetTypes;
            this.abilityEffectType = ability.abilityEffectType;
        }

        public void ActivateSelectTarget(EffectTarget targetType = EffectTarget.OpponentOrOpponentCreature, Action callback = null, Action failedCallback = null)
        {
            OnObjectSelectedByTargettingArrowCallback = callback;
            OnObjectSelectFailedByTargettingArrowCallback = failedCallback;

            _targettingArrow = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/AbilityTargetingArrow")).GetComponent<AbilityTargetingArrow>();
            _targettingArrow.possibleTargets = abilityTargetTypes;
            _targettingArrow.selfBoardCreature = boardCreature;

            if (this.cardKind == Enumerators.CardKind.CREATURE)
                _targettingArrow.Begin(boardCreature.transform.position);
            else if (this.cardKind == Enumerators.CardKind.SPELL)
                _targettingArrow.Begin(boardSpell.transform.position);
            else
                _targettingArrow.Begin(cardCaller.transform.position);

            _targettingArrow.OnCardSelectedEvent += OnCardSelectedEventHandler;
            _targettingArrow.OnCardUnselectedevent += OnCardUnselectedeventHandler;
            _targettingArrow.OnPlayerSelectedEvent += OnPlayerSelectedHandler;
            _targettingArrow.OnPlayerUnselectedEvent += OnPlayerUnselectedHandler;
            _targettingArrow.OnInputEndEvent += OnInputEndEventHandler;
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
            _abilitiesController = GameClient.Get<IGameplayManager>().GetController<AbilitiesController>();

            cardCaller.OnEndTurnEvent += OnEndTurnEventHandler;
            cardCaller.OnStartTurnEvent += OnStartTurnEventHandler;

            if (this.cardKind == Enumerators.CardKind.CREATURE)
            {
				boardCreature.CreatureOnDieEvent += CreatureOnDieEventHandler;
                boardCreature.CreatureOnAttackEvent += CreatureOnAttackEventHandler;

				if (abilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
                {
                    boardCreature.card.ConnectAbility((uint)abilityType);
                }
            }
            else if (this.cardKind == Enumerators.CardKind.SPELL)
                boardSpell.SpellOnUsedEvent += SpellOnUsedEventHandler;
        }

        public virtual void Update()
        {
        }

        public virtual void Dispose()
        {
            cardCaller.OnEndTurnEvent -= OnEndTurnEventHandler;
            cardCaller.OnStartTurnEvent -= OnStartTurnEventHandler;

            DeactivateSelectTarget();
        }

        protected virtual void OnCardSelectedEventHandler(BoardCreature obj)
        {
            targetCreature = obj;
            targetPlayer = null;
        }

        protected virtual void OnCardUnselectedeventHandler(BoardCreature obj)
        {
            targetCreature = null;
        }

        protected virtual void OnPlayerSelectedHandler(PlayerAvatar obj)
        {
            targetPlayer = obj;
            targetCreature = null;
        }

        protected virtual void OnPlayerUnselectedHandler(PlayerAvatar obj)
        {
            targetPlayer = null;
        }

        protected virtual void CreateVFX(Vector3 pos) //todo make it async
        {
            if (_vfxObject != null)
            {
                _vfxObject = MonoBehaviour.Instantiate(_vfxObject);
                _vfxObject.transform.position = (pos - Constants.VFX_OFFSET) + Vector3.forward;
            }
        }

        protected virtual void OnInputEndEventHandler()
        {
            if (targetCreature != null)
                affectObjectType = Enumerators.AffectObjectType.CHARACTER;
            else if (targetPlayer != null)
                affectObjectType = Enumerators.AffectObjectType.PLAYER;
            else
                affectObjectType = Enumerators.AffectObjectType.NONE;

            if (affectObjectType != Enumerators.AffectObjectType.NONE)
            {
                _isAbilityResolved = true;

                if(affectObjectType == Enumerators.AffectObjectType.CHARACTER)
                {
                    targetCreature.card.ConnectAbility((uint)abilityType);
                }

                OnObjectSelectedByTargettingArrowCallback?.Invoke();
                OnObjectSelectedByTargettingArrowCallback = null;
            }
            else
            {
                OnObjectSelectFailedByTargettingArrowCallback?.Invoke();
                OnObjectSelectFailedByTargettingArrowCallback = null;
            }

            DeactivateSelectTarget();
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
            if(targetCreature != null)
                targetCreature.card.DisconnectAbility((uint)abilityType);

            boardCreature.CreatureOnDieEvent -= CreatureOnDieEventHandler;
            _abilitiesController.DeactivateAbility(activityId);
            Dispose();
        }

		protected virtual void CreatureOnAttackEventHandler(object info)
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
    }
}