using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class AbilityBase
    {
        public event Action VFXAnimationEnded;

        public ulong ActivityId;

        public bool IsPVPAbility;

        public bool IgnoreAbilityUsageEvent;

        public Enumerators.AbilityActivity AbilityActivity;

        public Enumerators.AbilityTrigger AbilityTrigger;

        public Enumerators.AffectObjectType AffectObjectType;

        public Enumerators.AbilityEffect AbilityEffect;

        public Enumerators.CardType TargetCardType = Enumerators.CardType.UNDEFINED;

        public Enumerators.UnitSpecialStatus TargetUnitSpecialStatus = Enumerators.UnitSpecialStatus.NONE;

        public List<Enumerators.Target> AbilityTargets;

        public Enumerators.CardKind CardKind;

        public BoardUnitModel CardOwnerOfAbility => BoardUnitModel;

        public BoardUnitModel BoardUnitModel;

        public BoardUnitModel AbilityUnitOwner;

        public Player PlayerCallerOfAbility;

        public BoardItem BoardItem;

        public BoardCardView BoardCardView;

        public BoardUnitModel TargetUnit;

        public Player TargetPlayer;

        public Player SelectedPlayer;

        public List<ParametrizedAbilityBoardObject> PredefinedTargets;

        protected AbilitiesController AbilitiesController;

        protected ParticlesController ParticlesController;

        protected BattleController BattleController;

        protected ActionsQueueController ActionsQueueController;

        protected BattlegroundController BattlegroundController;

        protected RanksController RanksController;

        protected CardsController CardsController;

        protected BoardController BoardController;

        protected ILoadObjectsManager LoadObjectsManager;

        protected IGameplayManager GameplayManager;

        protected IDataManager DataManager;

        protected ITimerManager TimerManager;

        protected ISoundManager SoundManager;

        protected IPvPManager PvPManager;

        protected ITutorialManager TutorialManager;

        protected GameObject VfxObject;

        protected bool IsAbilityResolved;

        protected Action OnObjectSelectedByTargettingArrowCallback;

        protected Action OnObjectSelectFailedByTargettingArrowCallback;

        protected List<ulong> ParticleIds;

        private readonly Player _playerAvatar;

        private readonly Player _opponentAvatar;

        protected GameplayQueueAction<object> AbilityProcessingAction;

        protected bool UnitOwnerIsInRage;
        
        public AbilityBase()
        {
            GameplayManager = GameClient.Get<IGameplayManager>();
            BoardController = GameplayManager.GetController<BoardController>();
            BattleController = GameplayManager.GetController<BattleController>();
            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
            CardsController = GameplayManager.GetController<CardsController>();
            RanksController = GameplayManager.GetController<RanksController>();
        }

        public AbilityBase(Enumerators.CardKind cardKind, AbilityData ability)
        {
            LoadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            GameplayManager = GameClient.Get<IGameplayManager>();
            DataManager = GameClient.Get<IDataManager>();
            TimerManager = GameClient.Get<ITimerManager>();
            SoundManager = GameClient.Get<ISoundManager>();
            PvPManager = GameClient.Get<IPvPManager>();
            TutorialManager = GameClient.Get<ITutorialManager>();

            AbilitiesController = GameplayManager.GetController<AbilitiesController>();
            ParticlesController = GameplayManager.GetController<ParticlesController>();
            BattleController = GameplayManager.GetController<BattleController>();
            ActionsQueueController = GameplayManager.GetController<ActionsQueueController>();
            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
            CardsController = GameplayManager.GetController<CardsController>();
            BoardController = GameplayManager.GetController<BoardController>();
            RanksController = GameplayManager.GetController<RanksController>();

            AbilityData = ability;
            CardKind = cardKind;
            AbilityActivity = ability.Activity;
            AbilityTrigger = ability.Trigger;
            AbilityTargets = ability.Targets;
            AbilityEffect = ability.Effect;
            _playerAvatar = GameplayManager.CurrentPlayer;
            _opponentAvatar = GameplayManager.OpponentPlayer;

            PermanentInputEndEvent += InputEndedHandler;

            ParticleIds = new List<ulong>();
        }

        protected event Action PermanentInputEndEvent;

        public event Action<object> ActionTriggered;

        public event Action Disposed;

        public AbilityBoardArrow TargettingArrow { get; protected set; }

        public AbilityData AbilityData { get; protected set; }

        public void ActivateSelectTarget(
            List<Enumerators.SkillTarget> targetsType = null, Action callback = null, Action failedCallback = null)
        {
            OnObjectSelectedByTargettingArrowCallback = callback;
            OnObjectSelectFailedByTargettingArrowCallback = failedCallback;

            TargettingArrow =
                Object
                .Instantiate(LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object"))
                .AddComponent<AbilityBoardArrow>();
            TargettingArrow.PossibleTargets = AbilityTargets;
            TargettingArrow.TargetUnitType = TargetCardType;
            TargettingArrow.TargetUnitSpecialStatusType = TargetUnitSpecialStatus;
            TargettingArrow.UnitDefense = AbilityData.Defense2;
            TargettingArrow.UnitCost = AbilityData.Cost;

            switch (CardKind)
            {
                case Enumerators.CardKind.CREATURE:

                    BoardUnitView abilityUnitOwnerView = GetAbilityUnitOwnerView();
                    TargettingArrow.SelfBoardCreature = abilityUnitOwnerView;
                    TargettingArrow.Begin(abilityUnitOwnerView.Transform.position);
                    break;
                case Enumerators.CardKind.ITEM:
                    TargettingArrow.Begin(SelectedPlayer.AvatarObject.transform.position);
                    break;
                default:
                    TargettingArrow.Begin(PlayerCallerOfAbility.AvatarObject.transform.position);
                    break;
            }

            TargettingArrow.CardSelected += CardSelectedHandler;
            TargettingArrow.CardUnselected += CardUnselectedHandler;
            TargettingArrow.PlayerSelected += PlayerSelectedHandler;
            TargettingArrow.PlayerUnselected += PlayerUnselectedHandler;
            TargettingArrow.InputEnded += InputEndedHandler;
            TargettingArrow.InputCanceled += InputCanceledHandler;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);
        }

        public void DeactivateSelectTarget()
        {
            if (TargettingArrow != null)
            {
                TargettingArrow.CardSelected -= CardSelectedHandler;
                TargettingArrow.CardUnselected -= CardUnselectedHandler;
                TargettingArrow.PlayerSelected -= PlayerSelectedHandler;
                TargettingArrow.PlayerUnselected -= PlayerUnselectedHandler;
                TargettingArrow.InputEnded -= InputEndedHandler;
                TargettingArrow.InputCanceled -= InputCanceledHandler;

                TargettingArrow.Dispose();
                TargettingArrow = null;
            }
        }

        public virtual void Activate()
        {
            GameplayManager.GameEnded += GameEndedHandler;

            PlayerCallerOfAbility.TurnEnded += TurnEndedHandler;
            PlayerCallerOfAbility.TurnStarted += TurnStartedHandler;

            VFXAnimationEnded += VFXAnimationEndedHandler;

            switch (CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    if (AbilityUnitOwner != null)
                    {
                        AbilityUnitOwner.UnitDied += UnitDiedHandler;
                        AbilityUnitOwner.UnitAttacked += UnitAttackedHandler;
                        AbilityUnitOwner.UnitDefenseChanged += UnitHpChangedHandler;
                        AbilityUnitOwner.UnitDamaged += UnitDamagedHandler;
                        AbilityUnitOwner.PrepairingToDie += PrepairingToDieHandler;
                        AbilityUnitOwner.KilledUnit += UnitKilledUnitHandler;
                        AbilityUnitOwner.UnitAttackedEnded += UnitAttackedEndedHandler;
                    }
                    break;
                case Enumerators.CardKind.ITEM:
                    if (BoardItem != null)
                    {
                        BoardItem.Used += UsedHandler;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(CardKind), CardKind, null);
            }

            SelectedPlayer = PlayerCallerOfAbility.IsLocalPlayer ? _playerAvatar : _opponentAvatar;
        }

        public virtual void Update()
        {
        }

        public virtual void Dispose()
        {
            GameplayManager.GameEnded -= GameEndedHandler;

            PlayerCallerOfAbility.TurnEnded -= TurnEndedHandler;
            PlayerCallerOfAbility.TurnStarted -= TurnStartedHandler;

            VFXAnimationEnded -= VFXAnimationEndedHandler;

            DeactivateSelectTarget();
            ClearParticles();

            Disposed?.Invoke();
        }

        public virtual void Deactivate()
        {
            if (AbilityUnitOwner != null)
            {
                AbilityUnitOwner.UnitDied -= UnitDiedHandler;
                AbilityUnitOwner.UnitAttacked -= UnitAttackedHandler;
                AbilityUnitOwner.UnitDefenseChanged -= UnitHpChangedHandler;
                AbilityUnitOwner.UnitDamaged -= UnitDamagedHandler;
                AbilityUnitOwner.PrepairingToDie -= PrepairingToDieHandler;
                AbilityUnitOwner.KilledUnit -= UnitKilledUnitHandler;
                AbilityUnitOwner.UnitAttackedEnded -= UnitAttackedEndedHandler;
            }

            AbilitiesController.DeactivateAbility(ActivityId);
        }

        public void ChangePlayerCallerOfAbility(Player player)
        {
            PlayerOwnerHasChanged(PlayerCallerOfAbility, player);

            PlayerCallerOfAbility.TurnEnded -= TurnEndedHandler;
            PlayerCallerOfAbility.TurnStarted -= TurnStartedHandler;

            PlayerCallerOfAbility = player;

            PlayerCallerOfAbility.TurnEnded += TurnEndedHandler;
            PlayerCallerOfAbility.TurnStarted += TurnStartedHandler;
        }

        private void GameEndedHandler(Enumerators.EndGameType endGameType)
        {
            Deactivate();
        }

        protected virtual void PlayerOwnerHasChanged(Player oldPlayer, Player newPlayer)
        {
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
            }
            else if (TargetPlayer != null)
            {
                AffectObjectType = Enumerators.AffectObjectType.Player;
            }
            else
            {
                AffectObjectType = Enumerators.AffectObjectType.None;
            }

            if (AffectObjectType != Enumerators.AffectObjectType.None)
            {
                IsAbilityResolved = true;

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

        public void InvokeVFXAnimationEnded()
        {
            VFXAnimationEnded?.Invoke();
        }

        protected virtual void CardSelectedHandler(BoardUnitView obj)
        {
            TargetUnit = obj.Model;

            TargetPlayer = null;
        }

        protected virtual void CardUnselectedHandler(BoardUnitView obj)
        {
            TargetUnit = null;
        }

        protected virtual void PlayerSelectedHandler(Player obj)
        {
            TargetPlayer = obj;

            TargetUnit = null;
        }

        protected virtual void PlayerUnselectedHandler(Player obj)
        {
            TargetPlayer = null;
        }

        protected virtual void CreateVfx(
            Vector3 pos, bool autoDestroy = false, float duration = 3f, bool justPosition = false)
        {
            // todo make it async
            if (VfxObject != null)
            {
                VfxObject = Object.Instantiate(VfxObject);

                if (!justPosition)
                {
                    VfxObject.transform.position = pos - Constants.VfxOffset + Vector3.forward;
                }
                else
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

        protected virtual void InputEndedHandler()
        {
            SelectedTargetAction();
            DeactivateSelectTarget();
        }

        protected virtual void InputCanceledHandler()
        {
            OnObjectSelectFailedByTargettingArrowCallback?.Invoke();
            OnObjectSelectFailedByTargettingArrowCallback = null;

            DeactivateSelectTarget();
        }

        protected virtual void TurnEndedHandler()
        {
            if (TargettingArrow != null && !GameplayManager.GetController<BoardArrowController>().IsBoardArrowNowInTheBattle)
            {
                InputEndedHandler();
            }
        }

        protected virtual void TurnStartedHandler()
        {
        }

        protected virtual void UnitDiedHandler()
        {
            Deactivate();
            Dispose();
        }

        protected virtual void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
        }

        protected virtual void UnitHpChangedHandler(int oldValue, int newValue)
        {
            if (!UnitOwnerIsInRage)
            {
                UnitOwnerIsInRage = true;
                ChangeRageStatusAction(UnitOwnerIsInRage);
            }
            else
            {
                UnitOwnerIsInRage = false;
                ChangeRageStatusAction(UnitOwnerIsInRage);
            }
        }

        protected virtual void ChangeRageStatusAction(bool rageStatus)
        {
        }

        protected virtual void UnitDamagedHandler(BoardObject from)
        {
        }

        protected virtual void UnitAttackedEndedHandler()
        {
        }

        protected virtual void UnitKilledUnitHandler(BoardUnitModel unit)
        {

        }


        protected virtual void PrepairingToDieHandler(BoardObject from)
        {
            AbilitiesController.DeactivateAbility(ActivityId);
        }
        
        protected void UsedHandler()
        {
            BoardItem.Used -= UsedHandler;
        }

        protected void ClearParticles()
        {
            foreach (ulong id in ParticleIds)
            {
                ParticlesController.DestroyParticle(id);
            }
        }

        public BoardObject GetCaller()
        {
            return AbilityUnitOwner ?? (BoardObject) BoardItem;
        }

        public Player GetOpponentOverlord()
        {
            return PlayerCallerOfAbility.Equals(GameplayManager.CurrentPlayer) ?
                GameplayManager.OpponentPlayer :
                GameplayManager.CurrentPlayer;
        }

        protected BoardUnitView GetAbilityUnitOwnerView()
        {
            return BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(AbilityUnitOwner);
        }

        protected List<BoardUnitModel> GetRandomEnemyUnits(int count)
        {
            return InternalTools.GetRandomElementsFromList(GetOpponentOverlord().CardsOnBoard, count)
                .FindAll(card => card.CurrentDefense > 0 && !card.IsDead);
        }

        protected List<BoardUnitModel> GetRandomUnits(List<BoardUnitModel> units,int count)
        {
            return InternalTools.GetRandomElementsFromList(units, count)
                .FindAll(card => card.CurrentDefense > 0 && !card.IsDead);
        }

        protected List<T> GetRandomElements<T>(List<T> elements, int count)
        {
            return InternalTools.GetRandomElementsFromList(elements, count);
        }

        public void InvokeActionTriggered(object info = null)
        {
            ActionTriggered?.Invoke(info);
        }

        protected virtual void VFXAnimationEndedHandler()
        {

        }

        protected void ReportAbilityDoneAction(List<BoardObject> targets)
        {

        }

        protected int GetAbilityIndex()
        {
            int index = BoardUnitModel.Card.InstanceCard.Abilities.IndexOf(AbilityData);
            if (index == -1)
                throw new Exception($"Ability {AbilityData} not found in card {BoardUnitModel}");

            return index;
        }

        protected void InvokeUseAbilityEvent(List<ParametrizedAbilityBoardObject> targets = null)
        {
            if (IgnoreAbilityUsageEvent)
                return;

            AbilitiesController.InvokeUseAbilityEvent(
                BoardUnitModel,
                AbilityData.Ability,
                targets ?? new List<ParametrizedAbilityBoardObject>()
            );
        }
    }
}
