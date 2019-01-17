//#define USE_PLASMA_STAGING_ENVIRONMENT

using System.Collections.Generic;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public static class PlasmaChainEndpointsContainer
    {
#if USE_PLASMA_STAGING_ENVIRONMENT
        //STAGING ENVIRONMENT
        public static readonly string ContractAddressCardFaucet = "0x3a25fd6f5d1408c0f87f52cc4c187fd107d9d4fd";  
        public static readonly string ContractAddressBoosterPack = "0x2fa54683d976c72806d2e54d1d61a476848e4da9";
        public static readonly string ContractAddressFiatPurchase = "0x1ea7954bc25227851051d45f0477d2b9667e4d5d";       
        
        public static readonly string Chainid = "default";
        public static readonly string WebSocket = "wss://test-z-us1.dappchains.com/websocket";
        public static readonly string QueryWS = "wss://test-z-us1.dappchains.com/queryws";
        
        public static readonly string FiatValidationURL = "https://stage-auth.loom.games/fiat/validate";
        public static readonly string FiatTransactionURL = "https://stage-auth.loom.games/fiat/transaction";
        public static readonly string FiatClaimURL = "https://stage-auth.loom.games/fiat/claim-orders"; 
#else
        //DEV ENVIRONMENT (asia1)
        public static readonly string ContractAddressCardFaucet = "0xc5641a5dc35aa0c70144a1ba423d6d24a1a6749e";  
        public static readonly string ContractAddressBoosterPack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public static readonly string ContractAddressFiatPurchase = "0x9212f7b678ce954888a429575f3fcc09b499dc90";
        
        public static readonly string Chainid = "asia1";
        public static readonly string WebSocket = "wss://test-z-asia1.dappchains.com/websocket";
        public static readonly string QueryWS = "wss://test-z-asia1.dappchains.com/queryws"; 
        
        public static readonly string FiatValidationURL = "https://dev-auth.loom.games/fiat/validate";
        public static readonly string FiatTransactionURL = "https://dev-auth.loom.games/fiat/transaction";
        public static readonly string FiatClaimURL = "https://dev-auth.loom.games/fiat/claim-orders";
#endif   
       
    }
}
