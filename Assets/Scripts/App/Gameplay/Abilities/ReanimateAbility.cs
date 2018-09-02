using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ReanimateAbility : AbilityBase
    {
        public ReanimateAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (AbilityUnitOwner.IsReanimated)

                return;

            Player owner = AbilityUnitOwner.OwnerPlayer;
            Card libraryCard = AbilityUnitOwner.Card.LibraryCard.Clone();
            WorkingCard card = new WorkingCard(libraryCard, owner);
            BoardUnit unit = CreateBoardUnit(card, owner);
            unit.IsReanimated = true;

            owner.AddCardToBoard(card);
            owner.BoardCards.Add(unit);

            if (!owner.IsLocalPlayer)
            {
                BattlegroundController.OpponentBoardCards.Add(unit);
                BattlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
            } else
            {
                BattlegroundController.PlayerBoardCards.Add(unit);
                BattlegroundController.UpdatePositionOfBoardUnitsOfPlayer(GameplayManager.CurrentPlayer.BoardCards);
            }

            ActionsQueueController.PostGameActionReport(ActionsQueueController.FormatGameActionReport(Enumerators.ActionType.ReanimateUnitByAbility, new object[] { owner, unit }));
        }

        protected override void UnitOnDieEventHandler()
        {
            base.UnitOnDieEventHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.Death)

                return;

            Action();
        }

        private BoardUnit CreateBoardUnit(WorkingCard card, Player owner)
        {
            GameObject playerBoard = owner.IsLocalPlayer?BattlegroundController.PlayerBoardObject:BattlegroundController.OpponentBoardObject;

            BoardUnit boardUnit = new BoardUnit(playerBoard.transform);
            boardUnit.Transform.tag = owner.IsLocalPlayer?Constants.KTagPlayerOwned:Constants.KTagOpponentOwned;
            boardUnit.Transform.parent = playerBoard.transform;
            boardUnit.Transform.position = new Vector2(2f * owner.BoardCards.Count, owner.IsLocalPlayer?-1.66f:1.66f);
            boardUnit.OwnerPlayer = owner;
            boardUnit.SetObjectInfo(card);

            if (!owner.Equals(GameplayManager.CurrentTurnPlayer))
            {
                boardUnit.IsPlayable = true;
            }

            boardUnit.PlayArrivalAnimation();

            return boardUnit;
        }
    }
}
