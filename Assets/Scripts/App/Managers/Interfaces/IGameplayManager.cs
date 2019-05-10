using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground
{
    public interface IGameplayManager
    {
        event Action GameStarted;

        event Action<Enumerators.EndGameType> GameEnded;

        event Action GameInitialized;

        event Action TurnStarted;

        event Action TurnEnded;

        DeckId PlayerDeckId { get; set; }

        DeckId OpponentDeckId { get; set; }

        Player CurrentTurnPlayer { get; set; }

        Player CurrentPlayer { get; set; }

        Player OpponentPlayer { get; set; }

        bool IsGameStarted { get; set; }

        bool IsGameEnded { get; set; }

        bool IsTutorial { get; set; }

        bool IsPreparingEnded { get; set; }

        bool IsDesyncDetected { get; set; }

        bool IsSpecificGameplayBattleground { get; set; }

        bool CanDoDragActions { get; set; }

        bool IsGameplayInputBlocked { get; set; }

        Enumerators.StartingTurn StartingTurn { get; set; }

        T GetController<T>()
            where T : IController;

        void RearrangeHands();

        bool IsLocalPlayerTurn();

        Player GetOpponentByPlayer(Player player);

        void StartGameplay();

        void StopGameplay();

        AnalyticsTimer MatchDuration { get; set; }

        void EndGame(Enumerators.EndGameType endGameType, float timer = 4f);

        void ResetWholeGameplayScene();

        bool IsGameplayReady();

        Player GetPlayerByInstanceId(Loom.ZombieBattleground.Data.InstanceId id);

        PlayerMoveAction PlayerMoves { get; set; }

        Data.Deck CurrentPlayerDeck { get; set; }
        Data.Deck OpponentPlayerDeck { get; set; }

        DeckId OpponentIdCheat { get; set; }
        bool AvoidGooCost { get; set; }
        bool UseInifiniteAbility { get; set; }
        PlayerActionMulligan OpponentHasDoneMulligan { get; set; }
    }
}
