using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground.Test
{
    public interface IPlayerActionTestProxy
    {
        Task EndTurn();
        Task LeaveMatch();
        Task Mulligan(IEnumerable<InstanceId> cards);
        Task CardPlay(InstanceId card, ItemPosition position, InstanceId? entryAbilityTarget = null);
        Task RankBuff(InstanceId card, IEnumerable<InstanceId> units);

        Task CardAbilityUsed(
            InstanceId card,
            int abilityIndex,
            int choosableAbilityIndex = 0,
            IReadOnlyList<ParametrizedAbilityInstanceId> targets = null
        );

        Task OverlordSkillUsed(SkillId skillId, Enumerators.AffectObjectType affectObjectType, InstanceId targetInstanceId);
        Task CardAttack(InstanceId attacker, Enumerators.AffectObjectType type, InstanceId target);

        Task CheatDestroyCardsOnBoard(IEnumerable<Data.InstanceId> targets);

        Task<bool> GetIsCurrentTurn();
    }
}
