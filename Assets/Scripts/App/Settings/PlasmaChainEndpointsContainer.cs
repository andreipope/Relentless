#define USE_PLASMA_DEV_ENVIRONMENT
using System.Collections.Generic;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public static class PlasmaChainEndpointsContainer
    {
#if USE_PLASMA_DEV_ENVIRONMENT
        //DEV ENVIRONMENT (asia1)
        public static readonly string ContractAddressCardFaucet = "0xbd54b4b26ac8e4bd202b70f3fbad6b3d5a885eec";  
        public static readonly string ContractAddressBoosterPack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public static readonly string ContractAddressSuperPack = "0x60ab575af210cc952999976854e938447e919871";
        public static readonly string ContractAddressAirPack = "0xb681fbf4b36c49e0811ee640cca1933ab57be81e";
        public static readonly string ContractAddressEarthPack = "0x909de1c6f5863a70e593267db4148f73d475517a";
        public static readonly string ContractAddressFirePack = "0x54ec9e19f72312167b310dcd6bc4039e416cc1bc";
        public static readonly string ContractAddressLifePack = "0xa243cd1ed96d8af80109b88b448105b0473f5cad";
        public static readonly string ContractAddressToxicPack = "0x99a3378185f7b7b6aa9f6632e8ca0514ec01247d";
        public static readonly string ContractAddressWaterPack = "0x9ed8380b47feef8945251ded45f0ba4f1b72f522";
        public static readonly string ContractAddressSmallPack = "0x0049493999a5ecc90654b5f0678d50a8952b9c3d";
        public static readonly string ContractAddressMinionPack = "0x637845034f92c3fe58d4b43447815dab7b880f9d";
        public static readonly string ContractAddressBinancePack = "0x8240905f778ab546939957bf9f57a369c368a296";
        public static readonly string ContractAddressFiatPurchase = "0xe385465fc42c0898b743282b71ae8d74922a525a";
        public static readonly string ContractAddressTutorialReward = "0x2668524145c2f05f2e9de4f6d4a0171535df733a";
        public static readonly string Chainid = "asia1";
        public static readonly string WebSocket = "wss://test-z-asia1.dappchains.com/websocket";
        public static readonly string QueryWS = "wss://test-z-asia1.dappchains.com/queryws"; 
        
        public static readonly string FiatValidationURL = "https://dev-auth.loom.games/fiat/validate";
        public static readonly string FiatTransactionURL = "https://dev-auth.loom.games/fiat/transaction";
        public static readonly string FiatClaimURL = "https://dev-auth.loom.games/fiat/claim-orders";       
#else
        //STAGING ENVIRONMENT
        public static readonly string ContractAddressCardFaucet = "0xa99d2de260dd88a46017d9187c381e1dfc9bb0d0";  
        public static readonly string ContractAddressBoosterPack = "0xdc745ac9945c981a63748a6b46dc31c2909bc865";
        public static readonly string ContractAddressSuperPack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public static readonly string ContractAddressAirPack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public static readonly string ContractAddressEarthPack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public static readonly string ContractAddressFirePack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public static readonly string ContractAddressLifePack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public static readonly string ContractAddressToxicPack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public static readonly string ContractAddressWaterPack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public static readonly string ContractAddressSmallPack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public static readonly string ContractAddressMinionPack = "0x63c43b64f40b8115c2c9970e559405fd16377a57";
        public static readonly string ContractAddressBinancePack = "0x8240905f778ab546939957bf9f57a369c368a296";
        public static readonly string ContractAddressFiatPurchase = "0xb4b0bd2eb757b124a72065bfa535bea1849101ab";       
        public static readonly string ContractAddressTutorialReward = "0xad5fb91fc52afcc76e4a996d4bc13137fa813bb6";
        
        public static readonly string Chainid = "default";
        public static readonly string WebSocket = "wss://test-z-us1.dappchains.com/websocket";
        public static readonly string QueryWS = "wss://test-z-us1.dappchains.com/queryws";
        
        public static readonly string FiatValidationURL = "https://stage-auth.loom.games/fiat/validate";
        public static readonly string FiatTransactionURL = "https://stage-auth.loom.games/fiat/transaction";
        public static readonly string FiatClaimURL = "https://stage-auth.loom.games/fiat/claim-orders"; 
#endif        
    }
}
