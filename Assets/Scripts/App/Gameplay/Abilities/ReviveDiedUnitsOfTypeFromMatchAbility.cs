using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReviveDiedUnitsOfTypeFromMatchAbility : AbilityBase
    {
        private IGameplayManager _gameplayManager;

        private AbilitiesController _abilitiesController;
        public Enumerators.Faction Faction;

        public ReviveDiedUnitsOfTypeFromMatchAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Faction = ability.Faction;
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            void Process(Player player)
            {
                List<CardModel> cardModels = new List<CardModel>();
                cardModels.AddRange(player.CardsOnBoard.Where(x => x.Card.Prototype.Faction == Faction));
                cardModels.AddRange(player.CardsInHand.Where(x => x.Card.Prototype.Faction == Faction));
                IReadOnlyList<CardModel> graveyardCards =
                    player.CardsInGraveyard.FindAll(unit =>
                        unit.Prototype.Faction == Faction && !cardModels.Exists(card => card.InstanceId == unit.InstanceId));

                foreach (CardModel unit in graveyardCards)
                {
                    ReviveUnit(unit);
                }
            }

            Process(GameplayManager.CurrentPlayer);
            Process(GameplayManager.OpponentPlayer);

            GameplayManager.CanDoDragActions = true;
        }

        private void ReviveUnit(CardModel cardModel)
        {
            Player playerOwner = cardModel.Owner;

            if (playerOwner.CardsOnBoard.Count >= playerOwner.MaxCardsInPlay)
                return;

            playerOwner.PlayerCardsController.RemoveCardFromGraveyard(cardModel);
            cardModel.ResetToInitial();
            CardModel revivedCardModel = cardModel;
            BoardUnitView revivedBoardUnitView = BattlegroundController.CreateBoardUnit(playerOwner, revivedCardModel);

            playerOwner.PlayerCardsController.AddCardToBoard(revivedCardModel, ItemPosition.End);

            if (playerOwner.IsLocalPlayer)
            {
                BattlegroundController.RegisterBoardUnitView(revivedBoardUnitView, GameplayManager.CurrentPlayer);
                _abilitiesController.ActivateAbilitiesOnCard(revivedBoardUnitView.Model, AbilityUnitOwner, AbilityUnitOwner.Owner);
            }
            else
            {
                BattlegroundController.RegisterBoardUnitView(revivedBoardUnitView, GameplayManager.OpponentPlayer);
                if (_gameplayManager.IsLocalPlayerTurn()) {
                    _abilitiesController.ActivateAbilitiesOnCard(revivedBoardUnitView.Model, AbilityUnitOwner, AbilityUnitOwner.Owner);
                }
            }

            RanksController.AddUnitForIgnoreRankBuff(revivedCardModel);

            BoardController.UpdateCurrentBoardOfPlayer(playerOwner, null);
        }
    }
}
