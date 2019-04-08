using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class ZbVersion
    {
        [JsonProperty("version")]
        public ZbVersionData Version { get; private set; }

        [JsonConstructor]
        public ZbVersion(ZbVersionData version)
        {
            Version = version;
        }

        public class ZbVersionData
        {
            [JsonProperty("id")]
            public int Id { get; private set; }

            [JsonProperty("major")]
            public int Major { get; private set; }

            [JsonProperty("minor")]
            public int Minor { get; private set; }

            [JsonProperty("patch")]
            public int Patch { get; private set; }

            [JsonProperty("environment")]
            public string Environment { get; private set; }

            [JsonProperty("auth_url")]
            public string AuthUrl { get; private set; }

            [JsonProperty("read_url")]
            public string ReadUrl { get; private set; }

            [JsonProperty("write_url")]
            public string WriteUrl { get; private set; }

            [JsonProperty("vault_url")]
            public string VaultUrl { get; private set; }

            [JsonProperty("data_version")]
            public string DataVersion { get; private set; }

            [JsonProperty("is_maintenace_mode")]
            public bool IsMaintenanceMode { get; private set; }

            [JsonProperty("is_force_update")]
            public bool IsForceUpdate { get; private set; }

            [JsonProperty("download_url_pc")]
            public string DownloadUrlPC { get; private set; }

            [JsonProperty("download_url_mac")]
            public string DownloadUrlMac { get; private set; }

            [JsonProperty("download_url_app_store")]
            public string DownloadUrlAppStore { get; private set; }

            [JsonProperty("download_url_play_store")]
            public string DownloadUrlPlayStore { get; private set; }

            [JsonProperty("download_url_steam_store")]
            public string DownloadUrlSteamStore { get; private set; }

            [JsonConstructor]
            public ZbVersionData(
                int id,
                int major,
                int minor,
                int patch,
                string environment,
                string authUrl,
                string readUrl,
                string writeUrl,
                string vaultUrl,
                string dataVersion,
                bool isMaintenaceMode,
                bool isForceUpdate,
                string downloadUrlPC,
                string downloadUrlMac,
                string downloadUrlAppStore,
                string downloadUrlPlayStore,
                string downloadUrlSteamStore)
            {
                Id = id;
                Major = major;
                Minor = minor;
                Patch = patch;
                Environment = environment;
                AuthUrl = authUrl;
                ReadUrl = readUrl;
                WriteUrl = writeUrl;
                VaultUrl = vaultUrl;
                DataVersion = dataVersion;
                IsMaintenanceMode = isMaintenaceMode;
                IsForceUpdate = isForceUpdate;
                DownloadUrlPC = downloadUrlPC;
                DownloadUrlMac = downloadUrlMac;
                DownloadUrlAppStore = downloadUrlAppStore;
                DownloadUrlPlayStore = downloadUrlPlayStore;
                DownloadUrlSteamStore = downloadUrlSteamStore;
            }
        }
    }
}
