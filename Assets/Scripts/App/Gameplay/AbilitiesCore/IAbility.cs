using log4net;
using Loom.ZombieBattleground.Common;
using System;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public interface ICardAbility
    {
        event Action AbilityInitialized;

        IReadOnlyList<GenericParameter> GenericParameters { get; }

        BoardUnitModel UnitModelOwner { get; }

        Player PlayerOwner { get; }

        CardAbilitiesCombination Combination { get; }

        CardAbilityData CardAbilityData { get; }

        ICardAbilityView AbilityView { get; }

        CardAbilityData.TriggerInfo MainTrigger { get; }

        int TurnsOnBoard { get; }

        void Init(
            BoardUnitModel boardUnitModel,
            CardAbilitiesCombination combination,
            CardAbilityData.TriggerInfo trigger,
            CardAbilityData cardAbilityData,
            IReadOnlyList<BoardObject> targets = null,
            ICardAbilityView abilityView = null);
        void DoAction();
        void DoAction(IReadOnlyList<GenericParameter> genericParameters);
        void ChangePlayerOwner(Player player);
        void InsertTargets(IReadOnlyList<BoardObject> targets);
        void IncreaseTurnsOnBoard();
        void Dispose();
    }

    public class CardAbility : ICardAbility
    {
        protected static readonly ILog Log = Logging.GetLog(nameof(CardAbility));

        public event Action AbilityInitialized;

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

        public CardAbilityData CardAbilityData { get; private set; }

        public CardAbilitiesCombination Combination { get; private set; }

        public ICardAbilityView AbilityView { get; private set; }

        public CardAbilityData.TriggerInfo MainTrigger { get; private set; }

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
        }

        public virtual void DoAction() { }

        public virtual void DoAction(IReadOnlyList<GenericParameter> genericParameters) { }

        public virtual void Init(
            BoardUnitModel boardUnitModel,
            CardAbilitiesCombination combination,
            CardAbilityData.TriggerInfo trigger,
            CardAbilityData cardAbilityData,
            IReadOnlyList<BoardObject> targets = null,
            ICardAbilityView abilityView = null)
        {
            UnitModelOwner = boardUnitModel;
            PlayerOwner = boardUnitModel.OwnerPlayer;
            CardAbilityData = cardAbilityData;
            GenericParameters = cardAbilityData.GenericParameters;
            Targets = targets;
            AbilityView = abilityView;
            Combination = combination;
            MainTrigger = trigger;

            AbilityView?.Init(this);

            AbilityInitializedAction();
        }

        public void ChangePlayerOwner(Player player)
        {
            PlayerOwner = player;
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

        public void IncreaseTurnsOnBoard()
        {
            TurnsOnBoard++;
        }

        protected void PostGameActionReport(Enumerators.ActionType actionType, List<PastActionsPopup.TargetEffectParam> targetEffectParams)
        {
            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = actionType,
                Caller = UnitModelOwner,
                TargetEffects = targetEffectParams
            });
        }
    }
}
