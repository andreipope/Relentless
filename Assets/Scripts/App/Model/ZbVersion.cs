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
            public string DownloadUrlPc { get; private set; }

            [JsonProperty("download_url_mac")]
            public string DownloadUrlMac { get; private set; }

            [JsonProperty("download_url_app_store")]
            public string DownloadUrlAppStore { get; private set; }

            [JsonProperty("download_url_play_store")]
            public string DownloadUrlPlayStore { get; private set; }

            [JsonProperty("download_url_steam_store")]
            public string DownloadUrlSteamStore { get; private set; }

            [JsonProperty("plasmachain_chain_id")]
            public string PlasmachainChainId { get; private set; }

            [JsonProperty("plasmachain_reader_host")]
            public string PlasmachainReaderHost { get; private set; }

            [JsonProperty("plasmachain_writer_host")]
            public string PlasmachainWriterHost { get; private set; }

            [JsonProperty("plasmachain_zbgcard_contract_address")]
            public string PlasmachainZbgCardContractAddress { get; private set; }

            [JsonProperty("plasmachain_cardfaucet_contract_address")]
            public string PlasmachainCardFaucetContractAddress { get; private set; }

            [JsonProperty("plasmachain_boosterpack_contract_address")]
            public string PlasmachainBoosterPackContractAddress { get; private set; }

            [JsonProperty("plasmachain_superpack_contract_address")]
            public string PlasmachainSuperPackContractAddress { get; private set; }

            [JsonProperty("plasmachain_airpack_contract_address")]
            public string PlasmachainAirPackContractAddress { get; private set; }

            [JsonProperty("plasmachain_earthpack_contract_address")]
            public string PlasmachainEarthPackContractAddress { get; private set; }

            [JsonProperty("plasmachain_firepack_contract_address")]
            public string PlasmachainFirePackContractAddress { get; private set; }

            [JsonProperty("plasmachain_lifepack_contract_address")]
            public string PlasmachainLifePackContractAddress { get; private set; }

            [JsonProperty("plasmachain_toxicpack_contract_address")]
            public string PlasmachainToxicPackContractAddress { get; private set; }

            [JsonProperty("plasmachain_waterpack_contract_address")]
            public string PlasmachainWaterPackContractAddress { get; private set; }

            [JsonProperty("plasmachain_smallpack_contract_address")]
            public string PlasmachainSmallPackContractAddress { get; private set; }

            [JsonProperty("plasmachain_minionpack_contract_address")]
            public string PlasmachainMinionPackContractAddress { get; private set; }

            [JsonProperty("plasmachain_binancepack_contract_address")]
            public string PlasmachainBinancePackContractAddress { get; private set; }

            [JsonProperty("plasmachain_tronpack_contract_address")]
            public string PlasmachainTronPackContractAddress { get; private set; }

            [JsonProperty("plasmachain_fiatpurchase_contract_address")]
            public string PlasmachainFiatPurchaseContractAddress { get; private set; }

            [JsonProperty("plasmachain_openlottery_contract_address")]
            public string PlasmachainOpenLotteryContractAddress { get; private set; }

            [JsonProperty("plasmachain_tronlottery_contract_address")]
            public string PlasmachainTronLotteryContractAddress { get; private set; }

            [JsonConstructor]
            public ZbVersionData(int id, int major, int minor, int patch, string environment,
                string authUrl,
                string readUrl,
                string writeUrl,
                string vaultUrl,
                string dataVersion,
                bool isMaintenanceMode,
                bool isForceUpdate,
                string downloadUrlPc,
                string downloadUrlMac,
                string downloadUrlAppStore,
                string downloadUrlPlayStore,
                string downloadUrlSteamStore,
                string plasmachainChainId,
                string plasmachainReaderHost,
                string plasmachainWriterHost,
                string plasmachainZbgCardContractAddress,
                string plasmachainCardFaucetContractAddress,
                string plasmachainBoosterPackContractAddress,
                string plasmachainSuperPackContractAddress,
                string plasmachainAirPackContractAddress,
                string plasmachainEarthPackContractAddress,
                string plasmachainFirePackContractAddress,
                string plasmachainLifePackContractAddress,
                string plasmachainToxicPackContractAddress,
                string plasmachainWaterPackContractAddress,
                string plasmachainSmallPackContractAddress,
                string plasmachainMinionPackContractAddress,
                string plasmachainBinancePackContractAddress,
                string plasmachainTronPackContractAddress,
                string plasmachainFiatPurchaseContractAddress,
                string plasmachainOpenLotteryContractAddress,
                string plasmachainTronLotteryContractAddress)
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
                IsMaintenanceMode = isMaintenanceMode;
                IsForceUpdate = isForceUpdate;
                DownloadUrlPc = downloadUrlPc;
                DownloadUrlMac = downloadUrlMac;
                DownloadUrlAppStore = downloadUrlAppStore;
                DownloadUrlPlayStore = downloadUrlPlayStore;
                DownloadUrlSteamStore = downloadUrlSteamStore;
                PlasmachainChainId = plasmachainChainId;
                PlasmachainReaderHost = plasmachainReaderHost;
                PlasmachainWriterHost = plasmachainWriterHost;
                PlasmachainZbgCardContractAddress = plasmachainZbgCardContractAddress;
                PlasmachainCardFaucetContractAddress = plasmachainCardFaucetContractAddress;
                PlasmachainBoosterPackContractAddress = plasmachainBoosterPackContractAddress;
                PlasmachainSuperPackContractAddress = plasmachainSuperPackContractAddress;
                PlasmachainAirPackContractAddress = plasmachainAirPackContractAddress;
                PlasmachainEarthPackContractAddress = plasmachainEarthPackContractAddress;
                PlasmachainFirePackContractAddress = plasmachainFirePackContractAddress;
                PlasmachainLifePackContractAddress = plasmachainLifePackContractAddress;
                PlasmachainToxicPackContractAddress = plasmachainToxicPackContractAddress;
                PlasmachainWaterPackContractAddress = plasmachainWaterPackContractAddress;
                PlasmachainSmallPackContractAddress = plasmachainSmallPackContractAddress;
                PlasmachainMinionPackContractAddress = plasmachainMinionPackContractAddress;
                PlasmachainBinancePackContractAddress = plasmachainBinancePackContractAddress;
                PlasmachainTronPackContractAddress = plasmachainTronPackContractAddress;
                PlasmachainFiatPurchaseContractAddress = plasmachainFiatPurchaseContractAddress;
                PlasmachainOpenLotteryContractAddress = plasmachainOpenLotteryContractAddress;
                PlasmachainTronLotteryContractAddress = plasmachainTronLotteryContractAddress;
            }
        }
    }
}
