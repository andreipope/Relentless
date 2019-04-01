using Loom.ZombieBattleground.Common;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public interface ICardAbility
    {
        BoardUnitModel UnitModelOwner { get; }

        Player PlayerOwner { get; }

        CardAbilitiesCombination Combination { get; }

        CardAbilityData CardAbilityData { get; }

        ICardAbilityView AbilityView { get; }

        void Init(
            BoardUnitModel boardUnitModel,
            CardAbilitiesCombination combination,
            CardAbilityData cardAbilityData,
            IReadOnlyList<BoardObject> targets = null,
            ICardAbilityView abilityView = null);
        void DoAction();
        void DoAction(IReadOnlyList<GenericParameter> genericParameters);
        void ChangePlayerOwner(Player player);
        void InsertTargets(IReadOnlyList<BoardObject> targets);
        void Dispose();
    }

    public class CardAbility : ICardAbility
    {
        protected IReadOnlyList<BoardObject> Targets { get; private set; }

        protected IReadOnlyList<GenericParameter> GenericParameters { get; private set; }

        protected readonly IGameplayManager GameplayManager;

        protected readonly AbilitiesController AbilitiesController;

        protected readonly BattlegroundController BattlegroundController;

        protected readonly BattleController BattleController;

        protected readonly CardsController CardsController;

        protected readonly BoardController BoardController;

        protected readonly ActionsQueueController ActionsQueueController;

        public BoardUnitModel UnitModelOwner { get; private set; }

        public Player PlayerOwner { get; private set; }

        public CardAbilityData CardAbilityData { get; private set; }

        public CardAbilitiesCombination Combination { get; private set; }

        public ICardAbilityView AbilityView { get; private set; }

        public CardAbility()
        {
            GameplayManager = GameClient.Get<IGameplayManager>();
            AbilitiesController = GameplayManager.GetController<AbilitiesController>();
            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
            BattleController = GameplayManager.GetController<BattleController>();
            CardsController = GameplayManager.GetController<CardsController>();
            BoardController = GameplayManager.GetController<BoardController>();
            ActionsQueueController = GameplayManager.GetController<ActionsQueueController>();
        }

        public virtual void DoAction() { }

        public virtual void DoAction(IReadOnlyList<GenericParameter> genericParameters) { }

        public virtual void Init(
            BoardUnitModel boardUnitModel,
            CardAbilitiesCombination combination,
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

            AbilityView?.Init(this);
        }

        public void ChangePlayerOwner(Player player)
        {
            PlayerOwner = player;
        }

        public void Dispose()
        {
            AbilitiesController.EndAbility(this);
        }

        public void InsertTargets(IReadOnlyList<BoardObject> targets)
        {
            Targets = targets;
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
