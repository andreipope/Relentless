// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ReviveDiedUnitsOfTypeFromMatchAbility : AbilityBase
    {
        public Enumerators.SetType setType;

        public ReviveDiedUnitsOfTypeFromMatchAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            setType = ability.abilitySetType;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.AT_START)
                return;

            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            var units = _gameplayManager.CurrentPlayer.CardsInGraveyard.FindAll(x => x.libraryCard.cardSetType == setType);

            foreach (var unit in units)
                ReviveUnit(unit);

            units = _gameplayManager.OpponentPlayer.CardsInGraveyard.FindAll(x => x.libraryCard.cardSetType == setType);

            foreach (var unit in units)
                ReviveUnit(unit);
        }

        private void ReviveUnit(WorkingCard workingCard)
        {
            var playerOwner = workingCard.owner;

            if (playerOwner.BoardCards.Count >= Constants.MAX_BOARD_UNITS)
                return;

            var libraryCard = workingCard.libraryCard.Clone();

            string cardSetName = _cardsController.GetSetOfCard(libraryCard);
            var card = new WorkingCard(libraryCard, playerOwner);
            var unit = CreateBoardUnit(card, cardSetName);

            playerOwner.RemoveCardFromGraveyard(workingCard);
            playerOwner.AddCardToBoard(card);
            playerOwner.BoardCards.Add(unit);
            _battlegroundController.playerBoardCards.Add(unit);

            if (!playerOwner.IsLocalPlayer) _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
            else _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer();
        }

        private BoardUnit CreateBoardUnit(WorkingCard card, string cardSetName)
        {
            var cardObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/BoardCreature"));

            GameObject _playerBoard = playerCallerOfAbility.IsLocalPlayer ? _battlegroundController.playerBoardObject : _battlegroundController.opponentBoardObject;

            var boardUnit = new BoardUnit(_playerBoard.transform);
            boardUnit.transform.tag = playerCallerOfAbility.IsLocalPlayer ? Constants.TAG_PLAYER_OWNED : Constants.TAG_OPPONENT_OWNED;
            boardUnit.transform.parent = _playerBoard.transform;
            boardUnit.transform.position = new Vector2(1.9f * playerCallerOfAbility.BoardCards.Count, 0);
            boardUnit.ownerPlayer = playerCallerOfAbility;
            boardUnit.SetObjectInfo(card, cardSetName);

            boardUnit.PlayArrivalAnimation();

            return boardUnit;
        }
    }
}
