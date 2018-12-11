using System;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class UserInfo
    {
        [JsonProperty(PropertyName = "email")]
        public string Email;

        [JsonProperty(PropertyName = "user_id")]
        public int UserId;

        [JsonProperty(PropertyName = "username")]
        public string Username;

        [JsonProperty(PropertyName = "latest_version")]
        public Version LatestVersion;
    }
}
