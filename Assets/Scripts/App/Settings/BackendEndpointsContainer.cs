using System.Collections.Generic;

namespace LoomNetwork.CZB.BackendCommunication
{
    public static class BackendEndpointsContainer
    {
        public static readonly Dictionary<BackendPurpose, BackendEndpoint> Endpoints = 
            new Dictionary<BackendPurpose, BackendEndpoint>
            {
                {
                    BackendPurpose.Local,
                    new BackendEndpoint(
                        "http://stage.loom.games",
                        "ws://127.0.0.1:9999/queryws",
                        "ws://127.0.0.1:46657/websocket"
                    )
                },
                {
                    BackendPurpose.Staging,
                    new BackendEndpoint(
                        "http://loom.games",
                        // FIXME
                        "ws://127.0.0.2:9999/queryws",
                        "ws://127.0.0.2:46657/websocket"
                        /*"ws://battleground-testnet-asia1.dappchains.com:9999/queryws", 
                        "ws://battleground-testnet-asia1.dappchains.com:46657/websocket"*/
                    )
                },
                {
                    BackendPurpose.Public,
                    new BackendEndpoint(
                        "http://loom.games",
                        "ws://battleground-testnet-asia1.dappchains.com:9999/queryws",
                        "ws://battleground-testnet-asia1.dappchains.com:46657/websocket"
                    )
                }
            };
        
        public class BackendEndpoint
        {
            public string AuthHost { get; }
            public string ReaderHost { get; }
            public string WriterHost { get; }

            public BackendEndpoint(string authHost, string readerHost, string writerHost)
            {
                AuthHost = authHost;
                ReaderHost = readerHost;
                WriterHost = writerHost;
            }
        }
    }
}
