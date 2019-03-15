using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReviveDiedUnitsOfTypeFromMatchAbility : AbilityBase
    {
        public Enumerators.Faction Faction;

        public ReviveDiedUnitsOfTypeFromMatchAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Faction = ability.Faction;
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

            Card prototype = new Card(boardUnitModel.Prototype);

            WorkingCard card = new WorkingCard(prototype, prototype, playerOwner);
            BoardUnitModel revivedBoardUnitModel = new BoardUnitModel(card);
            BoardUnitView revivedBoardUnitView = BattlegroundController.CreateBoardUnit(playerOwner, revivedBoardUnitModel);

            playerOwner.PlayerCardsController.RemoveCardFromGraveyard(revivedBoardUnitModel);
            playerOwner.PlayerCardsController.AddCardToBoard(revivedBoardUnitModel, ItemPosition.End);

            if (playerOwner.IsLocalPlayer)
            {
                BattlegroundController.RegisterBoardUnitView(GameplayManager.CurrentPlayer, revivedBoardUnitView);
            }
            else
            {
                BattlegroundController.RegisterBoardUnitView(GameplayManager.OpponentPlayer, revivedBoardUnitView);
            }

            RanksController.AddUnitForIgnoreRankBuff(revivedBoardUnitModel);

            BoardController.UpdateCurrentBoardOfPlayer(playerOwner, null);
        }
    }
}
