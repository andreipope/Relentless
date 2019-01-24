using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using InstanceId = Loom.ZombieBattleground.Data.InstanceId;

namespace Loom.ZombieBattleground.Test
{
    /// <summary>
    /// Sends gameplay actions from the debug opponent client. No logic here, just sends requests.
    /// </summary>
    public class DebugClientPlayerActionTestProxy : IPlayerActionTestProxy
    {
        private readonly TestHelper _testHelper;
        private readonly MultiplayerDebugClient _client;

        public DebugClientPlayerActionTestProxy(TestHelper testHelper, MultiplayerDebugClient client)
        {
            _testHelper = testHelper;
            _client = client;
        }

        public async Task EndTurn()
        {
            await SendPlayerAction(_client.PlayerActionFactory.EndTurn());
        }

        public async Task LeaveMatch()
        {
            await SendPlayerAction(_client.PlayerActionFactory.LeaveMatch());
        }

        public async Task Mulligan(IEnumerable<InstanceId> cards)
        {
            await SendPlayerAction(_client.PlayerActionFactory.Mulligan(cards));
        }

        public async Task CardPlay(InstanceId card, int position, InstanceId? entryAbilityTarget = null)
        {
            if (entryAbilityTarget != null)
                throw new Exception($"'{nameof(entryAbilityTarget)}' not supported for {nameof(DebugClientPlayerActionTestProxy)}. Use CardPlay + CardAbilityUsed instead.");

            await SendPlayerAction(_client.PlayerActionFactory.CardPlay(card, position));
        }

        public async Task RankBuff(InstanceId card, IEnumerable<InstanceId> units)
        {
            await SendPlayerAction(_client.PlayerActionFactory.RankBuff(card, units));
        }

        public async Task CardAbilityUsed(
            InstanceId card,
            Enumerators.AbilityType abilityType,
            IReadOnlyList<ParametrizedAbilityInstanceId> targets = null)
        {
            await SendPlayerAction(_client.PlayerActionFactory.CardAbilityUsed(card, abilityType, targets));
        }

        public async Task OverlordSkillUsed(SkillId skillId, Enumerators.AffectObjectType affectObjectType, InstanceId targetInstanceId)
        {
            await SendPlayerAction(_client.PlayerActionFactory.OverlordSkillUsed(skillId, affectObjectType, targetInstanceId));
        }

        public async Task CardAttack(InstanceId attacker, Enumerators.AffectObjectType type, InstanceId target)
        {
            await SendPlayerAction(_client.PlayerActionFactory.CardAttack(attacker, type, target));
        }

        public async Task CheatDestroyCardsOnBoard(IEnumerable<InstanceId> targets)
        {
            await SendPlayerAction(_client.PlayerActionFactory.CheatDestroyCardsOnBoard(targets));
        }

        public async Task<bool> GetIsCurrentTurn()
        {
            GetGameStateResponse gameStateResponse = await _client.BackendFacade.GetGameState(_client.MatchMakingFlowController.MatchMetadata.Id);
            GameState gameState = gameStateResponse.GameState;
            return gameState.PlayerStates[gameState.CurrentPlayerIndex].Id == _client.UserDataModel.UserId;
        }

        private async Task SendPlayerAction(PlayerAction action)
        {
            await _client.BackendFacade.SendPlayerAction(
                _client.MatchRequestFactory.CreateAction(
                    action
                )
            );
        }
    }
}
