using System;
using Newtonsoft.Json;

namespace LoomNetwork.CZB.BackendCommunication
{
    public class BetaConfig
    {
        [JsonProperty(PropertyName = "email")]
        public string Email;

        [JsonProperty(PropertyName = "beta_key")]
        public string BetaKey;

        [JsonProperty(PropertyName = "save_turn_data")]
        public bool SaveTurnData;

        [JsonProperty(PropertyName = "latest_version")]
        public Version LatestVersion;
    }
}
