using System.IO;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

static class DecksCommandHandler
{
    private static IMatchManager _matchManager;
    private static ILocalizationManager _localizationManager;

    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(DecksCommandHandler));

        _matchManager = GameClient.Get<IMatchManager>();
        _localizationManager = GameClient.Get<ILocalizationManager>();
    }

    [CommandHandler(Description = "Find PvP Match with specific Deck Set")]
    public static void FindPvPMatch([Autocomplete(typeof(DecksCommandHandler), "DecksName")] string deckName)
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

    [CommandHandler(Description = "Language")]
    public static void ChangeLanguage(Enumerators.Language language)
    {
        _localizationManager.SetLanguage(language);
    }
}
