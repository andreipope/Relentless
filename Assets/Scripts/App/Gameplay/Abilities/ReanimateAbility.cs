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

            if (abilityUnitOwner.IsReanimated)

                return;

            Player owner = abilityUnitOwner.ownerPlayer;
            Card libraryCard = abilityUnitOwner.Card.libraryCard.Clone();
            WorkingCard card = new WorkingCard(libraryCard, owner);
            BoardUnit unit = CreateBoardUnit(card, owner);
            unit.IsReanimated = true;

            owner.AddCardToBoard(card);
            owner.BoardCards.Add(unit);

            if (!owner.IsLocalPlayer)
            {
                _battlegroundController.opponentBoardCards.Add(unit);
                _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
            } else
            {
                _battlegroundController.playerBoardCards.Add(unit);
                _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(_gameplayManager.CurrentPlayer.BoardCards);
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.REANIMATE_UNIT_BY_ABILITY, new object[] { owner, unit }));
        }

        protected override void UnitOnDieEventHandler()
        {
            base.UnitOnDieEventHandler();

            if (abilityCallType != Enumerators.AbilityCallType.DEATH)

                return;

            Action();
        }

        private BoardUnit CreateBoardUnit(WorkingCard card, Player owner)
        {
            GameObject _playerBoard = owner.IsLocalPlayer?_battlegroundController.playerBoardObject:_battlegroundController.opponentBoardObject;

            BoardUnit boardUnit = new BoardUnit(_playerBoard.transform);
            boardUnit.transform.tag = owner.IsLocalPlayer?Constants.TAG_PLAYER_OWNED:Constants.TAG_OPPONENT_OWNED;
            boardUnit.transform.parent = _playerBoard.transform;
            boardUnit.transform.position = new Vector2(2f * owner.BoardCards.Count, owner.IsLocalPlayer?-1.66f:1.66f);
            boardUnit.ownerPlayer = owner;
            boardUnit.SetObjectInfo(card);

            if (!owner.Equals(_gameplayManager.CurrentTurnPlayer))
            {
                boardUnit.IsPlayable = true;
            }

            boardUnit.PlayArrivalAnimation();

            return boardUnit;
        }
    }
}
