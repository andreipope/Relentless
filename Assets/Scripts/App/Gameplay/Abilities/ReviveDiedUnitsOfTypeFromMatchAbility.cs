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

            List<BoardUnitModel> cards = new List<BoardUnitModel>();
            cards.AddRange(GameplayManager.CurrentPlayer.BoardCards
                    .FindAll(x => x.Model.Card.Prototype.Faction == Faction)
                    .Select(boardCard => boardCard.Model));
            cards.AddRange(GameplayManager.CurrentPlayer.CardsInHand
                    .FindAll(x => x.Prototype.Faction == Faction));

            IReadOnlyList<BoardUnitModel> units =
                GameplayManager.CurrentPlayer.CardsInGraveyard.FindAll(unit => unit.Prototype.Faction == Faction &&
                    !cards.Exists(card => card.InstanceId == unit.InstanceId));

            foreach (BoardUnitModel unit in units)
            {
                ReviveUnit(unit);
            }

            cards.Clear();
            cards.AddRange(GameplayManager.OpponentPlayer.BoardCards
                    .FindAll(x => x.Model.Card.Prototype.Faction == Faction)
                    .Select(boardCard => boardCard.Model));
            cards.AddRange(GameplayManager.OpponentPlayer.CardsInHand
                    .FindAll(x => x.Prototype.Faction == Faction));

            units = GameplayManager.OpponentPlayer.CardsInGraveyard.FindAll(unit => unit.Prototype.Faction == Faction &&
                    !cards.Exists(card => card.InstanceId == unit.InstanceId));


            foreach (BoardUnitModel unit in units)
            {
                ReviveUnit(unit);
            }

            GameplayManager.CanDoDragActions = true;
        }

        private void ReviveUnit(BoardUnitModel boardUnitModel)
        {
            Player playerOwner = boardUnitModel.Owner;

            if (playerOwner.BoardCards.Count >= playerOwner.MaxCardsInPlay)
                return;

            Card prototype = new Card(boardUnitModel.Prototype);

            WorkingCard card = new WorkingCard(prototype, prototype, playerOwner);
            BoardUnitModel revivedBoardUnitModel = new BoardUnitModel(card);
            BoardUnitView revivedBoardUnitView = BattlegroundController.CreateBoardUnit(playerOwner, revivedBoardUnitModel);

            playerOwner.RemoveCardFromGraveyard(revivedBoardUnitModel);
            playerOwner.AddCardToBoard(revivedBoardUnitModel, ItemPosition.End);
            playerOwner.BoardCards.Insert(ItemPosition.End, revivedBoardUnitView);

            if (playerOwner.IsLocalPlayer)
            {
                BattlegroundController.PlayerBoardCards.Insert(ItemPosition.End, revivedBoardUnitView);
            }
            else
            {
                BattlegroundController.OpponentBoardCards.Insert(ItemPosition.End, revivedBoardUnitView);
            }

            RanksController.AddUnitForIgnoreRankBuff(revivedBoardUnitView);

            BoardController.UpdateCurrentBoardOfPlayer(playerOwner, null);
        }
    }
}
