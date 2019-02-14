using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
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

            // Entry abilities handling
            WorkingCard workingCard = _testHelper.BattlegroundController.GetWorkingCardByInstanceId(card);

            // First, fire targetable entry abilities
            if (entryAbilityTarget != null)
            {
                entryAbilityTargetBoardObject = _testHelper.BattlegroundController.GetBoardObjectByInstanceId(entryAbilityTarget.Value);
                if (entryAbilityTargetBoardObject == null)
                    throw new Exception($"'Entry ability target with instance ID {entryAbilityTarget.Value}' not found on board");

                AbilityData entryAbility =
                    workingCard.LibraryCard.Abilities
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

            // Second, fire non-targetable entry abilities
            AbilityData[] entryAbilities =
                workingCard.LibraryCard.Abilities
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

        public async Task OverlordSkillUsed(SkillId skillId, InstanceId? target)
        {
            await SendPlayerAction(_client.PlayerActionFactory.OverlordSkillUsed(skillId, target ?? _testHelper.GetOpponentPlayer().InstanceId));
            await new CustomWaitForSeconds(4f);
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
