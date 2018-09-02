using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class SummonsAbility : AbilityBase
    {
        public int count;

        public string name;

        public List<Enumerators.AbilityTargetType> targetTypes;

        public SummonsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            name = ability.name;
            count = ability.count;
            targetTypes = ability.abilityTargetTypes;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            if (abilityCallType != Enumerators.AbilityCallType.ENTRY)

                return;

            Action();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            foreach (Enumerators.AbilityTargetType target in targetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT:
                        for (int i = 0; i < count; i++)
                        {
                            SpawnMinion(GetOpponentOverlord());
                        }

                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        for (int i = 0; i < count; i++)
                        {
                            SpawnMinion(playerCallerOfAbility);
                        }

                        break;
                    default: continue;
                }
            }
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();

            if ((abilityCallType != Enumerators.AbilityCallType.TURN) || !_gameplayManager.CurrentTurnPlayer.Equals(playerCallerOfAbility))

                return;

            Action();
        }

        private void SpawnMinion(Player owner)
        {
            if (owner.BoardCards.Count >= Constants.MAX_BOARD_UNITS)

                return;

            Card libraryCard = _dataManager.CachedCardsLibraryData.GetCardFromName(name).Clone();

            string cardSetName = _cardsController.GetSetOfCard(libraryCard);
            WorkingCard card = new WorkingCard(libraryCard, owner);
            BoardUnit unit = CreateBoardUnit(card, cardSetName, owner);

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

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.SUMMON_UNIT_CARD, new object[] { owner, unit }));
        }

        private BoardUnit CreateBoardUnit(WorkingCard card, string cardSetName, Player owner)
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
