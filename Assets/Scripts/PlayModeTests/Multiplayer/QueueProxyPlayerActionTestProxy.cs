using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground.Test
{
    internal class QueueProxyPlayerActionTestProxy
    {
        public readonly Queue<Func<Task>> Queue = new Queue<Func<Task>>();
        public IPlayerActionTestProxy CurrentProxy;

        public void EndTurn()
        {
            IPlayerActionTestProxy currentProxy = CurrentProxy;
            Queue.Enqueue(() => currentProxy.EndTurn());
        }

        public void LeaveMatch()
        {
            IPlayerActionTestProxy currentProxy = CurrentProxy;
            Queue.Enqueue(() => currentProxy.LeaveMatch());
        }

        public void Mulligan(IEnumerable<InstanceId> cards)
        {
            IPlayerActionTestProxy currentProxy = CurrentProxy;
            Queue.Enqueue(() => currentProxy.Mulligan(cards));
        }

        public void CardPlay(InstanceId card, int position)
        {
            IPlayerActionTestProxy currentProxy = CurrentProxy;
            Queue.Enqueue(() => currentProxy.CardPlay(card, position));
        }

        public void RankBuff(WorkingCard card, IEnumerable<InstanceId> units)
        {
            IPlayerActionTestProxy currentProxy = CurrentProxy;
            Queue.Enqueue(() => currentProxy.RankBuff(card, units));
        }

        public void CardAbilityUsed(
            WorkingCard card,
            Enumerators.AbilityType abilityType,
            Enumerators.CardKind cardKind,
            Enumerators.AffectObjectType affectObjectType,
            IReadOnlyList<ParametrizedAbilityBoardObject> targets = null,
            IEnumerable<InstanceId> cards = null)
        {
            IPlayerActionTestProxy currentProxy = CurrentProxy;
            Queue.Enqueue(() => currentProxy.CardAbilityUsed(card, abilityType, cardKind, affectObjectType, targets, cards));
        }

        public void OverlordSkillUsed(SkillId skillId, Enumerators.AffectObjectType affectObjectType, InstanceId targetInstanceId)
        {
            IPlayerActionTestProxy currentProxy = CurrentProxy;
            Queue.Enqueue(() => currentProxy.OverlordSkillUsed(skillId, affectObjectType, targetInstanceId));
        }

        public void CardAttack(InstanceId attacker, Enumerators.AffectObjectType type, InstanceId target)
        {
            IPlayerActionTestProxy currentProxy = CurrentProxy;
            Queue.Enqueue(() => currentProxy.CardAttack(attacker, type, target));
        }
    }
}
