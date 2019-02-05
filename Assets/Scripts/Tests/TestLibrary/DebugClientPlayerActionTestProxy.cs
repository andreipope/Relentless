using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task CardPlay(InstanceId card, ItemPosition position, InstanceId? entryAbilityTarget = null)
        {
            await SendPlayerAction(_client.PlayerActionFactory.CardPlay(card, position.GetIndex(int.MaxValue)));

            BoardObject entryAbilityTargetBoardObject = null;
            if (entryAbilityTarget != null)
            {
                entryAbilityTargetBoardObject = _testHelper.BattlegroundController.GetBoardObjectById(entryAbilityTarget.Value);
                if (entryAbilityTargetBoardObject == null)
                    throw new Exception($"'Entry ability target with instance ID {entryAbilityTarget.Value}' not found on board");

                WorkingCard workingCard = _testHelper.BattlegroundController.GetWorkingCardById(card);
                AbilityData entryAbility =
                    workingCard.LibraryCard.Abilities
                    .FirstOrDefault(x => _testHelper.AbilitiesController.IsAbilityCanActivateTargetAtStart(x));

                if (entryAbility == null)
                    throw new Exception("not entry ability found");

                Enumerators.AbilityType abilityType = entryAbility.AbilityType;
                await SendPlayerAction(_client.PlayerActionFactory.CardAbilityUsed(
                    card,
                    abilityType,
                    new []{new ParametrizedAbilityInstanceId(entryAbilityTarget.Value) }
                    ));
            }
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

        public async Task OverlordSkillUsed(SkillId skillId, InstanceId targetInstanceId)
        {
            await SendPlayerAction(_client.PlayerActionFactory.OverlordSkillUsed(skillId, targetInstanceId));
        }

        public async Task CardAttack(InstanceId attacker, InstanceId target)
        {
            await SendPlayerAction(_client.PlayerActionFactory.CardAttack(attacker, target));
        }

        public async Task CheatDestroyCardsOnBoard(IEnumerable<InstanceId> targets)
        {
            await SendPlayerAction(_client.PlayerActionFactory.CheatDestroyCardsOnBoard(targets));
        }

        public async Task<bool> GetIsCurrentTurn()
        {
            if (_client.BackendFacade == null)
                return false;

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
