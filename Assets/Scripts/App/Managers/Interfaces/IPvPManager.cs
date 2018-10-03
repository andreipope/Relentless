
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using System;

namespace Loom.ZombieBattleground
{
    public interface IPvPManager
    {
        event Action MatchCreatedActionReceived;
        event Action MatchingStartedActionReceived;
        event Action PlayerLeftGameActionReceived;

        event Action GameStartedActionReceived;
        event Action GameEndedActionReceived;

        event Action EndTurnActionReceived;
        event Action<PlayerActionCardPlay> CardPlayedActionReceived;
        event Action<PlayerActionCardAttack> CardAttackedActionReceived;
        event Action<PlayerActionOverlordSkillUsed> OverlordSkillUsedActionReceived;
        //event Action<PlayerActionUseCardAbility> CardAbilityUsedActionReceived;
        event Action<PlayerActionMulligan> MulliganProcessUsedActionReceived;
        event Action<PlayerActionDrawCard> DrawCardActionReceived;

        FindMatchResponse MatchResponse { get; set; }
        GetGameStateResponse GameStateResponse { get; set; }

        OpponentDeck OpponentDeck { get; set; }
        int OpponentDeckIndex { get; set; }

        bool IsCurrentPlayer();
    }
}
