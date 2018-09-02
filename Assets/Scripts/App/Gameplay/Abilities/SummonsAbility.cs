using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class SummonsAbility : AbilityBase
    {
        public int Count;

        public string Name;

        public List<Enumerators.AbilityTargetType> TargetTypes;

        public SummonsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Name = ability.Name;
            Count = ability.Count;
            TargetTypes = ability.AbilityTargetTypes;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            if (AbilityCallType != Enumerators.AbilityCallType.Entry)
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

            foreach (Enumerators.AbilityTargetType target in TargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.Opponent:
                        for (int i = 0; i < Count; i++)
                        {
                            SpawnMinion(GetOpponentOverlord());
                        }

                        break;
                    case Enumerators.AbilityTargetType.Player:
                        for (int i = 0; i < Count; i++)
                        {
                            SpawnMinion(PlayerCallerOfAbility);
                        }

                        break;
                    default: continue;
                }
            }
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();

            if ((AbilityCallType != Enumerators.AbilityCallType.Turn) || !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            Action();
        }

        private void SpawnMinion(Player owner)
        {
            if (owner.BoardCards.Count >= Constants.MaxBoardUnits)
                return;

            Card libraryCard = DataManager.CachedCardsLibraryData.GetCardFromName(Name).Clone();

            string cardSetName = CardsController.GetSetOfCard(libraryCard);
            WorkingCard card = new WorkingCard(libraryCard, owner);
            BoardUnit unit = CreateBoardUnit(card, cardSetName, owner);

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

            ActionsQueueController.PostGameActionReport(ActionsQueueController.FormatGameActionReport(Enumerators.ActionType.SummonUnitCard, new object[] { owner, unit }));
        }

        private BoardUnit CreateBoardUnit(WorkingCard card, string cardSetName, Player owner)
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
