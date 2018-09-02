using System;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
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

        int TutorialStep { get; set; }

        T GetController<T>()
            where T : IController;

        void RearrangeHands();

        bool IsLocalPlayerTurn();

        void StartGameplay();

        void StopGameplay();

        void EndGame(Enumerators.EndGameType endGameType, float timer = 4f);

        void ResetWholeGameplayScene();

        bool IsGameplayReady();
    }
}
