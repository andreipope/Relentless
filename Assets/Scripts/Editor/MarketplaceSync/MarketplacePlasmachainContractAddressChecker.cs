using System;
using System.Collections.Generic;
using System.IO;
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Loom.ZombieBattleground.Editor
{
    public class MarketplacePlasmachainContractAddressChecker
    {
        public static readonly IReadOnlyList<MarketplacePlasmachainNetwork> Networks =
            (MarketplacePlasmachainNetwork[]) Enum.GetValues(typeof(MarketplacePlasmachainNetwork));

        private readonly string _contractsRootDirectory;

        public MarketplacePlasmachainContractAddressChecker(string contractsRootDirectory)
        {
            _contractsRootDirectory = contractsRootDirectory;
        }

        public ComparisonResult CompareNetworks(
            MarketplacePlasmachainContractAddressesNetworks storedNetworks,
            MarketplacePlasmachainContractAddressesNetworks fetchedNetworks
        )
        {
            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.ShowBreadcrumb = true;
            compareLogic.Config.TreatStringEmptyAndNullTheSame = true;
            compareLogic.Config.SkipInvalidIndexers = true;
            compareLogic.Config.MaxDifferences = 100;
            compareLogic.Config.ActualName = "Fetched";
            compareLogic.Config.ExpectedName = "Stored";
            return compareLogic.Compare(storedNetworks, fetchedNetworks);
        }

        public MarketplacePlasmachainContractAddressesNetworks GetContractAddressesOnAllNetworks()
        {
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> zbgCardAddresses = GetAddressesOfContract("MigratedZBGCard");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> cardFaucetAddresses = GetAddressesOfContract("CardFaucet");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> boosterPackAddresses = GetAddressesOfContract("ZBGBoosterPack");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> superPackAddresses = GetAddressesOfContract("ZBGSuperPack");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> airPackAddresses = GetAddressesOfContract("ZBGStarterAirPack");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> earthPackAddresses = GetAddressesOfContract("ZBGStarterEarthPack");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> firePackAddresses = GetAddressesOfContract("ZBGStarterFirePack");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> lifePackAddresses = GetAddressesOfContract("ZBGStarterLifePack");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> toxicPackAddresses = GetAddressesOfContract("ZBGStarterToxicPack");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> waterPackAddresses = GetAddressesOfContract("ZBGStarterWaterPack");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> smallPackAddresses = GetAddressesOfContract("ZBGSmallPack");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> minionPackAddresses = GetAddressesOfContract("ZBGMinionPack");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> binancePackAddresses = GetAddressesOfContract("ZBGBinancePack");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> fiatPurchaseAddresses = GetAddressesOfContract("FiatPurchase");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> openLotteryAddresses = GetAddressesOfContract("OpenLottery");
            IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> tronLotteryAddresses = GetAddressesOfContract("TronLottery");
            Dictionary<MarketplacePlasmachainNetwork, MarketplacePlasmachainNetworkContractAddresses> networkContractAddresses =
                new Dictionary<MarketplacePlasmachainNetwork, MarketplacePlasmachainNetworkContractAddresses>();

            foreach (MarketplacePlasmachainNetwork network in Networks)
            {
                networkContractAddresses[network] =
                    new MarketplacePlasmachainNetworkContractAddresses(
                        zbgCardAddresses[network],
                        cardFaucetAddresses[network],
                        boosterPackAddresses[network],
                        superPackAddresses[network],
                        airPackAddresses[network],
                        earthPackAddresses[network],
                        firePackAddresses[network],
                        lifePackAddresses[network],
                        toxicPackAddresses[network],
                        waterPackAddresses[network],
                        smallPackAddresses[network],
                        minionPackAddresses[network],
                        binancePackAddresses[network],
                        fiatPurchaseAddresses[network],
                        openLotteryAddresses[network],
                        tronLotteryAddresses[network]
                    );
            }

            return new MarketplacePlasmachainContractAddressesNetworks(
                networkContractAddresses[MarketplacePlasmachainNetwork.Development],
                networkContractAddresses[MarketplacePlasmachainNetwork.Staging],
                networkContractAddresses[MarketplacePlasmachainNetwork.Production]
            );
        }

        private IReadOnlyDictionary<MarketplacePlasmachainNetwork, string> GetAddressesOfContract(string contractName)
        {
            string path = Path.Combine(_contractsRootDirectory, contractName + ".json");
            string contractJson = File.ReadAllText(path);
            JToken contract = JToken.Parse(contractJson);
            Dictionary<MarketplacePlasmachainNetwork, string> networkToContractAddress = new Dictionary<MarketplacePlasmachainNetwork, string>();
            foreach (MarketplacePlasmachainNetwork network in Networks)
            {
                string address = contract["networks"][GetPlasmachainNetworkInternalName(network)].Value<string>("address");
                networkToContractAddress[network] = address.ToLowerInvariant();
            }

            return networkToContractAddress;
        }

        public static bool IsValidContractsRootDirectory(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return false;

            if (!Directory.Exists(path))
                return false;

            if (!File.Exists(Path.Combine(path, "CardFaucet.json")))
                return false;

            return true;
        }

        public static string GetPlasmachainNetworkInternalName(MarketplacePlasmachainNetwork network)
        {
            switch (network)
            {
                case MarketplacePlasmachainNetwork.Development:
                    return "asia1";
                case MarketplacePlasmachainNetwork.Staging:
                    return "stage";
                case MarketplacePlasmachainNetwork.Production:
                    return "plasma";
                default:
                    throw new ArgumentOutOfRangeException(nameof(network), network, null);
            }
        }
    }
}
