// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class SummonsAbility : AbilityBase
    {
        public Enumerators.StatType statType;
        public int value = 1;
        private GameObject _boardCreaturePrefab;


        public SummonsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.statType = ability.abilityStatType;
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
            _boardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/BoardCreature");
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();
            if (abilityCallType != Enumerators.AbilityCallType.TURN_START || !_gameplayManager.CurrentTurnPlayer.Equals(playerCallerOfAbility))
                return;

            if (playerCallerOfAbility.BoardCards.Count >= Constants.MAX_BOARD_CREATURES)
                return;

            var libraryCard = _dataManager.CachedCardsLibraryData.GetCard(value);

            string cardSetName = _cardsController.GetSetOfCard(libraryCard);
            var card = new WorkingCard(libraryCard, playerCallerOfAbility);
            var unit = CreateBoardUnit(card, cardSetName);

            playerCallerOfAbility.AddCardToBoard(card);
            playerCallerOfAbility.BoardCards.Add(unit);
            _battlegroundController.playerBoardCards.Add(unit);

            if (!playerCallerOfAbility.IsLocalPlayer) _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
            else _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer();

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.SUMMON_UNIT_CARD, new object[]
            {
                playerCallerOfAbility,
                unit
            }));
        }

        private BoardUnit CreateBoardUnit(WorkingCard card, string cardSetName)
        {
            var cardObject = GameObject.Instantiate(_boardCreaturePrefab);

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