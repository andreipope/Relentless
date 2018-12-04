
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
        event Action<PlayerActionOutcome> PlayerActionOutcomeReceived;

        event Action LeaveMatchReceived;

        Address? CustomGameModeAddress { get; set; }

        MatchMetadata MatchMetadata { get; }

        GameState InitialGameState { get; }

        List<string> PvPTags { get; set; }

        MatchMakingFlowController MatchMakingFlowController { get; }

        string GetOpponentUserId();

        bool IsCurrentPlayer();

        Task StartMatchmaking(int deckId);

        Task StopMatchmaking();
    }
}
