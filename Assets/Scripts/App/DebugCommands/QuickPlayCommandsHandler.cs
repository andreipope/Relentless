using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

static class QuickPlayCommandsHandler
{
    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(QuickPlayCommandsHandler));
    }

    [CommandHandler(Description = "Print Settings for QuickPlay")]
    private static void Print()
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        IUIManager uiManager = GameClient.Get<IUIManager>();
        IDataManager dataManager = GameClient.Get<IDataManager>();

        int playerDeckId = uiManager.GetPage<GameplayPage>().CurrentDeckId;
        string playerDeckName = dataManager.CachedDecksData.Decks.First(o => o.Id == playerDeckId).Name;

        int opponentDeckId = gameplayManager.OpponentDeckId;
        string opponentDeckName = "Default";
            //dataManager.CachedOpponentDecksData.Decks.First(o => o.Id == opponentDeckId).Name;

        string playerStarterCards = "[ ";
        for (int i = 0; i < gameplayManager.PlayerStarterCards.Count; i++)
        {
            playerStarterCards += gameplayManager.PlayerStarterCards[i] + " ,";
        }
        playerStarterCards = playerStarterCards.TrimEnd(',');
        playerStarterCards += "]";

        string opponentStarterCards = "[ ";
        for (int i = 0; i < gameplayManager.OpponentStarterCards.Count; i++)
        {
            opponentStarterCards += gameplayManager.OpponentStarterCards[i] + " ,";
        }
        opponentStarterCards = opponentStarterCards.TrimEnd(',');
        opponentStarterCards += "]";

        Debug.Log($"(1). Player Horde : {playerDeckName}\n"+
                  $"(2). Enemy AI Horde : {opponentDeckName}\n" +
                  $"(3). Starting Turn : {gameplayManager.StartingTurn}\n"+
                  $"(4). Player Starter Zombies : {playerStarterCards}\n"+
                  $"(5). Enemy Starter Zombies : {opponentStarterCards}\n");
    }

    [CommandHandler(Description = "Starts the battle")]
    private static void Start()
    {
        IMatchManager matchManager = GameClient.Get<IMatchManager>();
        IDataManager dataManager = GameClient.Get<IDataManager>();
        IUIManager uiManager = GameClient.Get<IUIManager>();

        int index = dataManager.CachedDecksData.Decks.FindIndex(
            o => o.Id == uiManager.GetPage<GameplayPage>().CurrentDeckId);
        if (index == -1)
        {
            int lastPlayerDeckId = dataManager.CachedUserLocalData.LastSelectedDeckId;
            uiManager.GetPage<GameplayPage>().CurrentDeckId = lastPlayerDeckId;
        }

        matchManager.FindMatch(Enumerators.MatchType.LOCAL);
    }

    [CommandHandler(Description = "Set Start Turn  - Player / Enemy")]
    private static void StartingTurn(Enumerators.StartingTurn startingTurn)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        gameplayManager.StartingTurn = startingTurn;
    }

    [CommandHandler(Description = "Set which player horde to fight with. Accepts deck name.")]
    private static void SetPlayerHorde(string deckName)
    {
        IDataManager dataManager = GameClient.Get<IDataManager>();
        IUIManager uiManager = GameClient.Get<IUIManager>();

        int index = dataManager.CachedDecksData.Decks.FindIndex(x => x.Name == deckName);
        if (index == -1)
        {
            Debug.LogError(deckName + " Not found");
            return;
        }

        uiManager.GetPage<GameplayPage>().CurrentDeckId = (int)dataManager.CachedDecksData.Decks[index].Id;
    }

    // TODO : Set Enemy Horde, right now no name exist
    [CommandHandler(Description = "Set which enemy horde to fight with. Accepts deck name.")]
    private static void SetEnemyHorde(string deckName)
    {

    }

    [CommandHandler(Description = "Adds starting cards in Player Starter")]
    private static void AddPlayerStarter(params string[] cards)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        gameplayManager.PlayerStarterCards = new List<string>();

        for (int i = 0; i < cards.Length; i++)
        {
            if (gameplayManager.PlayerStarterCards.Count >= Constants.DefaultCardsInHandAtStartGame)
                break;

            gameplayManager.PlayerStarterCards.Add(cards[i]);
        }
    }

    [CommandHandler(Description = "Adds starting cards in Enemy Starter")]
    private static void AddEnemyStarter(string[] cards)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        gameplayManager.OpponentStarterCards = new List<string>();

        for (int i = 0; i < cards.Length; i++)
        {
            if (gameplayManager.OpponentStarterCards.Count >= Constants.DefaultCardsInHandAtStartGame)
                break;

            gameplayManager.OpponentStarterCards.Add(cards[i]);
        }
    }

}
