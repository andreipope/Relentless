using System.Collections.Generic;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public static class BackendEndpointsContainer
    {
        public static readonly Dictionary<BackendPurpose, BackendEndpoint> Endpoints =
            new Dictionary<BackendPurpose, BackendEndpoint>
            {
                {
                    BackendPurpose.Local,
                    new BackendEndpoint(
                        "https://stage-auth.loom.games",
                        "ws://127.0.0.1:9999/queryws",
                        "ws://127.0.0.1:46657/websocket",
                        "https://stage-vault.delegatecall.com/v1",
                        "v3",
                        false,
                        false,
                        false
                    )
                },
                {
                    BackendPurpose.BranchTesting,
                    new BackendEndpoint(
                        "https://stage-auth.loom.games",
                        "ws://gamechain-2.dappchains.com:9999/queryws",
                        "ws://gamechain-2.dappchains.com:46657/rpc",
                        "https://stage-vault.delegatecall.com/v1",
                        "v3",
                        false,
                        false,
                        false
                    )
                },
                {
                    BackendPurpose.Staging,
                    new BackendEndpoint(
                        "https://stage-auth.loom.games",
                        "ws://gamechain-staging.dappchains.com:9999/queryws",
                        "ws://gamechain-staging.dappchains.com:46657/websocket",
                        "https://stage-vault.delegatecall.com/v1",
                        "v3",
                        false,
                        false,
                        false
                    )
                },
                {
                    BackendPurpose.Production,
                    new BackendEndpoint(
                        "https://auth.loom.games",
                        "ws://gamechain.dappchains.com:9999/queryws",
                        "ws://gamechain.dappchains.com:46657/rpc",
                        "https://vault.delegatecall.com/v1",
                        "v5",
                        false,
                        false,
                        true
                    )
                },
                {
                    BackendPurpose.Rebalance,
                    new BackendEndpoint(
                        "https://stage-auth.loom.games",
                        "ws://gamechain-staging.dappchains.com:9999/queryws",
                        "ws://gamechain-staging.dappchains.com:46657/websocket",
                        "https://stage-vault.delegatecall.com/v1",
                        "v4",
                        false,
                        false,
                        false
                    )
                }
            };
    }
}
