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
                        "ws://127.0.0.1:46657/websocket"
                        )
                },
                {
                    BackendPurpose.PvP,
                    new BackendEndpoint(
                        "http://stage-auth.loom.games",
                        "ws://battleground-testnet-asia2.dappchains.com:9999/queryws",
                        "ws://battleground-testnet-asia2.dappchains.com:46657/websocket"
                    )
                },
                {
                    BackendPurpose.Staging,
                    new BackendEndpoint(
                        "http://stage-auth.loom.games",
                        "ws://gamechain-2.dappchains.com:9999/queryws",
                        "ws://gamechain-2.dappchains.com:46657/websocket"
                    )
                },
                {
                    BackendPurpose.Production,
                    new BackendEndpoint(
                        "http://auth.loom.games",
                        "ws://gamechain.dappchains.com:9999/queryws",
                        "ws://gamechain.dappchains.com:46657/websocket"
                    )
                }
            };

        public class BackendEndpoint
        {

            public BackendEndpoint(string authHost, string readerHost, string writerHost)
            {
                AuthHost = authHost;
                ReaderHost = readerHost;
                WriterHost = writerHost;
            }

            public string AuthHost { get; }

            public string ReaderHost { get; }

            public string WriterHost { get; }
        }
    }
}
