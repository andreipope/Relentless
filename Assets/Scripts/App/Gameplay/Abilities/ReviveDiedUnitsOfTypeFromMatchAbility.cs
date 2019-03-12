using System.Collections.Generic;
using System.Linq;
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

            List<WorkingCard> cards = new List<WorkingCard>();
            cards.AddRange(GameplayManager.CurrentPlayer.BoardCards
                    .FindAll(x => x.Model.Card.Prototype.CardSetType == SetType)
                    .Select(boardCard => boardCard.Model.Card));
            cards.AddRange(GameplayManager.CurrentPlayer.CardsInHand
                    .FindAll(x => x.Prototype.CardSetType == SetType));

            IReadOnlyList<WorkingCard> units =
                GameplayManager.CurrentPlayer.CardsInGraveyard.FindAll(unit => unit.Prototype.CardSetType == SetType &&
                    !cards.Exists(card => card.InstanceId == unit.InstanceId));

            foreach (WorkingCard unit in units)
            {
                ReviveUnit(unit);
            }

            units = GameplayManager.OpponentPlayer.CardsInGraveyard.FindAll(x => x.Prototype.CardSetType == SetType);

            cards.Clear();
            cards.AddRange(GameplayManager.OpponentPlayer.BoardCards
                    .FindAll(x => x.Model.Card.Prototype.CardSetType == SetType)
                    .Select(boardCard => boardCard.Model.Card));
            cards.AddRange(GameplayManager.OpponentPlayer.CardsInHand
                    .FindAll(x => x.Prototype.CardSetType == SetType));

            units = GameplayManager.OpponentPlayer.CardsInGraveyard.FindAll(unit => unit.Prototype.CardSetType == SetType &&
                    !cards.Exists(card => card.InstanceId == unit.InstanceId));


            foreach (WorkingCard unit in units)
            {
                ReviveUnit(unit);
            }

            GameplayManager.CanDoDragActions = true;
        }

        private void ReviveUnit(WorkingCard workingCard)
        {
            Player playerOwner = workingCard.Owner;

            if (playerOwner.BoardCards.Count >= playerOwner.MaxCardsInPlay)
                return;

            Card prototype = new Card(workingCard.Prototype);

            WorkingCard card = new WorkingCard(prototype, prototype, playerOwner);
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
