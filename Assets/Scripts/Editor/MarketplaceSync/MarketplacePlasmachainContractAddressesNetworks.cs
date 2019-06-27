using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    [Serializable]
    public class MarketplacePlasmachainContractAddressesNetworks
    {
        [SerializeField]
        private MarketplacePlasmachainNetworkContractAddresses _development = new MarketplacePlasmachainNetworkContractAddresses();

        [SerializeField]
        private MarketplacePlasmachainNetworkContractAddresses _staging = new MarketplacePlasmachainNetworkContractAddresses();

        [SerializeField]
        private MarketplacePlasmachainNetworkContractAddresses _production = new MarketplacePlasmachainNetworkContractAddresses();

        public MarketplacePlasmachainNetworkContractAddresses Development => _development;

        public MarketplacePlasmachainNetworkContractAddresses Staging => _staging;

        public MarketplacePlasmachainNetworkContractAddresses Production => _production;

        public MarketplacePlasmachainContractAddressesNetworks() { }

        public MarketplacePlasmachainNetworkContractAddresses this[MarketplacePlasmachainNetwork network]
        {
            get
            {
                switch (network)
                {
                    case MarketplacePlasmachainNetwork.Development:
                        return Development;
                    case MarketplacePlasmachainNetwork.Staging:
                        return Staging;
                    case MarketplacePlasmachainNetwork.Production:
                        return _production;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(network), network, null);
                }
            }
        }

        [JsonConstructor]
        public MarketplacePlasmachainContractAddressesNetworks(
            MarketplacePlasmachainNetworkContractAddresses development,
            MarketplacePlasmachainNetworkContractAddresses staging,
            MarketplacePlasmachainNetworkContractAddresses production)
        {
            _development = development ?? throw new ArgumentNullException(nameof(development));
            _staging = staging ?? throw new ArgumentNullException(nameof(staging));
            _production = production ?? throw new ArgumentNullException(nameof(production));
        }
    }
}
