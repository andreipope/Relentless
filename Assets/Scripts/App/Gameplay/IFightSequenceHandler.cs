using System;

namespace Loom.ZombieBattleground
{
    public interface IFightSequenceHandler
    {
        void HandleAttackPlayer(Action completeCallback, Player targetPlayer, Action hitCallback, Action attackCompleteCallback);
        void HandleAttackCard(Action completeCallback, BoardUnitModel targetCard, Action hitCallback, Action attackCompleteCallback);
    }
}
