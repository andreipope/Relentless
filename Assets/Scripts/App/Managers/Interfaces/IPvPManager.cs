
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;

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
        event Action<PlayerActionCardAbilityUsed> CardAbilityUsedActionReceived;
        event Action<PlayerActionMulligan> MulliganProcessUsedActionReceived;
        event Action<PlayerActionDrawCard> DrawCardActionReceived;
        event Action<PlayerActionRankBuff> RankBuffActionReceived;

        event Action LeaveMatchReceived;

        Address? CustomGameModeAddress { get; set; }

        MatchMetadata MatchMetadata { get; }

        GameState InitialGameState { get; }

        OpponentDeck OpponentDeck { get; set; }

        string GetOpponentUserId();

        bool IsCurrentPlayer();

        Task FindMatch();

        WorkingCard GetWorkingCardFromCardInstance(CardInstance cardInstance, Player ownerPlayer);
    }
}
