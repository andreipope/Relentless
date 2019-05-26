using System;
using System.Collections.Generic;
using System.Linq;
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

        public CardModel CardOwnerOfAbility => CardModel;

        public CardModel CardModel;

        public CardModel AbilityUnitOwner;

        public Player PlayerCallerOfAbility;

        public CardModel TargetUnit;

        public Player TargetPlayer;

        public Player SelectedPlayer;

        public bool LastAuraState;

        public List<ParametrizedAbilityBoardObject> PredefinedTargets;

        protected AbilitiesController AbilitiesController;

        protected ParticlesController ParticlesController;

        protected BattleController BattleController;

        protected ActionsQueueController ActionsQueueController;

        protected ActionsReportController ActionsReportController;

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

        protected GameplayActionQueueAction AbilityProcessingAction;

        protected GameplayActionQueueAction AbilityTargetingAction;

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
            ActionsReportController = GameplayManager.GetController<ActionsReportController>();
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
            TargettingArrow.SubTrigger = AbilityData.SubTrigger;
            TargettingArrow.OwnerOfThis = this;

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

            AbilityTargetingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityTargetingBlocker);
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

            CompleteTargetingAction();
        }

        public async virtual void Activate()
        {
            GameplayManager.GameEnded += GameEndedHandler;

            PlayerCallerOfAbility.TurnEnded += TurnEndedHandler;
            PlayerCallerOfAbility.TurnStarted += TurnStartedHandler;
            PlayerCallerOfAbility.PlayerCardsController.BoardChanged += BoardChangedHandler;
            PlayerCallerOfAbility.PlayerCardsController.HandChanged += HandChangedHandler;
            PlayerCallerOfAbility.PlayerCurrentGooChanged += PlayerCurrentGooChangedHandler;

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
                        AbilityUnitOwner.UnitAttackStateFinished += UnitAttackStateFinishedHandler;
                    }
                    break;
                case Enumerators.CardKind.ITEM:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(CardKind), CardKind, null);
            }

            SelectedPlayer = PlayerCallerOfAbility.IsLocalPlayer ? _playerAvatar : _opponentAvatar;

            await new WaitForUpdate();

            LastAuraState = true;
            ChangeAuraStatusAction(true);
        }

        public virtual void Update()
        {
        }

        public virtual void Dispose()
        {
            GameplayManager.GameEnded -= GameEndedHandler;

            if (PlayerCallerOfAbility != null) 
            {
                PlayerCallerOfAbility.TurnEnded -= TurnEndedHandler;
                PlayerCallerOfAbility.TurnStarted -= TurnStartedHandler;
                PlayerCallerOfAbility.PlayerCardsController.BoardChanged -= BoardChangedHandler;
                PlayerCallerOfAbility.PlayerCardsController.HandChanged -= HandChangedHandler;
                PlayerCallerOfAbility.PlayerCurrentGooChanged -= PlayerCurrentGooChangedHandler;
            }
            
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
                AbilityUnitOwner.UnitAttackStateFinished -= UnitAttackStateFinishedHandler;
            }

            AbilitiesController.DeactivateAbility(ActivityId);
        }

        public void ChangePlayerCallerOfAbility(Player player)
        {
            PlayerOwnerHasChanged(PlayerCallerOfAbility, player);

            PlayerCallerOfAbility.TurnEnded -= TurnEndedHandler;
            PlayerCallerOfAbility.TurnStarted -= TurnStartedHandler;
            PlayerCallerOfAbility.PlayerCardsController.BoardChanged -= BoardChangedHandler;
            PlayerCallerOfAbility.PlayerCardsController.HandChanged -= HandChangedHandler;
            PlayerCallerOfAbility.PlayerCurrentGooChanged -= PlayerCurrentGooChangedHandler;

            PlayerCallerOfAbility = player;

            PlayerCallerOfAbility.TurnEnded += TurnEndedHandler;
            PlayerCallerOfAbility.TurnStarted += TurnStartedHandler;
            PlayerCallerOfAbility.PlayerCardsController.BoardChanged += BoardChangedHandler;
            PlayerCallerOfAbility.PlayerCardsController.HandChanged += HandChangedHandler;
            PlayerCallerOfAbility.PlayerCurrentGooChanged += PlayerCurrentGooChangedHandler;
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
                CompleteTargetingAction();
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

            CompleteTargetingAction();
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
            if (LastAuraState)
            {
                LastAuraState = false;
                ChangeAuraStatusAction(false);
            }

            Deactivate();
            Dispose();
        }

        protected virtual void UnitAttackedHandler(IBoardObject info, int damage, bool isAttacker)
        {

        }

        protected virtual void UnitHpChangedHandler(int oldValue, int newValue)
        {
            if (!AbilityUnitOwner.IsAttacking)
            {
                CheckRageStatus();
            }

            if (AbilityUnitOwner.CurrentDefense <= 0)
            {
                if (LastAuraState)
                {
                    LastAuraState = false;
                    ChangeAuraStatusAction(false);
                }
            }
        }

        protected virtual void ChangeRageStatusAction(bool rageStatus)
        {
        }

        protected virtual void UnitDamagedHandler(IBoardObject from)
        {
        }

        protected virtual void UnitAttackedEndedHandler()
        {
        }

        protected virtual void UnitKilledUnitHandler(CardModel unit)
        {

        }

        protected virtual void ChangeAuraStatusAction(bool status)
        {

        }

        protected virtual void BoardChangedHandler(int count)
        {

        }

        protected virtual void HandChangedHandler(int count)
        {

        }
        protected virtual void PlayerCurrentGooChangedHandler(int goo)
        {
            
        }

        protected virtual void UnitAttackStateFinishedHandler()
        {
            CheckRageStatus();
        }

        protected virtual void PrepairingToDieHandler(IBoardObject from)
        {
            AbilitiesController.DeactivateAbility(ActivityId);
        }

        private void CheckRageStatus()
        {
            if (AbilityUnitOwner.CurrentDefense < AbilityUnitOwner.MaxCurrentDefense)
            {
                if (!UnitOwnerIsInRage)
                {
                    UnitOwnerIsInRage = true;
                    ChangeRageStatusAction(UnitOwnerIsInRage);
                }
            }
            else
            {
                if (UnitOwnerIsInRage)
                {
                    UnitOwnerIsInRage = false;
                    ChangeRageStatusAction(UnitOwnerIsInRage);
                }
            }
        }

        protected void ClearParticles()
        {
            foreach (ulong id in ParticleIds)
            {
                ParticlesController.DestroyParticle(id);
            }
        }

        public Player GetOpponentOverlord()
        {
            return GetOpponentOverlord(PlayerCallerOfAbility);
        }

        public Player GetOpponentOverlord(Player player)
        {
            return player == GameplayManager.CurrentPlayer ?
                GameplayManager.OpponentPlayer :
                GameplayManager.CurrentPlayer;
        }

        protected BoardUnitView GetAbilityUnitOwnerView()
        {
            return BattlegroundController.GetCardViewByModel<BoardUnitView>(AbilityUnitOwner);
        }

        protected List<CardModel> GetRandomEnemyUnits(int count)
        {
            return BattlegroundController.GetDeterministicRandomElements(GetOpponentOverlord().CardsOnBoard.ToList(), count);
        }

        protected List<CardModel> GetRandomUnits(List<CardModel> units,int count)
        {
            return BattlegroundController.GetDeterministicRandomElements(units, count);
        }

        protected List<T> GetRandomElements<T>(List<T> elements, int count)
        {
            return BattlegroundController.GetDeterministicRandomElements(elements, count);
        }

        protected IEnumerable<CardModel> GetAliveUnits(IEnumerable<CardModel> units)
        {
            return BattlegroundController.GetAliveUnits(units);
        }

        protected bool HasEmptySpaceOnBoard(Player player, out int emptyFields)
        {
            emptyFields = GetAliveUnits(player.PlayerCardsController.CardsOnBoard).Count();
            emptyFields = (int)player.MaxCardsInPlay-emptyFields;
            return emptyFields > 0;
        }

        public void InvokeActionTriggered(object info = null)
        {
            ActionTriggered?.Invoke(info);
        }

        protected virtual void VFXAnimationEndedHandler()
        {

        }

        protected void ReportAbilityDoneAction(List<IBoardObject> targets)
        {

        }

        protected int GetAbilityIndex()
        {
            int index = CardModel.Card.InstanceCard.Abilities.IndexOf(AbilityData);
            if (index == -1)
                throw new Exception($"Ability {AbilityData} not found in card {CardModel}");

            return index;
        }

        protected void InvokeUseAbilityEvent(List<ParametrizedAbilityBoardObject> targets = null)
        {
            if (IgnoreAbilityUsageEvent)
                return;

            AbilitiesController.InvokeUseAbilityEvent(
                CardModel,
                AbilityData.Ability,
                targets ?? new List<ParametrizedAbilityBoardObject>()
            );
        }

        private void CompleteTargetingAction()
        {
            AbilityTargetingAction?.TriggerActionExternally();
            AbilityTargetingAction = null;
        }
    }
}
