// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public interface IGameplayManager
    {
        event Action OnGameStartedEvent;

        event Action<Enumerators.EndGameType> OnGameEndedEvent;

        event Action OnGameInitializedEvent;

        event Action OnTurnStartedEvent;

        event Action OnTurnEndedEvent;

        int PlayerDeckId { get; set; }

        int OpponentDeckId { get; set; }

        Player CurrentTurnPlayer { get; set; }

        Player CurrentPlayer { get; set; }

        Player OpponentPlayer { get; set; }

        bool GameStarted { get; set; }

        bool GameEnded { get; set; }

        bool IsTutorial { get; set; }

        bool IsPrepairingEnded { get; set; }

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
