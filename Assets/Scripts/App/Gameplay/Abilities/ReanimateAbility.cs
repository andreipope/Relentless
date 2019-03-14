using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
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

            if (!AbilityUnitOwner.IsReanimated)
            {
                InvokeUseAbilityEvent();

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
            _reanimatedUnit = CreateBoardUnit(AbilityUnitOwner, owner);
            _reanimatedUnit.BoardUnitModel.IsReanimated = true;

            owner.LocalCardsController.RemoveCardFromGraveyard(AbilityUnitOwner);
            owner.LocalCardsController.AddCardToBoard(AbilityUnitOwner, ItemPosition.End);

            if (owner.IsLocalPlayer)
            {
                BattlegroundController.RegisterBoardUnitView(GameplayManager.CurrentPlayer, _reanimatedUnit);
                _abilitiesController.ActivateAbilitiesOnCard(_reanimatedUnit.BoardUnitModel, AbilityUnitOwner, owner);
            }
            else
            {
                BattlegroundController.RegisterBoardUnitView(GameplayManager.OpponentPlayer, _reanimatedUnit);
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
                _reanimatedUnit.BoardUnitModel.RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescriptionType.Reanimate);
            }

            _gameplayManager.CanDoDragActions = true;
        }

        private BoardUnitView CreateBoardUnit(BoardUnitModel boardUnitModel, Player owner)
        {
            GameObject playerBoard = owner.IsLocalPlayer ?
                BattlegroundController.PlayerBoardObject :
                BattlegroundController.OpponentBoardObject;

            BoardUnitView boardUnitView = new BoardUnitView(boardUnitModel, playerBoard.transform);
            boardUnitView.Transform.tag = owner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.parent = playerBoard.transform;
            boardUnitView.Transform.position = new Vector2(2f * owner.CardsOnBoard.Count, owner.IsLocalPlayer ? -1.66f : 1.66f);

            if (!owner.Equals(GameplayManager.CurrentTurnPlayer))
            {
                boardUnitView.BoardUnitModel.IsPlayable = true;
            }

            boardUnitView.PlayArrivalAnimation();
            boardUnitView.StopSleepingParticles();

            GameplayManager.CanDoDragActions = true;

            return boardUnitView;
        }
    }
}
