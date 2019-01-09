using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Data;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

static class DecksCommandHandler
{
    private static IMatchManager _matchManager;

    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(DecksCommandHandler));

        _matchManager = GameClient.Get<IMatchManager>();
    }

    [CommandHandler(Description = "Find PvP Match with specific Deck Set")]
    public static void FindPvPMatch(string deckName)
    {
        string path = Path.Combine(Application.persistentDataPath, "default_decks.json");
        if (!File.Exists(path))
        {
            Debug.LogError($"File '{path}' not found");
        }

        DecksData decksData = JsonConvert.DeserializeObject<DecksData>(File.ReadAllText(path));
        Deck selectedDeck = decksData.Decks.Find(deck => deck.Name == deckName);
        _matchManager.DebugFindPvPMatch(selectedDeck);
    }
}
