
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Deck = Loom.ZombieBattleground.Data.Deck;

namespace Loom.ZombieBattleground
{
    public interface IPvPManager
    {
        event Action MatchCreatedActionReceived;
        event Action MatchingStartedActionReceived;
        event Action PlayerLeftGameActionReceived;
        event Action MatchingFailed;

        event Action GameStartedActionReceived;
        event Action GameEndedActionReceived;

        event Action EndTurnActionReceived;
        event Action<PlayerActionCardPlay> CardPlayedActionReceived;
        event Action<PlayerActionCardAttack> CardAttackedActionReceived;
        event Action<PlayerActionOverlordSkillUsed> OverlordSkillUsedActionReceived;
        event Action<PlayerActionCardAbilityUsed> CardAbilityUsedActionReceived;
        event Action<PlayerActionMulligan> MulliganProcessUsedActionReceived;
        event Action<PlayerActionDrawCard> DrawCardActionReceived;
        event Action<PlayerActionRankBuff> RankBuffActionReceived;

        event Action LeaveMatchReceived;

        Address? CustomGameModeAddress { get; set; }

        MatchMetadata MatchMetadata { get; }

        GameState InitialGameState { get; }

        string GetOpponentUserId();

        bool IsCurrentPlayer();

        Task<bool> FindMatch();
        Task<bool> DebugFindMatch(Deck deck);

        Task CancelFindMatch();

        WorkingCard GetWorkingCardFromCardInstance(CardInstance cardInstance, Player ownerPlayer);
    }
}
