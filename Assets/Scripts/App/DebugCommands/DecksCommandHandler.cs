using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Data;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

static class DecksCommandHandler
{
    private static IMatchManager _matchManager;
    private static IPvPManager _pvpManager;
    private static ILoadObjectsManager _loadObjectsManager;

    private static DecksData FixedDeckCollection { get; set; }

    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(DecksCommandHandler));

        _matchManager = GameClient.Get<IMatchManager>();
        _pvpManager = GameClient.Get<IPvPManager>();
        _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

        FixedDeckCollection = JsonConvert.DeserializeObject<DecksData>(_loadObjectsManager.GetObjectByPath<TextAsset>("FixedDecks_data").text);
    }


    [CommandHandler(Description = "Find PvP Match with specific Deck Set")]
    public static void FindPvPMatch([Autocomplete(typeof(DecksCommandHandler), "DecksName")] string deckName)
    {
        Deck selectedDeck = FixedDeckCollection.Decks.Find(deck => deck.Name == deckName);
        _matchManager.DebugFindPvPMatch(selectedDeck);
    }

    public static IEnumerable<string> DecksName()
    {
        string[] deckNames = new string[FixedDeckCollection.Decks.Count];
        for (var i = 0; i < FixedDeckCollection.Decks.Count; i++)
        {
            deckNames[i] = FixedDeckCollection.Decks[i].Name;
        }
        return deckNames;
    }
}
