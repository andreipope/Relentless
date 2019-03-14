using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using NUnit.Framework;
using UnityEngine;
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
            await Task.Delay(3000);
            await SendPlayerAction(_client.PlayerActionFactory.EndTurn());
            await Task.Delay(2000);
        }

        public async Task LeaveMatch()
        {
            await SendPlayerAction(_client.PlayerActionFactory.LeaveMatch());
        }

        public async Task Mulligan(IEnumerable<InstanceId> cards)
        {
            await SendPlayerAction(_client.PlayerActionFactory.Mulligan(cards));
        }

        public async Task CardPlay(InstanceId card, ItemPosition position, InstanceId? entryAbilityTarget = null, bool skipEntryAbilities = false, bool forceSkipForPlayerToo = false)
        {
            await SendPlayerAction(_client.PlayerActionFactory.CardPlay(card, position.GetIndex(int.MaxValue)));

            BoardObject entryAbilityTargetBoardObject = null;

            // Entry abilities handling
            BoardUnitModel boardUnitModel = _testHelper.BattlegroundController.GetBoardUnitModelByInstanceId(card);
            Assert.NotNull(boardUnitModel, $"boardUnitModel != null for instance id {card}");

            // First, fire targetable entry abilities
            if (entryAbilityTarget != null)
            {
                entryAbilityTargetBoardObject = _testHelper.BattlegroundController.GetBoardObjectByInstanceId(entryAbilityTarget.Value);
                if (entryAbilityTargetBoardObject == null)
                    throw new Exception($"'Entry ability target with instance ID {entryAbilityTarget.Value}' not found on board");

                AbilityData entryAbility =
                    boardUnitModel.Prototype.Abilities
                    .FirstOrDefault(x => _testHelper.AbilitiesController.IsAbilityCanActivateTargetAtStart(x));

                if (entryAbility == null)
                    throw new Exception($"No entry ability found for target {entryAbilityTarget}");

                Enumerators.AbilityType abilityType = entryAbility.AbilityType;
                await SendPlayerAction(_client.PlayerActionFactory.CardAbilityUsed(
                    card,
                    abilityType,
                    new []{new ParametrizedAbilityInstanceId(entryAbilityTarget.Value) }
                    ));
            }

            if (skipEntryAbilities)
                return;

            // Second, fire non-targetable entry abilities
            AbilityData[] entryAbilities =
                boardUnitModel.Prototype.Abilities
                    .Where(x =>
                        _testHelper.AbilitiesController.IsAbilityCallsAtStart(x) &&
                        !_testHelper.AbilitiesController.IsAbilityCanActivateTargetAtStart(x))
                    .ToArray();

            foreach (AbilityData entryAbility in entryAbilities)
            {
                await SendPlayerAction(_client.PlayerActionFactory.CardAbilityUsed(
                    card,
                    entryAbility.AbilityType,
                    new ParametrizedAbilityInstanceId[]{ }
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

        public async Task OverlordSkillUsed(SkillId skillId, IReadOnlyList<ParametrizedAbilityInstanceId> targets = null)
        {
            await SendPlayerAction(_client.PlayerActionFactory.OverlordSkillUsed(skillId, targets));
            await new WaitForSeconds(4f);
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

        public async Task LetsThink(float thinkTime, bool forceRealtime)
        {
            await _testHelper.LetsThink(thinkTime, forceRealtime);
        }

        public async Task AssertInQueue(Action action)
        {
           action();
           await new WaitForSeconds(1f);
        }
    }
}
