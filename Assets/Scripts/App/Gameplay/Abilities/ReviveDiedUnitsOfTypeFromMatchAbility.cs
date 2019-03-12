using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReviveDiedUnitsOfTypeFromMatchAbility : AbilityBase
    {
        public Enumerators.SetType SetType;

        public ReviveDiedUnitsOfTypeFromMatchAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            IReadOnlyList<BoardUnitModel> playerGraveyardCards =
                GameplayManager.CurrentPlayer.CardsInGraveyard.FindAll(x => x.Prototype.CardSetType == SetType);

            IReadOnlyList<BoardUnitModel> playerBoardCards =
                GameplayManager.CurrentPlayer.CardsOnBoard.FindAll(x => x.Card.Prototype.CardSetType == SetType);

            foreach (BoardUnitModel unit in playerGraveyardCards)
            {
                bool isInPlay = false;
                foreach (BoardUnitModel model in playerBoardCards)
                {
                    if (model.InstanceId == unit.InstanceId)
                    {
                        isInPlay = true;
                        break;
                    }
                }
                if (!isInPlay) {
                    ReviveUnit(unit);
                }
            }

            playerGraveyardCards = GameplayManager.OpponentPlayer.CardsInGraveyard.FindAll(x => x.Prototype.CardSetType == SetType);

            IReadOnlyList<BoardUnitModel> opponentBoardCards =
                GameplayManager.OpponentPlayer.CardsOnBoard.FindAll(x => x.Card.Prototype.CardSetType == SetType);

            foreach (BoardUnitModel unit in playerGraveyardCards)
            {
                bool isInPlay = false;
                foreach (BoardUnitModel model in opponentBoardCards)
                {
                    if (model.InstanceId == unit.InstanceId)
                    {
                        isInPlay = true;
                        break;
                    }
                }
                if (!isInPlay) {
                    ReviveUnit(unit);
                }
            }

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

            playerOwner.RemoveCardFromGraveyard(revivedBoardUnitModel);
            playerOwner.AddCardToBoard(revivedBoardUnitModel, ItemPosition.End);
            playerOwner.BoardCards.Insert(ItemPosition.End, revivedBoardUnitView);

            if (playerOwner.IsLocalPlayer)
            {
                GameplayManager.CurrentPlayer.BoardCards.Insert(ItemPosition.End, revivedBoardUnitView);
            }
            else
            {
                GameplayManager.OpponentPlayer.BoardCards.Insert(ItemPosition.End, revivedBoardUnitView);
            }

            RanksController.AddUnitForIgnoreRankBuff(revivedBoardUnitModel);

            BoardController.UpdateCurrentBoardOfPlayer(playerOwner, null);
        }
    }
}
