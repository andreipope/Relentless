using System.Collections.Generic;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public static class PlasmachainEndpointConfigurationsContainer
    {
        private static readonly PlasmachainEndpointsConfiguration DevelopmentConfiguration =
            new PlasmachainEndpointsConfiguration(
                "asia1",
                "wss://test-z-asia1.dappchains.com/queryws",
                "wss://test-z-asia1.dappchains.com/websocket",
                "0x1a31b9b9d281d49001fe7f3f638000a739afc9c3",
                "0xacc7bc52599ec656aa66ce31d8915ad123e8a693",
                "0x04aed4899e1514e9ebd3b1ea19d845d60f9eab95",
                "0x60ab575af210cc952999976854e938447e919871",
                "0xb681fbf4b36c49e0811ee640cca1933ab57be81e",
                "0x909de1c6f5863a70e593267db4148f73d475517a",
                "0x54ec9e19f72312167b310dcd6bc4039e416cc1bc",
                "0xa243cd1ed96d8af80109b88b448105b0473f5cad",
                "0x99a3378185f7b7b6aa9f6632e8ca0514ec01247d",
                "0x9ed8380b47feef8945251ded45f0ba4f1b72f522",
                "0x0fd6e6405b2cef67447b11630d1c9e0ae03e39dc",
                "0x03a66793dfbec8ed27b6ef96fd3215a1b4f35093",
                "0x2e6d4f50150c2eba04ecb64d73772864c3bf1ad1",
                "0x70f467d600cb3f4b0f7646d2e6346b70ae6876d7",
                "0x9b7243bcfea5cb960c3a739f3619d13bb88f9da9"
            );

        private static readonly PlasmachainEndpointsConfiguration StagingConfiguration =
            new PlasmachainEndpointsConfiguration(
                "default",
                "wss://test-z-us1.dappchains.com/queryws",
                "wss://test-z-us1.dappchains.com/websocket",
                "0x2658d8c94062227d17a4ba61adb166e152369de3",
                "0x42ac2c5ef756896b2820e5a2b433c5cc1ae7ca41",
                "0xdc745ac9945c981a63748a6b46dc31c2909bc865",
                "0xd05b46ffb3828218d5b7d9b1225575477c9e79d7",
                "0x4408927c62a6c8013612c11d630c222c130fd4f8",
                "0x2926196ef74fe0611c474ba822c3b41b8796373e",
                "0x5a1a9d8d8cb5ce2e1f664133effb1ba0c9597074",
                "0xcc8450ab3f874e741d187d897602ec5a72c4a0be",
                "0x750ffb9928d9fb1dd3b8b7eda47c130b410dde72",
                "0x4f5b70188f14b6d80e8bf0002eca2ab2863ece5e",
                "0x837da2498b31d1654d51c1871b10fc4e3d192f02",
                "0x3d9cb3c4af539f88f3fdd863621758be4917ef05",
                "0xaff6212ab34f4066ee46f4b20429b2c74726eb67",
                "0x808a1b867d2061d7bab4c610b6f98da9b65e42cf",
                "0x4d608125767b951758fa23d9767791b683345a1a"
            );

        private static readonly PlasmachainEndpointsConfiguration ProductionConfiguration =
            new PlasmachainEndpointsConfiguration(
                "default",
                "wss://plasma.dappchains.com/queryws",
                "wss://plasma.dappchains.com/websocket",
                "0xad94eae166f5f69167e5668f2c8dbfa8e690a120",
                "0x1621fbbede0e85fb1fafd4f29cc2faaebde62db0",
                "0x2fa54683d976c72806d2e54d1d61a476848e4da9",
                "0x2b44d4f3b086d4b752d762ec2cf1ab7a0b3bfe44",
                "0xcfaef1552a11acc7794088ea2434c23af8434ced",
                "0x413db2a16de5d2bd6b4a48be7a772d739932ded1",
                "0x6c73a3d880ec475d69576a44705ae24aa5dfaf06",
                "0x3f2147aac990e11f0b20ca01a1347b41d52739ae",
                "0x7aef265be0f7aac7c2fe0343d328afbc4c1253c2",
                "0xb8357e4deb282fb20bc6da6a18f21f2da546149b",
                "0x554e93eca9f192a07d5e1337b0d85a12abceb803",
                "0x90df68c6745ee7664ac35eae18d98bd0cf930cfb",
                "0x4e6da4b0bc7450a1ab5ff2e3812726a722a38a8b",
                "0xd7782b2c7da1098749aa63ef6107ee0c7a12adb6",
                "0x3c218c1fd6fd767e170b2a1476972b8f2a2d7e65"
            );

        public static readonly IReadOnlyDictionary<BackendPurpose, PlasmachainEndpointsConfiguration> EndpointConfigurations =
            new Dictionary<BackendPurpose, PlasmachainEndpointsConfiguration>
            {
                {
                    BackendPurpose.Local,
                    DevelopmentConfiguration
                },
                {
                    BackendPurpose.Development,
                    DevelopmentConfiguration
                },
                {
                    BackendPurpose.Staging,
                    StagingConfiguration
                },
                {
                    BackendPurpose.Production,
                    ProductionConfiguration
                }
            };
    }
}
