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
                        "http://stage-auth.loom.games",
                        "ws://127.0.0.1:9999/queryws",
                        "ws://127.0.0.1:46657/websocket",
                        "https://stage-vault.delegatecall.com/v1",
                        "v3",
                        false,
                        false
                    )
                },
                {
                    BackendPurpose.Staging,
                    new BackendEndpoint(
                        "http://stage-auth.loom.games",
                        "ws://battleground-testnet-asia2.dappchains.com:9999/queryws",
                        "ws://battleground-testnet-asia2.dappchains.com:46657/websocket",
                        "https://stage-vault.delegatecall.com/v1",
                        "v3",
                        false,
                        false
                    )
                },
                {
                    BackendPurpose.Production,
                    new BackendEndpoint(
                        "http://auth.loom.games",
                        "ws://gamechain.dappchains.com:9999/queryws",
                        "ws://gamechain.dappchains.com:46657/websocket",
                        "https://vault.delegatecall.com/v1",
                        "v3",
                        true,
                        false
                    )
                },
                {
                    BackendPurpose.Rebalance,
                    new BackendEndpoint(
                        "http://stage-auth.loom.games",
                        "ws://battleground-testnet-asia2.dappchains.com:9999/queryws",
                        "ws://battleground-testnet-asia2.dappchains.com:46657/websocket",
                        "https://stage-vault.delegatecall.com/v1",
                        "v4",
                        false,
                        false
                    )
                }
            };
    }
}
