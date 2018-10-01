
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using System;

namespace Loom.ZombieBattleground
{
    public interface IPvPManager
    {
        event Action OnMatchCreated;
        event Action OnMatchingStarted;
        event Action OnPlayerLeftGame;

        event Action OnGameStarted;
        event Action OnGameEnded;

        event Action OnGetEndTurnAction;
        event Action<PlayerActionCardPlay> OnCardPlayedAction;
        event Action<PlayerActionCardAttack> OnCardAttackedAction;
        event Action<PlayerActionUseOverlordSkill> OnOverlordSkillUsedAction;
        event Action<PlayerActionUseCardAbility> OnCardAbilityUsedAction;
        event Action<PlayerActionMulligan> OnMulliganProcessUsedAction;
        event Action<PlayerActionDrawCard> OnDrawCardAction;

        FindMatchResponse MatchResponse { get; set; }
        GetGameStateResponse GameStateResponse { get; set; }

        OpponentDeck OpponentDeck { get; set; }
        int OpponentDeckIndex { get; set; }

        bool IsCurrentPlayer();
    }
}
