using System.Collections.Generic;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class AIDecksData
    {
        [JsonProperty("ai_decks")]
        public List<AIDeck> Decks;
    }
}
