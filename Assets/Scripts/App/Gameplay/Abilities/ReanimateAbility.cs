using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReanimateAbility : AbilityBase
    {
        public ReanimateAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
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
            AbilityUnitOwner.IsReanimated = true;

            owner.AddCardToBoard(card);
            owner.BoardCards.Add(unit);

            if (!owner.IsLocalPlayer)
            {
                BattlegroundController.OpponentBoardCards.Add(unit);
                BattlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
            }
            else
            {
                BattlegroundController.PlayerBoardCards.Add(unit);
                BattlegroundController.UpdatePositionOfBoardUnitsOfPlayer(GameplayManager.CurrentPlayer.BoardCards);
            }

            ActionsQueueController.PostGameActionReport(ActionsQueueController.FormatGameActionReport(
                Enumerators.ActionType.REANIMATE_UNIT_BY_ABILITY, new object[]
                {
                    owner, unit
                }));
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            Action();
        }

        private BoardUnit CreateBoardUnit(WorkingCard card, Player owner)
        {
            GameObject playerBoard = owner.IsLocalPlayer ?
                BattlegroundController.PlayerBoardObject :
                BattlegroundController.OpponentBoardObject;

            BoardUnit boardUnit = new BoardUnit(playerBoard.transform);
            boardUnit.Transform.tag = owner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnit.Transform.parent = playerBoard.transform;
            boardUnit.Transform.position =
                new Vector2(2f * owner.BoardCards.Count, owner.IsLocalPlayer ? -1.66f : 1.66f);
            boardUnit.OwnerPlayer = owner;
            boardUnit.SetObjectInfo(card);

            if (!owner.Equals(GameplayManager.CurrentTurnPlayer))
            {
                boardUnit.IsPlayable = true;
            }

            boardUnit.PlayArrivalAnimation();

            GameplayManager.CanDoDragActions = true;

            return boardUnit;
        }
    }
}
