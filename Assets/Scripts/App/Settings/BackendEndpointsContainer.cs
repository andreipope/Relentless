using System.Collections.Generic;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public static class BackendEndpointsContainer
    {
        public static readonly string CurrentStagingDataVersion = "v23";

        public static readonly IReadOnlyDictionary<BackendPurpose, BackendEndpoint> Endpoints =
            new Dictionary<BackendPurpose, BackendEndpoint>
            {
                {
                    BackendPurpose.Local,
                    new BackendEndpoint(
                        "https://stage-auth.loom.games",
                        "ws://127.0.0.1:46658/queryws",
                        "ws://127.0.0.1:46658/websocket",
                        "https://stage-vault.delegatecall.com/v1",
                        CurrentStagingDataVersion,
                        false,
                        false,
                        false,
                        PlasmaChainEndpointConfigurationsContainer.EndpointConfigurations[BackendPurpose.Local]
                    )
                },
                {
                    BackendPurpose.Development,
                    new BackendEndpoint(
                        "https://stage-auth.loom.games",
                        "ws://battleground-testnet-asia2.dappchains.com:46658/queryws",
                        "ws://battleground-testnet-asia2.dappchains.com:46658/websocket",
                        "https://stage-vault.delegatecall.com/v1",
                        CurrentStagingDataVersion,
                        false,
                        false,
                        false,
                        PlasmaChainEndpointConfigurationsContainer.EndpointConfigurations[BackendPurpose.Development]
                    )
                },
                {
                    BackendPurpose.Staging,
                    new BackendEndpoint(
                        "https://stage-auth.loom.games",
                        "ws://gamechain-staging.dappchains.com:46658/queryws",
                        "ws://gamechain-staging.dappchains.com:46658/websocket",
                        "https://stage-vault.delegatecall.com/v1",
                        CurrentStagingDataVersion,
                        false,
                        false,
                        false,
                        PlasmaChainEndpointConfigurationsContainer.EndpointConfigurations[BackendPurpose.Staging]
                    )
                },
                {
                    BackendPurpose.Production,
                    new BackendEndpoint(
                        "https://auth.loom.games",
                        "ws://gamechain.dappchains.com:46658/queryws",
                        "ws://gamechain.dappchains.com:46658/websocket",
                        "https://vault.delegatecall.com/v1",
                        "v7",
                        false,
                        false,
                        true,
                        PlasmaChainEndpointConfigurationsContainer.EndpointConfigurations[BackendPurpose.Production]
                    )
                }
            };
    }
}
