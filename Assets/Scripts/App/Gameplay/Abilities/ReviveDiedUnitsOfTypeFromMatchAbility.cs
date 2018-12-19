using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

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

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Character);

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<WorkingCard> units =
                GameplayManager.CurrentPlayer.CardsInGraveyard.FindAll(x => x.LibraryCard.CardSetType == SetType);

            foreach (WorkingCard unit in units)
            {
                ReviveUnit(unit);
            }

            units = GameplayManager.OpponentPlayer.CardsInGraveyard.FindAll(x => x.LibraryCard.CardSetType == SetType);

            foreach (WorkingCard unit in units)
            {
                ReviveUnit(unit);
            }
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
            playerOwner.AddCardToBoard(card);
            playerOwner.BoardCards.Add(unit);

            if (playerOwner.IsLocalPlayer)
            {
                BattlegroundController.PlayerBoardCards.Add(unit);
                BattlegroundController.UpdatePositionOfBoardUnitsOfPlayer(GameplayManager.CurrentPlayer.BoardCards);
            }
            else
            {
                BattlegroundController.OpponentBoardCards.Add(unit);
                BattlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
            }
        }
    }
}
