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
        public ulong ActivityId;

        public Enumerators.AbilityActivityType AbilityActivityType;

        public Enumerators.AbilityCallType AbilityCallType;

        public Enumerators.AbilityType AbilityType;

        public Enumerators.AffectObjectType AffectObjectType;

        public Enumerators.AbilityEffectType AbilityEffectType;

        public Enumerators.CardType TargetCardType = Enumerators.CardType.None;

        public Enumerators.UnitStatusType TargetUnitStatusType = Enumerators.UnitStatusType.None;

        public List<Enumerators.AbilityTargetType> AbilityTargetTypes;

        public Enumerators.CardKind CardKind;

        public Card CardOwnerOfAbility;

        public WorkingCard MainWorkingCard;

        public BoardUnit AbilityUnitOwner;

        public Player PlayerCallerOfAbility;

        public BoardSpell BoardSpell;

        public BoardCard BoardCard;

        public BoardUnit TargetUnit;

        public Player TargetPlayer;

        public Player SelectedPlayer;

        protected AbilitiesController AbilitiesController;

        protected ParticlesController ParticlesController;

        protected BattleController BattleController;

        protected ActionsQueueController ActionsQueueController;

        protected BattlegroundController BattlegroundController;

        protected CardsController CardsController;

        protected RanksController RanksController;

        protected ILoadObjectsManager LoadObjectsManager;

        protected IGameplayManager GameplayManager;

        protected IDataManager DataManager;

        protected ITimerManager TimerManager;

        protected ISoundManager SoundManager;

        protected GameObject VfxObject;

        protected bool IsAbilityResolved;

        protected Action OnObjectSelectedByTargettingArrowCallback;

        protected Action OnObjectSelectFailedByTargettingArrowCallback;

        protected List<ulong> ParticleIds;

        private readonly Player _playerAvatar;

        private readonly Player _opponenentAvatar;

        public AbilityBase(Enumerators.CardKind cardKind, AbilityData ability)
        {
            LoadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            GameplayManager = GameClient.Get<IGameplayManager>();
            DataManager = GameClient.Get<IDataManager>();
            TimerManager = GameClient.Get<ITimerManager>();
            SoundManager = GameClient.Get<ISoundManager>();

            AbilitiesController = GameplayManager.GetController<AbilitiesController>();
            ParticlesController = GameplayManager.GetController<ParticlesController>();
            BattleController = GameplayManager.GetController<BattleController>();
            ActionsQueueController = GameplayManager.GetController<ActionsQueueController>();
            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
            CardsController = GameplayManager.GetController<CardsController>();
            RanksController = GameplayManager.GetController<RanksController>();

            AbilityData = ability;
            this.CardKind = cardKind;
            AbilityType = ability.AbilityType;
            AbilityActivityType = ability.AbilityActivityType;
            AbilityCallType = ability.AbilityCallType;
            AbilityTargetTypes = ability.AbilityTargetTypes;
            AbilityEffectType = ability.AbilityEffectType;
            _playerAvatar = GameplayManager.CurrentPlayer;
            _opponenentAvatar = GameplayManager.OpponentPlayer;

            PermanentInputEndEvent += OnInputEndEventHandler;

            ParticleIds = new List<ulong>();
        }

        protected event Action PermanentInputEndEvent;

        public AbilityBoardArrow TargettingArrow { get; protected set; }

        public AbilityData AbilityData { get; protected set; }

        public void ActivateSelectTarget(List<Enumerators.SkillTargetType> targetsType = null, Action callback = null, Action failedCallback = null)
        {
            OnObjectSelectedByTargettingArrowCallback = callback;
            OnObjectSelectFailedByTargettingArrowCallback = failedCallback;

            TargettingArrow = Object.Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object")).AddComponent<AbilityBoardArrow>();
            TargettingArrow.PossibleTargets = AbilityTargetTypes;
            TargettingArrow.SelfBoardCreature = AbilityUnitOwner;
            TargettingArrow.TargetUnitType = TargetCardType;
            TargettingArrow.TargetUnitStatusType = TargetUnitStatusType;

            if (CardKind == Enumerators.CardKind.Creature)
            {
                TargettingArrow.Begin(AbilityUnitOwner.Transform.position);
            } else if (CardKind == Enumerators.CardKind.Spell)
            {
                TargettingArrow.Begin(SelectedPlayer.AvatarObject.transform.position); // (boardSpell.transform.position);
            } else
            {
                TargettingArrow.Begin(PlayerCallerOfAbility.AvatarObject.transform.position);
            }

            TargettingArrow.OnCardSelectedEvent += OnCardSelectedEventHandler;
            TargettingArrow.OnCardUnselectedevent += OnCardUnselectedeventHandler;
            TargettingArrow.OnPlayerSelectedEvent += OnPlayerSelectedHandler;
            TargettingArrow.OnPlayerUnselectedEvent += OnPlayerUnselectedHandler;
            TargettingArrow.OnInputEndEvent += OnInputEndEventHandler;
            TargettingArrow.OnInputCancelEvent += OnInputCancelEventHandler;
        }

        public void DeactivateSelectTarget()
        {
            if (TargettingArrow != null)
            {
                TargettingArrow.OnCardSelectedEvent -= OnCardSelectedEventHandler;
                TargettingArrow.OnCardUnselectedevent -= OnCardUnselectedeventHandler;
                TargettingArrow.OnPlayerSelectedEvent -= OnPlayerSelectedHandler;
                TargettingArrow.OnPlayerUnselectedEvent -= OnPlayerUnselectedHandler;
                TargettingArrow.OnInputEndEvent -= OnInputEndEventHandler;
                TargettingArrow.OnInputCancelEvent -= OnInputCancelEventHandler;

                TargettingArrow.Dispose();
                TargettingArrow = null;
            }
        }

        public AbilityBase Clone()
        {
            return (AbilityBase)MemberwiseClone();
        }

        public virtual void Activate()
        {
            PlayerCallerOfAbility.OnEndTurnEvent += OnEndTurnEventHandler;
            PlayerCallerOfAbility.OnStartTurnEvent += OnStartTurnEventHandler;

            if ((CardKind == Enumerators.CardKind.Creature) && (AbilityUnitOwner != null))
            {
                AbilityUnitOwner.UnitOnDieEvent += UnitOnDieEventHandler;
                AbilityUnitOwner.UnitOnAttackEvent += UnitOnAttackEventHandler;
                AbilityUnitOwner.UnitHpChangedEvent += UnitHPChangedEventHandler;
                AbilityUnitOwner.UnitGotDamageEvent += UnitGotDamageEventHandler;

                if (AbilityActivityType == Enumerators.AbilityActivityType.Passive)
                {
                    // boardCreature.Card.ConnectAbility((uint)abilityType);
                }
            } else if ((CardKind == Enumerators.CardKind.Spell) && (BoardSpell != null))
            {
                BoardSpell.SpellOnUsedEvent += SpellOnUsedEventHandler;
            }

            if (PlayerCallerOfAbility.IsLocalPlayer)
            {
                SelectedPlayer = _playerAvatar;
            } else
            {
                SelectedPlayer = _opponenentAvatar;
            }
        }

        public virtual void Update()
        {
        }

        public virtual void Dispose()
        {
            PlayerCallerOfAbility.OnEndTurnEvent -= OnEndTurnEventHandler;
            PlayerCallerOfAbility.OnStartTurnEvent -= OnStartTurnEventHandler;

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

            if (TargetUnit != null)
            {
                AffectObjectType = Enumerators.AffectObjectType.Character;
            } else if (TargetPlayer != null)
            {
                AffectObjectType = Enumerators.AffectObjectType.Player;
            } else
            {
                AffectObjectType = Enumerators.AffectObjectType.None;
            }

            if (AffectObjectType != Enumerators.AffectObjectType.None)
            {
                IsAbilityResolved = true;

                if (AffectObjectType == Enumerators.AffectObjectType.Character)
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
            TargetUnit = obj;

            TargetPlayer = null;
        }

        protected virtual void OnCardUnselectedeventHandler(BoardUnit obj)
        {
            TargetUnit = null;
        }

        protected virtual void OnPlayerSelectedHandler(Player obj)
        {
            TargetPlayer = obj;

            TargetUnit = null;
        }

        protected virtual void OnPlayerUnselectedHandler(Player obj)
        {
            TargetPlayer = null;
        }

        protected virtual void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3f, bool justPosition = false)
        {
            // todo make it async
            if (VfxObject != null)
            {
                VfxObject = Object.Instantiate(VfxObject);

                if (!justPosition)
                {
                    VfxObject.transform.position = (pos - Constants.VfxOffset) + Vector3.forward;
                } else
                {
                    VfxObject.transform.position = pos;
                }

                ulong id = ParticlesController.RegisterParticleSystem(VfxObject, autoDestroy, duration);

                if (!autoDestroy)
                {
                    ParticleIds.Add(id);
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
            if (TargettingArrow != null)
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
            AbilityUnitOwner.UnitOnDieEvent -= UnitOnDieEventHandler;
            AbilityUnitOwner.UnitHpChangedEvent -= UnitHPChangedEventHandler;
            AbilityUnitOwner.UnitGotDamageEvent -= UnitGotDamageEventHandler;

            AbilitiesController.DeactivateAbility(ActivityId);
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
            BoardSpell.SpellOnUsedEvent -= SpellOnUsedEventHandler;

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
            foreach (ulong id in ParticleIds)
            {
                ParticlesController.DestoryParticle(id);
            }
        }

        protected object GetCaller()
        {
            return AbilityUnitOwner != null?AbilityUnitOwner:(object)BoardSpell;
        }

        protected Player GetOpponentOverlord()
        {
            return PlayerCallerOfAbility.Equals(GameplayManager.CurrentPlayer)?GameplayManager.OpponentPlayer:GameplayManager.CurrentPlayer;
        }

        private void DestroyParticle(object[] param)
        {
            Object.Destroy(VfxObject);
        }
    }
}
