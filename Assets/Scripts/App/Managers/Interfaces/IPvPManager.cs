
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using System;

namespace Loom.ZombieBattleground
{
    public interface IPvPManager
    {
        event Action MatchCreatedActionRecieved;
        event Action MatchingStartedActionRecieved;
        event Action PlayerLeftGameActionRecived;

        event Action GameStartedActionRecieved;
        event Action GameEndedActioRecieved;

        event Action EndTurnActionRecieved;
        event Action<PlayerActionCardPlay> CardPlayedActionRecieved;
        event Action<PlayerActionCardAttack> CardAttackedActionRecieved;
        event Action<PlayerActionUseOverlordSkill> OverlordSkillUsedActionRecieved;
        event Action<PlayerActionUseCardAbility> CardAbilityUsedActionRecieved;
        event Action<PlayerActionMulligan> MulliganProcessUsedActionRecieved;
        event Action<PlayerActionDrawCard> DrawCardActionRecieved;

        FindMatchResponse MatchResponse { get; set; }
        GetGameStateResponse GameStateResponse { get; set; }

        OpponentDeck OpponentDeck { get; set; }
        int OpponentDeckIndex { get; set; }

        bool IsCurrentPlayer();
    }
}
