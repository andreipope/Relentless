using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReanimateAbility : AbilityBase
    {
        private IGameplayManager _gameplayManager;

        private AbilitiesController _abilitiesController;

        private BoardUnitView _reanimatedUnit;

        public ReanimateAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (!AbilityUnitOwner.IsReanimated)
            {
                AbilityUnitOwner.AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescriptionType.Reanimate);
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (PvPManager.UseBackendGameLogic)
                return;

            if (AbilityUnitOwner.IsReanimated)
                return;

            Player owner = AbilityUnitOwner.OwnerPlayer;
            _reanimatedUnit = CreateBoardUnit(AbilityUnitOwner.Card, owner);
            _reanimatedUnit.Model.IsReanimated = true;

            if (owner.CardsInGraveyard.Contains(AbilityUnitOwner.Card))
            {
                owner.CardsInGraveyard.Remove(AbilityUnitOwner.Card);
            }

            owner.AddCardToBoard(AbilityUnitOwner.Card, ItemPosition.End);
            owner.BoardCards.Insert(ItemPosition.End, _reanimatedUnit);

            if (owner.IsLocalPlayer)
            {
                BattlegroundController.PlayerBoardCards.Insert(ItemPosition.End, _reanimatedUnit);
                _abilitiesController.ActivateAbilitiesOnCard(_reanimatedUnit.Model, AbilityUnitOwner.Card, owner);
            }
            else
            {
                BattlegroundController.OpponentBoardCards.Insert(ItemPosition.End, _reanimatedUnit);
            }

            InvokeActionTriggered(_reanimatedUnit);
        }

        protected override void UnitHpChangedHandler(int oldValue, int newValue)
        {
            base.UnitHpChangedHandler(oldValue, newValue);

            if (AbilityUnitOwner.CurrentHp == 0 && !AbilityUnitOwner.IsReanimated)
            {
                AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue: true);
            }
        }

        protected override void UnitDiedHandler()
        {
            Action();
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            AbilityProcessingAction?.ForceActionDone();

            base.UnitDiedHandler();

            if (_reanimatedUnit != null)
            {
                _reanimatedUnit.Model.RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescriptionType.Reanimate);
            }
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
            boardUnitView.StopSleepingParticles();

            GameplayManager.CanDoDragActions = true;

            return boardUnitView;
        }
    }
}
