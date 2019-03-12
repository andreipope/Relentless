using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using NUnit.Framework;

namespace Loom.ZombieBattleground.Test
{
    /// <summary>
    /// Convenience wrapper for <see cref="IPlayerActionTestProxy"/> that wraps calls into lambdas and adds the
    /// to a queue to be called in a delayed fashion.
    /// </summary>
    public class QueueProxyPlayerActionTestProxy
    {
        private static readonly ILog Log = Logging.GetLog(nameof(MatchScenarioPlayer));

        private readonly Func<Queue<Func<Task>>> _getQueueFunc;
        private readonly MatchScenarioPlayer _matchScenarioPlayer;

        protected Queue<Func<Task>> Queue => _getQueueFunc();

        /// <summary>
        /// <see cref="IPlayerActionTestProxy"/> to be used for actions .
        /// </summary>
        public IPlayerActionTestProxy Proxy { get; }

        public QueueProxyPlayerActionTestProxy(MatchScenarioPlayer matchScenarioPlayer, Func<Queue<Func<Task>>> queueFunc, IPlayerActionTestProxy proxy)
        {
            _matchScenarioPlayer = matchScenarioPlayer;
            _getQueueFunc = queueFunc;
            Proxy = proxy;
        }

        public void AbortNextTurns()
        {
            _matchScenarioPlayer.AbortNextMoves();
        }

        public void EndTurn()
        {
            Queue.Enqueue(() =>
            {
                LogAction($"{nameof(EndTurn)}()");
                return Proxy.EndTurn();
            });
        }

        public void LeaveMatch()
        {
            Queue.Enqueue(() =>
            {
                LogAction($"{nameof(LeaveMatch)}()");
                return Proxy.LeaveMatch();
            });
        }

        public void Mulligan(IEnumerable<InstanceId> cards)
        {
            Queue.Enqueue(() =>
            {
                LogAction($"{nameof(Mulligan)}({nameof(cards)}: {StringifyInstanceIds(cards)})");
                return Proxy.Mulligan(cards);
            });
        }

        public void CardPlay(InstanceId card, ItemPosition position, InstanceId? entryAbilityTarget = null, bool skipEntryAbilities = false, bool forceSkipForPlayerToo = false)
        {
            Queue.Enqueue(() =>
            {
                LogAction($"{nameof(CardPlay)}({nameof(card)}: {card}, {nameof(position)}: {position}, {nameof(entryAbilityTarget)}: {entryAbilityTarget?.ToString() ?? "null"}, {nameof(skipEntryAbilities)}: {skipEntryAbilities}, {nameof(forceSkipForPlayerToo)}: {forceSkipForPlayerToo})");
                return Proxy.CardPlay(card, position, entryAbilityTarget, skipEntryAbilities);
            });
        }

        public void RankBuff(InstanceId card, IEnumerable<InstanceId> units)
        {
            Queue.Enqueue(() =>
            {
                LogAction($"{nameof(RankBuff)}({nameof(card)}: {card}, {nameof(units)}: {StringifyInstanceIds(units)})");
                return Proxy.RankBuff(card, units);
            });
        }

        public void CardAbilityUsed(
            InstanceId card,
            Enumerators.AbilityType abilityType,
            IReadOnlyList<ParametrizedAbilityInstanceId> targets = null)
        {
            Queue.Enqueue(() =>
            {
                LogAction($"{nameof(CardAbilityUsed)}({nameof(card)}: {card}, {nameof(abilityType)}: {abilityType}), {nameof(targets)}: {StringifyInstanceIds(targets)})");
                return Proxy.CardAbilityUsed(card, abilityType, targets);
            });
        }

        public void OverlordSkillUsed(SkillId skillId, IReadOnlyList<ParametrizedAbilityInstanceId> targets = null)
        {
            Queue.Enqueue(() =>
            {
                LogAction($"{nameof(OverlordSkillUsed)}({nameof(skillId)}: {nameof(targets)}: {StringifyInstanceIds(targets)})");
                return Proxy.OverlordSkillUsed(skillId, targets);
            });
        }

        public void CardAttack(InstanceId attacker, InstanceId target)
        {
            Queue.Enqueue(() =>
            {
                LogAction($"{nameof(CardAttack)}({nameof(attacker)}: {attacker}, {nameof(target)}: {target})");
                return Proxy.CardAttack(attacker, target);
            });
        }

        public void CheatDestroyCardsOnBoard(IEnumerable<InstanceId> targets)
        {
            Queue.Enqueue(() =>
            {
                LogAction($"{nameof(CheatDestroyCardsOnBoard)}({nameof(targets)}: {StringifyInstanceIds(targets)})");
                return Proxy.CheatDestroyCardsOnBoard(targets);
            });
        }

        public void LetsThink(float thinkTime = 1f, bool forceRealtime = false)
        {
            Queue.Enqueue(() =>
            {
                LogAction($"{nameof(LetsThink)}()");
                return Proxy.LetsThink(thinkTime, forceRealtime);
            });
        }

        public void AssertInQueue(Action action)
        {
            Queue.Enqueue(() =>
            {
                LogAction($"{nameof(Assert)}()");
                return Proxy.AssertInQueue(action);
            });
        }

        private void LogAction(string log)
        {
            Log.Info($"{Proxy.GetType().Name}: " + log);
        }

        private static string StringifyInstanceIds(IEnumerable<InstanceId> cards)
        {
            return StringifyList(cards.Select(card => card.Id.ToString()));
        }

        private static string StringifyInstanceIds(IEnumerable<ParametrizedAbilityInstanceId> abilityInstanceIds)
        {
            return StringifyList(abilityInstanceIds.Select(abilityInstanceId => abilityInstanceId.ToString()));
        }

        private static string StringifyList(IEnumerable<string> items)
        {
            return "[" + String.Join(", ", items) + "]";
        }
    }
}
