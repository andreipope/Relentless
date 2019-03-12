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

            IReadOnlyList<BoardUnitModel> units =
                GameplayManager.CurrentPlayer.CardsInGraveyard.FindAll(x => x.Prototype.CardSetType == SetType);

            IReadOnlyList<BoardUnitView> playerBoardCards =
                GameplayManager.CurrentPlayer.BoardCards.FindAll(x => x.Card.Prototype.CardSetType == SetType);

            foreach (BoardUnitModel unit in units)
            {
                bool isInPlay = false;
                foreach (BoardUnitView view in playerBoardCards) 
                {
                    if (view.Model.InstanceId == unit.InstanceId) 
                    {
                        isInPlay = true;
                        break;
                    }
                }
                if (!isInPlay) {
                    ReviveUnit(unit);
                }
            }

            units = GameplayManager.OpponentPlayer.CardsInGraveyard.FindAll(x => x.Prototype.CardSetType == SetType);

            IReadOnlyList<BoardUnitView> opponentBoardCards =
                GameplayManager.OpponentPlayer.BoardCards.FindAll(x => x.Card.Prototype.CardSetType == SetType);

            foreach (BoardUnitModel unit in units)
            {
                bool isInPlay = false;
                foreach (BoardUnitView view in opponentBoardCards) 
                {
                    if (view.Model.InstanceId == unit.InstanceId) 
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
