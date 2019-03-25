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
                List<BoardUnitModel> boardUnitModels = new List<BoardUnitModel>();
                boardUnitModels.AddRange(player.CardsOnBoard.Where(x => x.Card.Prototype.Faction == Faction));
                boardUnitModels.AddRange(player.CardsInHand.Where(x => x.Card.Prototype.Faction == Faction));
                IReadOnlyList<BoardUnitModel> graveyardCards =
                    player.CardsInGraveyard.FindAll(unit =>
                        unit.Prototype.Faction == Faction && !boardUnitModels.Exists(card => card.InstanceId == unit.InstanceId));

                foreach (BoardUnitModel unit in graveyardCards)
                {
                    ReviveUnit(unit);
                }
            }

            Process(GameplayManager.CurrentPlayer);
            Process(GameplayManager.OpponentPlayer);

            GameplayManager.CanDoDragActions = true;
        }

        private void ReviveUnit(BoardUnitModel boardUnitModel)
        {
            Player playerOwner = boardUnitModel.Owner;

            if (playerOwner.CardsOnBoard.Count >= playerOwner.MaxCardsInPlay)
                return;

            playerOwner.PlayerCardsController.RemoveCardFromGraveyard(boardUnitModel);
            boardUnitModel.ResetToInitial();
            BoardUnitModel revivedBoardUnitModel = boardUnitModel;
            BoardUnitView revivedBoardUnitView = BattlegroundController.CreateBoardUnit(playerOwner, revivedBoardUnitModel);

            playerOwner.PlayerCardsController.AddCardToBoard(revivedBoardUnitModel, ItemPosition.End);

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

            RanksController.AddUnitForIgnoreRankBuff(revivedBoardUnitModel);

            BoardController.UpdateCurrentBoardOfPlayer(playerOwner, null);
        }
    }
}
