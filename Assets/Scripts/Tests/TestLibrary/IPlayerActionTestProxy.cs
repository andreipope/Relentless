using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using NUnit.Framework;

namespace Loom.ZombieBattleground.Test
{
    public interface IPlayerActionTestProxy
    {
        Task EndTurn();
        Task LeaveMatch();
        Task Mulligan(IEnumerable<InstanceId> cards);
        Task CardPlay(InstanceId card, ItemPosition position, InstanceId? entryAbilityTarget = null, bool skipEntryAbilities = false, bool forceSkipForPlayerToo = false);
        Task RankBuff(InstanceId card, IEnumerable<InstanceId> units);

        Task CardAbilityUsed(
            InstanceId card,
            Enumerators.AbilityType abilityType,
            IReadOnlyList<ParametrizedAbilityInstanceId> targets = null
        );

        Task OverlordSkillUsed(SkillId skillId, IReadOnlyList<ParametrizedAbilityInstanceId> targets = null);
        Task CardAttack(InstanceId attacker, InstanceId target);

        Task CheatDestroyCardsOnBoard(IEnumerable<InstanceId> targets);

        Task<bool> GetIsCurrentTurn();

        Task LetsThink(
            float thinkTime = 1f,
            bool forceRealtime = false
        );


        Task AssertInQueue(Action action);
    }
}
