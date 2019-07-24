using Newtonsoft.Json;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class LoginData
    {
        [JsonProperty(PropertyName = "accessToken")]
        public string accessToken;
    }

    public class RegisterData
    {
        [JsonProperty(PropertyName = "accessToken")]
        public string accessToken;
    }

    public class AccessTokenData
    {
        [JsonProperty(PropertyName = "user_id")]
        public int user_id;

        [JsonProperty(PropertyName = "authy_id")]
        public int authy_id;

        [JsonProperty(PropertyName = "kind")]
        public string kind;

        [JsonProperty(PropertyName = "exp")]
        public int exp;

        [JsonProperty(PropertyName = "iss")]
        public string iss;

        [JsonProperty(PropertyName = "sub")]
        public string sub;
    }

    public class CreateVaultTokenData
    {
        [JsonProperty(PropertyName = "request_id")]
        public string request_id;

        [JsonProperty(PropertyName = "lease_id")]
        public string lease_id;

        [JsonProperty(PropertyName = "renewable")]
        public bool renewable;

        [JsonProperty(PropertyName = "lease_duration")]
        public int lease_duration;

        [JsonProperty(PropertyName = "auth")]
        public CreateVaultTokenAuthStruct auth;
    }

    public struct CreateVaultTokenAuthStruct 
    {
        public string client_token;
    }

    public class GetVaultDataResponse 
    {
        public GetVaultDataDataStruct data;
    }

    public struct GetVaultDataDataStruct
    {
        public string privatekey;
    }
}
