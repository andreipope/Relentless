using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
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

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Character);

            if (!AbilityUnitOwner.IsReanimated)
            {
                AbilityUnitOwner.AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.Reanimate);
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (AbilityUnitOwner.IsReanimated)
                return;

            Player owner = AbilityUnitOwner.OwnerPlayer;
            Card libraryCard = new Card(AbilityUnitOwner.Card.LibraryCard);
            WorkingCard card = new WorkingCard(libraryCard, libraryCard, owner);
            BoardUnitView unit = CreateBoardUnit(card, owner);
            AbilityUnitOwner.IsReanimated = true;

            owner.AddCardToBoard(card, ItemPosition.End);
            owner.BoardCards.Insert(ItemPosition.End, unit);

            if (owner.IsLocalPlayer)
            {
                BattlegroundController.PlayerBoardCards.Insert(ItemPosition.End, unit);
            }
            else
            {
                BattlegroundController.OpponentBoardCards.Insert(ItemPosition.End, unit);
            }

            BoardController.UpdateCurrentBoardOfPlayer(owner, null);

            InvokeActionTriggered(unit);
        }

        protected override void UnitDiedHandler()
        {
            Action();
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Action();

            base.UnitDiedHandler();
        }

        private BoardUnitView CreateBoardUnit(WorkingCard card, Player owner)
        {
            GameObject playerBoard = owner.IsLocalPlayer ?
                BattlegroundController.PlayerBoardObject :
                BattlegroundController.OpponentBoardObject;

            BoardUnitView boardUnitView = new BoardUnitView(new BoardUnitModel(card), playerBoard.transform);
            boardUnitView.Transform.tag = owner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.parent = playerBoard.transform;
            boardUnitView.Transform.position = new Vector2(2f * owner.BoardCards.Count, owner.IsLocalPlayer ? -1.66f : 1.66f);

            if (!owner.Equals(GameplayManager.CurrentTurnPlayer))
            {
                boardUnitView.Model.IsPlayable = true;
            }

            boardUnitView.PlayArrivalAnimation();

            GameplayManager.CanDoDragActions = true;

            return boardUnitView;
        }
    }
}
