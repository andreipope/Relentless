#define USE_PLASMA_DEV_ENVIRONMENT
using System.Collections.Generic;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public static class PlasmaChainEndpointsContainer
    {
#if USE_PLASMA_DEV_ENVIRONMENT
        //DEV ENVIRONMENT (asia1)
        public static readonly string ContractAddressCardFaucet = "0xc5641a5dc35aa0c70144a1ba423d6d24a1a6749e";  
        public static readonly string ContractAddressBoosterPack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public static readonly string ContractAddressFiatPurchase = "0x3f7d995201684ef012fe522c26761c81f5880e34";
        
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
        public static readonly string ContractAddressFiatPurchase = "0xb4b0bd2eb757b124a72065bfa535bea1849101ab";       
        
        public static readonly string Chainid = "default";
        public static readonly string WebSocket = "wss://test-z-us1.dappchains.com/websocket";
        public static readonly string QueryWS = "wss://test-z-us1.dappchains.com/queryws";
        
        public static readonly string FiatValidationURL = "https://stage-auth.loom.games/fiat/validate";
        public static readonly string FiatTransactionURL = "https://stage-auth.loom.games/fiat/transaction";
        public static readonly string FiatClaimURL = "https://stage-auth.loom.games/fiat/claim-orders"; 
#endif        
    }
}
