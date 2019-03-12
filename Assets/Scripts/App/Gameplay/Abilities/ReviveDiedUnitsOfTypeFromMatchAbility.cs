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

            IReadOnlyList<WorkingCard> units =
                GameplayManager.CurrentPlayer.CardsInGraveyard.FindAll(x => x.LibraryCard.CardSetType == SetType);

            UniquePositionedList<BoardUnitView> playerBoardCards =
                GameplayManager.CurrentPlayer.BoardCards.FindAll(x => x.Model.Card.LibraryCard.CardSetType == SetType);

            foreach (WorkingCard unit in units)
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

            units = GameplayManager.OpponentPlayer.CardsInGraveyard.FindAll(x => x.LibraryCard.CardSetType == SetType);

            UniquePositionedList<BoardUnitView> opponentBoardCards =
                GameplayManager.OpponentPlayer.BoardCards.FindAll(x => x.Model.Card.LibraryCard.CardSetType == SetType);

            foreach (WorkingCard unit in units)
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

        private void ReviveUnit(WorkingCard workingCard)
        {
            Player playerOwner = workingCard.Owner;

            if (playerOwner.BoardCards.Count >= playerOwner.MaxCardsInPlay)
                return;

            Card libraryCard = new Card(workingCard.LibraryCard);

            WorkingCard card = new WorkingCard(libraryCard, libraryCard, playerOwner);
            BoardUnitView unit = BattlegroundController.CreateBoardUnit(playerOwner, card);

            playerOwner.RemoveCardFromGraveyard(workingCard);
            playerOwner.AddCardToBoard(card, ItemPosition.End);
            playerOwner.BoardCards.Insert(ItemPosition.End, unit);

            if (playerOwner.IsLocalPlayer)
            {
                BattlegroundController.PlayerBoardCards.Insert(ItemPosition.End, unit);
            }
            else
            {
                BattlegroundController.OpponentBoardCards.Insert(ItemPosition.End, unit);
            }

            RanksController.AddUnitForIgnoreRankBuff(unit);

            BoardController.UpdateCurrentBoardOfPlayer(playerOwner, null);
        }
    }
}
