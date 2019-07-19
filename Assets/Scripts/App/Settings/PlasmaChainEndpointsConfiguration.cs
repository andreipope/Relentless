using System;
using Loom.Client;
using Newtonsoft.Json;

// ReSharper disable StringLiteralTypo

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class PlasmachainEndpointsConfiguration
    {
        [JsonConstructor]
        public PlasmachainEndpointsConfiguration(
            string chainId,
            string readerHost,
            string writerHost,
            string zbgCardContractAddress,
            string cardFaucetContractAddress,
            string boosterPackContractAddress,
            string superPackContractAddress,
            string airPackContractAddress,
            string earthPackContractAddress,
            string firePackContractAddress,
            string lifePackContractAddress,
            string toxicPackContractAddress,
            string waterPackContractAddress,
            string smallPackContractAddress,
            string minionPackContractAddress,
            string binancePackContractAddress,
            string tronPackContractAddress,
            string fiatPurchaseContractAddress,
            string openLotteryContractAddress,
            string tronLotteryContractAddress)
        {
            ChainId = !String.IsNullOrWhiteSpace(chainId) ? chainId : throw new ArgumentException(chainId, nameof(chainId));
            ReaderHost = !String.IsNullOrWhiteSpace(readerHost) ? readerHost : throw new ArgumentException(readerHost, nameof(readerHost));
            WriterHost = !String.IsNullOrWhiteSpace(writerHost) ? writerHost : throw new ArgumentException(writerHost, nameof(writerHost));
            ZbgCardContractAddress = Address.FromString(zbgCardContractAddress, ChainId);
            CardFaucetContractAddress = Address.FromString(cardFaucetContractAddress, ChainId);
            BoosterPackContractAddress = Address.FromString(boosterPackContractAddress, ChainId);
            SuperPackContractAddress = Address.FromString(superPackContractAddress, ChainId);
            AirPackContractAddress = Address.FromString(airPackContractAddress, ChainId);
            EarthPackContractAddress = Address.FromString(earthPackContractAddress, ChainId);
            FirePackContractAddress = Address.FromString(firePackContractAddress, ChainId);
            LifePackContractAddress = Address.FromString(lifePackContractAddress, ChainId);
            ToxicPackContractAddress = Address.FromString(toxicPackContractAddress, ChainId);
            WaterPackContractAddress = Address.FromString(waterPackContractAddress, ChainId);
            SmallPackContractAddress = Address.FromString(smallPackContractAddress, ChainId);
            MinionPackContractAddress = Address.FromString(minionPackContractAddress, ChainId);
            BinancePackContractAddress = Address.FromString(binancePackContractAddress, ChainId);
            TronPackContractAddress = Address.FromString(tronPackContractAddress, ChainId);
            FiatPurchaseContractAddress = Address.FromString(fiatPurchaseContractAddress, ChainId);
            OpenLotteryContractAddress = !String.IsNullOrWhiteSpace(openLotteryContractAddress) ?
                Address.FromString(openLotteryContractAddress, ChainId) :
                new Address();
            TronLotteryContractAddress = !String.IsNullOrWhiteSpace(tronLotteryContractAddress) ?
                Address.FromString(tronLotteryContractAddress, ChainId) :
                new Address();
        }

        [JsonProperty("plasmachain_chain_id")]
        public string ChainId { get; }

        [JsonProperty("plasmachain_reader_host")]
        public string ReaderHost { get; }

        [JsonProperty("plasmachain_writer_host")]
        public string WriterHost { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_zbgcard_contract_address")]
        public Address ZbgCardContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_cardfaucet_contract_address")]
        public Address CardFaucetContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_boosterpack_contract_address")]
        public Address BoosterPackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_superpack_contract_address")]
        public Address SuperPackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_airpack_contract_address")]
        public Address AirPackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_earthpack_contract_address")]
        public Address EarthPackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_firepack_contract_address")]
        public Address FirePackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_lifepack_contract_address")]
        public Address LifePackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_toxicpack_contract_address")]
        public Address ToxicPackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_waterpack_contract_address")]
        public Address WaterPackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_smallpack_contract_address")]
        public Address SmallPackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_minionpack_contract_address")]
        public Address MinionPackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_binancepack_contract_address")]
        public Address BinancePackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_tronpack_contract_address")]
        public Address TronPackContractAddress { get; }

        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_fiatpurchase_contract_address")]
        public Address FiatPurchaseContractAddress { get; }

        /// <summary>
        /// Binance card faucet.
        /// </summary>
        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_openlottery_contract_address")]
        public Address OpenLotteryContractAddress { get; }

        /// <summary>
        /// Tron card faucet.
        /// </summary>
        [JsonConverter(typeof(AddressToLocalAddressStringConverter))]
        [JsonProperty("plasmachain_tronlottery_contract_address")]
        public Address TronLotteryContractAddress { get; }
    }
}
