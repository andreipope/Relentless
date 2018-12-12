using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class DebugCheatsConfiguration
    {
        public bool Enabled { get; set; }

        public Deck CustomDeck { get; set; }

        public long? CustomRandomSeed { get; set; }
    }
}
