using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using InstanceId = Loom.ZombieBattleground.Data.InstanceId;
using NotImplementedException = System.NotImplementedException;

namespace Loom.ZombieBattleground.Test
{
    /// <summary>
    /// Initiates gameplay actions on the local client.
    /// </summary>
    public class LocalClientPlayerActionTestProxy : IPlayerActionTestProxy
    {
        private readonly TestHelper _testHelper;
        private readonly IPvPManager _pvpManager;
        private readonly IQueueManager _queueManager;
        private readonly BackendDataControlMediator _backendDataControlMediator;

        public LocalClientPlayerActionTestProxy(TestHelper testHelper)
        {
            _testHelper = testHelper;

            _pvpManager = GameClient.Get<IPvPManager>();
            _queueManager = GameClient.Get<IQueueManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        public async Task EndTurn()
        {
            await _testHelper.EndTurn();
        }

        public Task LeaveMatch()
        {
            throw new NotImplementedException();
        }

        public Task Mulligan(IEnumerable<InstanceId> cards)
        {
            throw new NotImplementedException();
        }

        public async Task CardPlay(InstanceId card, int position)
        {
            WorkingCard workingCard = _testHelper.GetCardInHandByInstanceId(card, Enumerators.MatchPlayer.CurrentPlayer);
            await _testHelper.PlayCardFromHandToBoard(workingCard);
        }

        public Task RankBuff(InstanceId card, IEnumerable<InstanceId> units)
        {
            throw new InvalidOperationException("Doesn't makes sense for local player - sent automatically by the local player");
        }

        public Task CardAbilityUsed(
            InstanceId card,
            Enumerators.AbilityType abilityType,
            Enumerators.CardKind cardKind,
            IReadOnlyList<ParametrizedAbilityBoardObject> targets = null,
            IEnumerable<InstanceId> cards = null)
        {
            throw new NotImplementedException("Doesn't makes sense for local player - sent automatically as part of card play");
        }

        public Task OverlordSkillUsed(SkillId skillId, Enumerators.AffectObjectType affectObjectType, InstanceId targetInstanceId)
        {
            throw new NotImplementedException();
        }

        public Task CardAttack(InstanceId attacker, Enumerators.AffectObjectType type, InstanceId target)
        {
            BoardUnitModel boardUnitModel = _testHelper.GetCardOnBoardByInstanceId(attacker, Enumerators.MatchPlayer.CurrentPlayer).Model;
            boardUnitModel.DoCombat(_testHelper.BattlegroundController.GetTargetById(target, type));

            return Task.CompletedTask;
        }

        public async Task CheatDestroyCardsOnBoard(IEnumerable<Data.InstanceId> targets)
        {
            MatchRequestFactory matchRequestFactory = new MatchRequestFactory(_pvpManager.MatchMetadata.Id);
            PlayerActionFactory playerActionFactory = new PlayerActionFactory(_backendDataControlMediator.UserDataModel.UserId);
            PlayerAction action = playerActionFactory.CheatDestroyCardsOnBoard(targets);
            _queueManager.AddAction(matchRequestFactory.CreateAction(action));
        }

        public Task<bool> GetIsCurrentTurn()
        {
            throw new NotSupportedException();
        }
    }
}
