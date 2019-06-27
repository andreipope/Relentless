using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    [Serializable]
    public class MarketplacePlasmachainNetworkContractAddresses
    {
        [SerializeField]
        private string _zbgCardContractAddress;

        [SerializeField]
        private string _cardFaucetContractAddress;

        [SerializeField]
        private string _boosterPackContractAddress;

        [SerializeField]
        private string _superPackContractAddress;

        [SerializeField]
        private string _airPackContractAddress;

        [SerializeField]
        private string _earthPackContractAddress;

        [SerializeField]
        private string _firePackContractAddress;

        [SerializeField]
        private string _lifePackContractAddress;

        [SerializeField]
        private string _toxicPackContractAddress;

        [SerializeField]
        private string _waterPackContractAddress;

        [SerializeField]
        private string _smallPackContractAddress;

        [SerializeField]
        private string _minionPackContractAddress;

        [SerializeField]
        private string _binancePackContractAddress;

        [SerializeField]
        private string _fiatPurchaseContractAddress;

        [SerializeField]
        private string _openLotteryContractAddress;

        [SerializeField]
        private string _tronLotteryContractAddress;

        public string ZbgCardContractAddress => _zbgCardContractAddress;

        public string CardFaucetContractAddress => _cardFaucetContractAddress;

        public string BoosterPackContractAddress => _boosterPackContractAddress;

        public string SuperPackContractAddress => _superPackContractAddress;

        public string AirPackContractAddress => _airPackContractAddress;

        public string EarthPackContractAddress => _earthPackContractAddress;

        public string FirePackContractAddress => _firePackContractAddress;

        public string LifePackContractAddress => _lifePackContractAddress;

        public string ToxicPackContractAddress => _toxicPackContractAddress;

        public string WaterPackContractAddress => _waterPackContractAddress;

        public string SmallPackContractAddress => _smallPackContractAddress;

        public string MinionPackContractAddress => _minionPackContractAddress;

        public string BinancePackContractAddress => _binancePackContractAddress;

        public string FiatPurchaseContractAddress => _fiatPurchaseContractAddress;

        public string OpenLotteryContractAddress => _openLotteryContractAddress;

        public string TronLotteryContractAddress => _tronLotteryContractAddress;

        public MarketplacePlasmachainNetworkContractAddresses()
        {
        }

        [JsonConstructor]
        public MarketplacePlasmachainNetworkContractAddresses(
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
            string fiatPurchaseContractAddress,
            string openLotteryContractAddress,
            string tronLotteryContractAddress)
        {
            _zbgCardContractAddress = zbgCardContractAddress ?? throw new ArgumentNullException(nameof(zbgCardContractAddress));
            _cardFaucetContractAddress = cardFaucetContractAddress ?? throw new ArgumentNullException(nameof(cardFaucetContractAddress));
            _boosterPackContractAddress = boosterPackContractAddress ?? throw new ArgumentNullException(nameof(boosterPackContractAddress));
            _superPackContractAddress = superPackContractAddress ?? throw new ArgumentNullException(nameof(superPackContractAddress));
            _airPackContractAddress = airPackContractAddress ?? throw new ArgumentNullException(nameof(airPackContractAddress));
            _earthPackContractAddress = earthPackContractAddress ?? throw new ArgumentNullException(nameof(earthPackContractAddress));
            _firePackContractAddress = firePackContractAddress ?? throw new ArgumentNullException(nameof(firePackContractAddress));
            _lifePackContractAddress = lifePackContractAddress ?? throw new ArgumentNullException(nameof(lifePackContractAddress));
            _toxicPackContractAddress = toxicPackContractAddress ?? throw new ArgumentNullException(nameof(toxicPackContractAddress));
            _waterPackContractAddress = waterPackContractAddress ?? throw new ArgumentNullException(nameof(waterPackContractAddress));
            _smallPackContractAddress = smallPackContractAddress ?? throw new ArgumentNullException(nameof(smallPackContractAddress));
            _minionPackContractAddress = minionPackContractAddress ?? throw new ArgumentNullException(nameof(minionPackContractAddress));
            _binancePackContractAddress = binancePackContractAddress ?? throw new ArgumentNullException(nameof(binancePackContractAddress));
            _fiatPurchaseContractAddress = fiatPurchaseContractAddress ?? throw new ArgumentNullException(nameof(fiatPurchaseContractAddress));
            _openLotteryContractAddress = openLotteryContractAddress ?? throw new ArgumentNullException(nameof(openLotteryContractAddress));
            _tronLotteryContractAddress = tronLotteryContractAddress ?? throw new ArgumentNullException(nameof(tronLotteryContractAddress));
        }
    }
}
