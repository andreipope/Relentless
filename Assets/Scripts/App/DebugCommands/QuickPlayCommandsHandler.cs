using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

static class QuickPlayCommandsHandler
{
    private static IGameplayManager _gameplayManager;
    private static IUIManager _uiManager;
    private static IDataManager _dataManager;
    private static IMatchManager _matchManager;

    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(QuickPlayCommandsHandler));

        _gameplayManager = GameClient.Get<IGameplayManager>();
        _uiManager = GameClient.Get<IUIManager>();
        _dataManager = GameClient.Get<IDataManager>();
        _matchManager = GameClient.Get<IMatchManager>();
    }

    [CommandHandler(Description = "Print Settings for QuickPlay")]
    private static void Print()
    {
        int playerDeckId = _uiManager.GetPage<GameplayPage>().CurrentDeckId;
        string playerDeckName = _dataManager.CachedDecksData.Decks.First(deck => deck.Id == playerDeckId).Name;

        int opponentDeckId = _gameplayManager.OpponentDeckId;
        string opponentDeckName = "Default";
            //dataManager.CachedOpponentDecksData.Decks.First(deck => deck.Id == opponentDeckId).Name;

        string playerStarterCards = "[ ";
        for (int i = 0; i < _gameplayManager.PlayerStarterCards.Count; i++)
        {
            playerStarterCards += _gameplayManager.PlayerStarterCards[i] + " ,";
        }
        playerStarterCards = playerStarterCards.TrimEnd(',');
        playerStarterCards += "]";

        string opponentStarterCards = "[ ";
        for (int i = 0; i < _gameplayManager.OpponentStarterCards.Count; i++)
        {
            opponentStarterCards += _gameplayManager.OpponentStarterCards[i] + " ,";
        }
        opponentStarterCards = opponentStarterCards.TrimEnd(',');
        opponentStarterCards += "]";

        Debug.Log($"(1). Player Horde : {playerDeckName}\n"+
                  $"(2). Enemy AI Horde : {opponentDeckName}\n" +
                  $"(3). Starting Turn : {_gameplayManager.StartingTurn}\n"+
                  $"(4). Player Starter Zombies : {playerStarterCards}\n"+
                  $"(5). Enemy Starter Zombies : {opponentStarterCards}\n");
    }

    [CommandHandler(Description = "Starts the battle")]
    private static void Start()
    {
        int index = _dataManager.CachedDecksData.Decks.FindIndex(
            deck => deck.Id == _uiManager.GetPage<GameplayPage>().CurrentDeckId);
        if (index == -1)
        {
            int lastPlayerDeckId = _dataManager.CachedUserLocalData.LastSelectedDeckId;
            _uiManager.GetPage<GameplayPage>().CurrentDeckId = lastPlayerDeckId;
        }

        _matchManager.FindMatch(Enumerators.MatchType.LOCAL);
    }

    [CommandHandler(Description = "Set Start Turn  - Player / Enemy")]
    private static void StartingTurn(Enumerators.StartingTurn startingTurn)
    {
        _gameplayManager.StartingTurn = startingTurn;
    }

    [CommandHandler(Description = "Set which player horde to fight with. Accepts deck name.")]
    private static void SetPlayerHorde(string deckName)
    {
        int index = _dataManager.CachedDecksData.Decks.FindIndex(deck => deck.Name == deckName);
        if (index == -1)
        {
            Debug.LogError(deckName + " Not found");
            return;
        }

        _uiManager.GetPage<GameplayPage>().CurrentDeckId = (int)_dataManager.CachedDecksData.Decks[index].Id;
    }

    // TODO : Set Enemy Horde, right now no name exist
    [CommandHandler(Description = "Set which enemy horde to fight with. Accepts deck name.")]
    private static void SetEnemyHorde(string deckName)
    {

    }

    [CommandHandler(Description = "Adds starting cards in Player Starter")]
    private static void AddPlayerStarter(params string[] cards)
    {
        _gameplayManager.PlayerStarterCards = new List<string>();

        for (int i = 0; i < cards.Length; i++)
        {
            if (_gameplayManager.PlayerStarterCards.Count >= Constants.DefaultCardsInHandAtStartGame)
                break;

            _gameplayManager.PlayerStarterCards.Add(cards[i]);
        }
    }

    [CommandHandler(Description = "Adds starting cards in Enemy Starter")]
    private static void AddEnemyStarter(string[] cards)
    {
        _gameplayManager.OpponentStarterCards = new List<string>();

        for (int i = 0; i < cards.Length; i++)
        {
            if (_gameplayManager.OpponentStarterCards.Count >= Constants.DefaultCardsInHandAtStartGame)
                break;

            _gameplayManager.OpponentStarterCards.Add(cards[i]);
        }
    }
}
