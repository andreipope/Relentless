using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class DebugCheatsConfiguration
    {
        public bool Enabled { get; set; }

        public bool UseCustomDeck { get; set; }

        public Deck CustomDeck { get; set; }

        public long? CustomRandomSeed { get; set; }

        public bool DisableDeckShuffle { get; set; }

        public string ForceFirstTurnUserId { get; set; }

        public bool IgnoreGooRequirements { get; set; }

        public bool SkipMulligan { get; set; }

        public DebugCheatsConfiguration()
        {
        }

        public DebugCheatsConfiguration(DebugCheatsConfiguration other)
        {
            CopyFrom(other);
        }

        public void CopyFrom(DebugCheatsConfiguration other)
        {
            Enabled = other.Enabled;
            UseCustomDeck = other.UseCustomDeck;
            CustomDeck = other.CustomDeck;
            CustomRandomSeed = other.CustomRandomSeed;
            DisableDeckShuffle = other.DisableDeckShuffle;
            ForceFirstTurnUserId = other.ForceFirstTurnUserId;
            IgnoreGooRequirements = other.IgnoreGooRequirements;
            SkipMulligan = other.SkipMulligan;
        }
    }
}
