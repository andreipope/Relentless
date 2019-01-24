using Loom.ZombieBattleground.Protobuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using DebugCheatsConfiguration = Loom.ZombieBattleground.BackendCommunication.DebugCheatsConfiguration;

namespace Loom.ZombieBattleground
{
    public interface IPvPManager
    {
        event Action MatchCreatedActionReceived;
        event Action MatchingStartedActionReceived;
        event Action<PlayerActionLeaveMatch> PlayerLeftGameActionReceived;
        event Action MatchingFailed;

        event Action GameStartedActionReceived;
        event Action GameEndedActionReceived;

        event Action EndTurnActionReceived;
        event Action<PlayerActionCardPlay> CardPlayedActionReceived;
        event Action<PlayerActionCardAttack> CardAttackedActionReceived;
        event Action<PlayerActionOverlordSkillUsed> OverlordSkillUsedActionReceived;
        event Action<PlayerActionCardAbilityUsed> CardAbilityUsedActionReceived;
        event Action<PlayerActionMulligan> MulliganProcessUsedActionReceived;
        event Action<PlayerActionRankBuff> RankBuffActionReceived;
        event Action<PlayerActionCheatDestroyCardsOnBoard> CheatDestroyCardsOnBoardActionReceived;
        event Action<PlayerActionOutcome> PlayerActionOutcomeReceived;

        event Action LeaveMatchReceived;

        Address? CustomGameModeAddress { get; set; }

        MatchMetadata MatchMetadata { get; }

        GameState InitialGameState { get; }

        List<string> PvPTags { get; set; }

        DebugCheatsConfiguration DebugCheats { get; }

        MatchMakingFlowController MatchMakingFlowController { get; }

        string GetOpponentUserId();

        bool IsCurrentPlayer();

        Task StartMatchmaking(int deckId);

        Task StopMatchmaking();

        bool UseBackendGameLogic { get; set; }
    }
}
