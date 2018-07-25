// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;
using System.Collections.Generic;

namespace LoomNetwork.CZB
{
    public class SummonsAbility : AbilityBase
    {
        public int count;
        public string name;
        public List<Enumerators.AbilityTargetType> targetTypes;


        public SummonsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.name = ability.name;
            this.count = ability.count;
            this.targetTypes = ability.abilityTargetTypes;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            if (abilityCallType != Enumerators.AbilityCallType.AT_START)
                return;

            Action();
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

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (playerCallerOfAbility.BoardCards.Count >= Constants.MAX_BOARD_CREATURES)
                return;

            foreach (var target in targetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT:
                        for (int i = 0; i < count; i++)
                            SpawnMinion(GetOpponentOverlord());
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        for (int i = 0; i < count; i++)
                            SpawnMinion(playerCallerOfAbility);
                        break;
                    default: continue;
                }
            }
        }

        private void SpawnMinion(Player owner)
        {
            var libraryCard = _dataManager.CachedCardsLibraryData.GetCardFromName(name).Clone();

            string cardSetName = _cardsController.GetSetOfCard(libraryCard);
            var card = new WorkingCard(libraryCard, owner);
            var unit = CreateBoardUnit(card, cardSetName, owner);

            owner.AddCardToBoard(card);
            owner.BoardCards.Add(unit);               

            if (!owner.IsLocalPlayer)
            {
                _battlegroundController.opponentBoardCards.Add(unit);
                _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
            }
            else
            {
                _battlegroundController.playerBoardCards.Add(unit);
                _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer();
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.SUMMON_UNIT_CARD, new object[]
            {
                owner,
                unit
            }));
        }

        private BoardUnit CreateBoardUnit(WorkingCard card, string cardSetName, Player owner)
        {
            var cardObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/BoardCreature"));

            GameObject _playerBoard = owner.IsLocalPlayer ? _battlegroundController.playerBoardObject : _battlegroundController.opponentBoardObject;

            var boardUnit = new BoardUnit(_playerBoard.transform);
            boardUnit.transform.tag = owner.IsLocalPlayer ? Constants.TAG_PLAYER_OWNED : Constants.TAG_OPPONENT_OWNED;
            boardUnit.transform.parent = _playerBoard.transform;
            boardUnit.transform.position = new Vector2(1.9f * owner.BoardCards.Count, 0);
            boardUnit.ownerPlayer = owner;
            boardUnit.SetObjectInfo(card, cardSetName);

            boardUnit.PlayArrivalAnimation();

            return boardUnit;
        }
    }
}