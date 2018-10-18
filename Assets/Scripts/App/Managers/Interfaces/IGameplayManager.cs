using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public interface IGameplayManager
    {
        event Action GameStarted;

        event Action<Enumerators.EndGameType> GameEnded;

        event Action GameInitialized;

        event Action TurnStarted;

        event Action TurnEnded;

        int PlayerDeckId { get; set; }

        int OpponentDeckId { get; set; }

        Player CurrentTurnPlayer { get; set; }

        Player CurrentPlayer { get; set; }

        Player OpponentPlayer { get; set; }

        bool IsGameStarted { get; set; }

        bool IsGameEnded { get; set; }

        bool IsTutorial { get; set; }

        bool IsPreparingEnded { get; set; }

        bool IsSpecificGameplayBattleground { get; set; }

        bool CanDoDragActions { get; set; }

        bool IsGameplayInputBlocked { get; set; }

        List<CardWithID> PlayerStarterCards { get; set; }
        List<CardWithID> OpponentStarterCards { get; set; }

        Enumerators.StartingTurn StartingTurn { get; set; }

        T GetController<T>()
            where T : IController;

        void RearrangeHands();

        bool IsLocalPlayerTurn();

        Player GetOpponentByPlayer(Player player);

        void StartGameplay();

        void StopGameplay();

        void EndGame(Enumerators.EndGameType endGameType, float timer = 4f);

        void ResetWholeGameplayScene();

        bool IsGameplayReady();

        Player GetPlayerById(int id);

        PlayerMoveAction PlayerMoves { get; set; }
    }
}
