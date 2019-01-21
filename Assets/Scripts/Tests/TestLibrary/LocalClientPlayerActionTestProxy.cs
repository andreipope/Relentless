using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
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

        public LocalClientPlayerActionTestProxy(TestHelper testHelper)
        {
            _testHelper = testHelper;
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

        public Task RankBuff(WorkingCard card, IEnumerable<InstanceId> units)
        {
            throw new InvalidOperationException("Doesn't makes sense for local player - sent automatically by the local player");
        }

        public Task CardAbilityUsed(
            WorkingCard card,
            Enumerators.AbilityType abilityType,
            Enumerators.CardKind cardKind,
            Enumerators.AffectObjectType affectObjectType,
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

        public Task<bool> GetIsCurrentTurn()
        {
            throw new NotSupportedException();
        }
    }
}
