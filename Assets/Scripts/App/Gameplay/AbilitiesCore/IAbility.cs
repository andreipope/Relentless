using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public interface ICardAbility
    {
        event Action AbilityInitialized;

        event Action<Player, Player> PlayerOwnerChanged;

        IReadOnlyList<GenericParameter> GenericParameters { get; }

        BoardUnitModel UnitModelOwner { get; }

        Player PlayerOwner { get; }

        CardAbilitiesData CardAbilitiesData { get; }

        AbilityData AbilityData { get; }

        ICardAbilityView AbilityView { get; }

        AbilityData.TriggerInfo MainTrigger { get; }

        int TurnsOnBoard { get; }

        void Init(
            BoardUnitModel boardUnitModel,
            CardAbilitiesData combination,
            AbilityData.TriggerInfo trigger,
            AbilityData cardAbilityData,
            IReadOnlyList<BoardObject> targets = null,
            ICardAbilityView abilityView = null);
        void DoAction(IReadOnlyList<GenericParameter> genericParameters);
        void ChangePlayerOwner(Player player);
        void InsertTargets(IReadOnlyList<BoardObject> targets);
        void IncreaseTurnsOnBoard();
        void Dispose();
    }

    public class CardAbility : ICardAbility
    {
        protected readonly ILog Log;

        public event Action AbilityInitialized;

        public event Action<Player, Player> PlayerOwnerChanged;

        protected IReadOnlyList<BoardObject> Targets { get; private set; }

        protected readonly IGameplayManager GameplayManager;

        protected readonly IDataManager DataManager;

        protected readonly AbilitiesController AbilitiesController;

        protected readonly BattlegroundController BattlegroundController;

        protected readonly BattleController BattleController;

        protected readonly CardsController CardsController;

        protected readonly BoardController BoardController;

        protected readonly ActionsQueueController ActionsQueueController;

        protected readonly RanksController RanksController;

        public IReadOnlyList<GenericParameter> GenericParameters { get; private set; }

        public BoardUnitModel UnitModelOwner { get; private set; }

        public Player PlayerOwner { get; private set; }

        public AbilityData AbilityData { get; private set; }

        public CardAbilitiesData CardAbilitiesData { get; private set; }

        public ICardAbilityView AbilityView { get; private set; }

        public AbilityData.TriggerInfo MainTrigger { get; private set; }

        public int TurnsOnBoard { get; private set; }

        public CardAbility()
        {
            GameplayManager = GameClient.Get<IGameplayManager>();
            DataManager = GameClient.Get<IDataManager>();
            AbilitiesController = GameplayManager.GetController<AbilitiesController>();
            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
            BattleController = GameplayManager.GetController<BattleController>();
            CardsController = GameplayManager.GetController<CardsController>();
            BoardController = GameplayManager.GetController<BoardController>();
            ActionsQueueController = GameplayManager.GetController<ActionsQueueController>();
            RanksController = GameplayManager.GetController<RanksController>();

            Log = Logging.GetLog(GetType().Name);
        }

        public virtual void DoAction(IReadOnlyList<GenericParameter> genericParameters) { }

        public virtual void Init(
            BoardUnitModel boardUnitModel,
            CardAbilitiesData combination,
            AbilityData.TriggerInfo trigger,
            AbilityData cardAbilityData,
            IReadOnlyList<BoardObject> targets = null,
            ICardAbilityView abilityView = null)
        {
            UnitModelOwner = boardUnitModel;
            PlayerOwner = boardUnitModel.OwnerPlayer;
            AbilityData = cardAbilityData;
            Targets = targets;
            AbilityView = abilityView;
            CardAbilitiesData = combination;
            MainTrigger = trigger;
            GenericParameters = AbilitiesController.GetAllGenericParameters(this);

            AbilityView?.Init(this);

            AbilityInitializedAction();
        }

        public void ChangePlayerOwner(Player player)
        {
            Player oldPlayer = PlayerOwner;

            PlayerOwner = player;

            PlayerOwnerChangedAction(oldPlayer, player);
        }

        public virtual void Dispose()
        {
            AbilitiesController.EndAbility(this);
        }

        public void InsertTargets(IReadOnlyList<BoardObject> targets)
        {
            Targets = targets;
        }

        public virtual void AbilityInitializedAction()
        {
            AbilityInitialized?.Invoke();
        }

        public virtual void PlayerOwnerChangedAction(Player oldPlayer, Player newPlayer)
        {
            PlayerOwnerChanged?.Invoke(oldPlayer, newPlayer);
        }

        public void IncreaseTurnsOnBoard()
        {
            TurnsOnBoard++;
        }

        protected void PostGameActionReport(Enumerators.ActionType actionType, List<PastActionsPopup.TargetEffectParam> targetEffectParams)
        {
            if (targetEffectParams == null || targetEffectParams.Count == 0)
                return;

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = actionType,
                Caller = UnitModelOwner,
                TargetEffects = targetEffectParams
            });
        }
    }
}
