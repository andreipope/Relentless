namespace Loom.ZombieBattleground.BackendCommunication
{
    public static class PlasmaChainEndpointsContainer
    {
#if USE_PLASMA_DEV_ENVIRONMENT
        //DEV ENVIRONMENT (asia1)
        public const string ContractAddressCardFaucet = "0xb903ba7d4834b97b694d27f857eb44ade6bc70dd";
        public const string ContractAddressBoosterPack = "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95";
        public const string ContractAddressSuperPack = "0x60ab575af210cc952999976854e938447e919871";
        public const string ContractAddressAirPack = "0xb681fbf4b36c49e0811ee640cca1933ab57be81e";
        public const string ContractAddressEarthPack = "0x909de1c6f5863a70e593267db4148f73d475517a";
        public const string ContractAddressFirePack = "0x54ec9e19f72312167b310dcd6bc4039e416cc1bc";
        public const string ContractAddressLifePack = "0xa243cd1ed96d8af80109b88b448105b0473f5cad";
        public const string ContractAddressToxicPack = "0x99a3378185f7b7b6aa9f6632e8ca0514ec01247d";
        public const string ContractAddressWaterPack = "0x9ed8380b47feef8945251ded45f0ba4f1b72f522";
        public const string ContractAddressSmallPack = "0x5f9925006b4ca5b69aed5d24948f80955a07b5ae";
        public const string ContractAddressMinionPack = "0x637845034f92c3fe58d4b43447815dab7b880f9d";
        public const string ContractAddressBinancePack = "0x8240905f778ab546939957bf9f57a369c368a296";
        public const string ContractAddressFiatPurchase = "0xe385465fc42c0898b743282b71ae8d74922a525a";

        public const string Chainid = "asia1";
        public const string WebSocket = "wss://test-z-asia1.dappchains.com/websocket";
        public const string QueryWS = "wss://test-z-asia1.dappchains.com/queryws";

        public const string FiatValidationURL = "https://dev-auth.loom.games/fiat/validate";
        public const string FiatTransactionURL = "https://dev-auth.loom.games/fiat/transaction";
        public const string FiatClaimURL = "https://dev-auth.loom.games/fiat/claim-orders";
        public const string FiatProductsURL = "https://dev-auth.loom.games/fiat/products";
#elif USE_PLASMA_STAGE_ENVIRONMENT
        //STAGING ENVIRONMENT
        public const string ContractAddressZbgCard = "0x2658d8c94062227d17a4ba61adb166e152369de3";
        public const string ContractAddressCardFaucet = "0x42ac2c5ef756896b2820e5a2b433c5cc1ae7ca41";
        public const string ContractAddressBoosterPack = "0xdc745ac9945c981a63748a6b46dc31c2909bc865";
        public const string ContractAddressSuperPack = "0xd05b46ffb3828218d5b7d9b1225575477c9e79d7";
        public const string ContractAddressAirPack = "0x4408927c62a6c8013612c11d630c222c130fd4f8";
        public const string ContractAddressEarthPack = "0x2926196ef74fe0611c474ba822c3b41b8796373e";
        public const string ContractAddressFirePack = "0x5a1a9d8d8cb5ce2e1f664133effb1ba0c9597074";
        public const string ContractAddressLifePack = "0xcc8450ab3f874e741d187d897602ec5a72c4a0be";
        public const string ContractAddressToxicPack = "0x750ffb9928d9fb1dd3b8b7eda47c130b410dde72";
        public const string ContractAddressWaterPack = "0x4f5b70188f14b6d80e8bf0002eca2ab2863ece5e";
        public const string ContractAddressSmallPack = "0x0049493999a5ecc90654b5f0678d50a8952b9c3d";
        public const string ContractAddressMinionPack = "0x63c43b64f40b8115c2c9970e559405fd16377a57";
        public const string ContractAddressBinancePack = "0x837da2498b31d1654d51c1871b10fc4e3d192f02";
        public const string ContractAddressFiatPurchase = "0xaff6212ab34f4066ee46f4b20429b2c74726eb67";
        public const string ContractAddressTutorialReward = "0xaad4debc20cc04b2b749bda12734fc9f2552e751";

        public const string Chainid = "default";
        public const string WebSocket = "wss://test-z-us1.dappchains.com/websocket";
        public const string QueryWS = "wss://test-z-us1.dappchains.com/queryws";

        public const string FiatValidationURL = "https://stage-auth.loom.games/fiat/validate";
        public const string FiatTransactionURL = "https://stage-auth.loom.games/fiat/transaction";
        public const string FiatClaimURL = "https://stage-auth.loom.games/fiat/claim-orders";
        public const string FiatProductsURL = "https://stage-auth.loom.games/fiat/products";
#elif USE_PRODUCTION_BACKEND
        //PRODUCTION ENVIRONMENT
        public const string ContractAddressZbgCard = "0xad94eae166f5f69167e5668f2c8dbfa8e690a120";
        public const string ContractAddressCardFaucet = "0x9d5dd04317a58a16a33b7730e0c02673039148af";
        public const string ContractAddressBoosterPack = "0x2fa54683d976c72806d2e54d1d61a476848e4da9";
        public const string ContractAddressSuperPack = "0x2b44d4f3b086d4b752d762ec2cf1ab7a0b3bfe44";
        public const string ContractAddressAirPack = "0xcfaef1552a11acc7794088ea2434c23af8434ced";
        public const string ContractAddressEarthPack = "0x413db2a16de5d2bd6b4a48be7a772d739932ded1";
        public const string ContractAddressFirePack = "0x6c73a3d880ec475d69576a44705ae24aa5dfaf06";
        public const string ContractAddressLifePack = "0x3f2147aac990e11f0b20ca01a1347b41d52739ae";
        public const string ContractAddressToxicPack = "0x7aef265be0f7aac7c2fe0343d328afbc4c1253c2";
        public const string ContractAddressWaterPack = "0xb8357e4deb282fb20bc6da6a18f21f2da546149b";
        public const string ContractAddressSmallPack = "0xd28138eb47f20314a60714f95f55d5f3dc39da60";
        public const string ContractAddressMinionPack = "0x01d78e15525f4500dc2a7e1fe0ec997dfc3a982e";
        public const string ContractAddressBinancePack = "0x554e93eca9f192a07d5e1337b0d85a12abceb803";
        public const string ContractAddressFiatPurchase = "0x90df68c6745ee7664ac35eae18d98bd0cf930cfb";

        public const string Chainid = "default";
        public const string WebSocket = "wss://plasma.dappchains.com/websocket";
        public const string QueryWS = "wss://plasma.dappchains.com/queryws";

        public const string HttpRpc = "http://plasma.dappchains.com/rpc";
        public const string HttpQuery = "http://plasma.dappchains.com/query";

        public const string FiatValidationURL = "https://auth.loom.games/fiat/validate";
        public const string FiatTransactionURL = "https://auth.loom.games/fiat/transaction";
        public const string FiatClaimURL = "https://auth.loom.games/fiat/claim-orders";
        public const string FiatProductsURL = "https://auth.loom.games/fiat/products";
#else
        //STAGING ENVIRONMENT
        public const string ContractAddressZbgCard = "0x2658d8c94062227d17a4ba61adb166e152369de3";
        public const string ContractAddressCardFaucet = "0x42ac2c5ef756896b2820e5a2b433c5cc1ae7ca41";
        public const string ContractAddressBoosterPack = "0xdc745ac9945c981a63748a6b46dc31c2909bc865";
        public const string ContractAddressSuperPack = "0xd05b46ffb3828218d5b7d9b1225575477c9e79d7";
        public const string ContractAddressAirPack = "0x4408927c62a6c8013612c11d630c222c130fd4f8";
        public const string ContractAddressEarthPack = "0x2926196ef74fe0611c474ba822c3b41b8796373e";
        public const string ContractAddressFirePack = "0x5a1a9d8d8cb5ce2e1f664133effb1ba0c9597074";
        public const string ContractAddressLifePack = "0xcc8450ab3f874e741d187d897602ec5a72c4a0be";
        public const string ContractAddressToxicPack = "0x750ffb9928d9fb1dd3b8b7eda47c130b410dde72";
        public const string ContractAddressWaterPack = "0x4f5b70188f14b6d80e8bf0002eca2ab2863ece5e";
        public const string ContractAddressSmallPack = "0x0049493999a5ecc90654b5f0678d50a8952b9c3d";
        public const string ContractAddressMinionPack = "0x63c43b64f40b8115c2c9970e559405fd16377a57";
        public const string ContractAddressBinancePack = "0x837da2498b31d1654d51c1871b10fc4e3d192f02";
        public const string ContractAddressFiatPurchase = "0xaff6212ab34f4066ee46f4b20429b2c74726eb67";

        public const string Chainid = "default";
        public const string WebSocket = "wss://test-z-us1.dappchains.com/websocket";
        public const string QueryWS = "wss://test-z-us1.dappchains.com/queryws";

        public const string FiatValidationURL = "https://stage-auth.loom.games/fiat/validate";
        public const string FiatTransactionURL = "https://stage-auth.loom.games/fiat/transaction";
        public const string FiatClaimURL = "https://stage-auth.loom.games/fiat/claim-orders";
        public const string FiatProductsURL = "https://stage-auth.loom.games/fiat/products";
#endif        
    }
}
