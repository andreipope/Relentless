using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReanimateAbility : AbilityBase
    {
        private IGameplayManager _gameplayManager;

        private AbilitiesController _abilitiesController;

        private BoardUnitView _reanimatedUnit;

        public ReanimateAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
        }

        public override void Activate()
        {
            base.Activate();

            if (!AbilityUnitOwner.IsReanimated)
            {
                InvokeUseAbilityEvent();

                AbilityUnitOwner.AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Reanimate);
            }
            else
            {
                base.Deactivate();
                base.Dispose();
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (PvPManager.UseBackendGameLogic || AbilityUnitOwner.IsReanimated)
            {
                AbilityProcessingAction?.TriggerActionExternally();
                return;
            }

            Player owner = AbilityUnitOwner.OwnerPlayer;

            int CardOnBoard = owner.PlayerCardsController.GetCardsOnBoardCount(true);
            if (CardOnBoard >= owner.MaxCardsInPlay)
            {
                AbilityProcessingAction?.TriggerActionExternally();
                return;
            }

            owner.PlayerCardsController.RemoveCardFromGraveyard(AbilityUnitOwner);

            AbilityUnitOwner.ResetToInitial();

            Card prototype = new Card(DataManager.CachedCardsLibraryData.GetCardFromName(AbilityUnitOwner.Card.Prototype.Name));
            InstanceId updatedId = new InstanceId(AbilityUnitOwner.InstanceId.Id, Enumerators.ReasonForInstanceIdChange.Reanimate);
            WorkingCard card = new WorkingCard(prototype, prototype, owner, id: updatedId);
            CardModel reanimatedUnitModel = new CardModel(card);
            reanimatedUnitModel.IsReanimated = true;

            _reanimatedUnit = CreateBoardUnit(reanimatedUnitModel, owner);
            BattlegroundController.RegisterCardView(_reanimatedUnit, reanimatedUnitModel.OwnerPlayer);

            if (reanimatedUnitModel != null)
            {
                reanimatedUnitModel.RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Reanimate);
            }

            _abilitiesController.ResolveAllAbilitiesOnUnit(reanimatedUnitModel, false);

            if (PlayerCallerOfAbility.IsLocalPlayer)
            {
                _abilitiesController.ActivateAbilitiesOnCard(reanimatedUnitModel, reanimatedUnitModel, reanimatedUnitModel.Owner);
            }
            else
            {
                if (_gameplayManager.IsLocalPlayerTurn()) {
                    _abilitiesController.ActivateAbilitiesOnCard(reanimatedUnitModel, reanimatedUnitModel, reanimatedUnitModel.Owner);
                }
            }

            _abilitiesController.ResolveAllAbilitiesOnUnit(reanimatedUnitModel);

            reanimatedUnitModel.Owner.PlayerCardsController.AddCardToBoard(reanimatedUnitModel, ItemPosition.End);

            InvokeActionTriggered(_reanimatedUnit);
        }

        protected override void UnitHpChangedHandler(int oldValue, int newValue)
        {
            base.UnitHpChangedHandler(oldValue, newValue);

            if (AbilityUnitOwner.CurrentDefense == 0 && !AbilityUnitOwner.IsReanimated)
            {
                AbilityProcessingAction?.TriggerActionExternally();
                AbilityProcessingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue: true);
            }
        }

        protected override void UnitDiedHandler()
        {
            Action();
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            AbilityProcessingAction?.TriggerActionExternally();

            base.UnitDiedHandler();

            _gameplayManager.CanDoDragActions = true;
        }

        private BoardUnitView CreateBoardUnit(CardModel cardModel, Player owner)
        {
            BoardUnitView boardUnitView = BattlegroundController.CreateBoardUnit(owner, cardModel);

            if (owner != GameplayManager.CurrentTurnPlayer)
            {
                boardUnitView.Model.IsPlayable = true;
            }

            GameplayManager.CanDoDragActions = true;

            return boardUnitView;
        }
    }
}
