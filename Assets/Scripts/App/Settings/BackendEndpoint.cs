using System;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class BackendEndpoint
    {
        [JsonConstructor]
        public BackendEndpoint(
            string authHost,
            string readerHost,
            string writerHost,
            string vaultHost,
            string dataVersion,
            bool isMaintenanceMode,
            bool isForceUpdate,
            bool isConnectionImpossible,
            PlasmaChainEndpointsConfiguration plasmaChainEndpointsConfiguration)
        {
            AuthHost = authHost;
            ReaderHost = readerHost;
            WriterHost = writerHost;
            VaultHost = vaultHost;
            DataVersion = dataVersion;
            IsMaintenanceMode = isMaintenanceMode;
            IsForceUpdate = isForceUpdate;
            IsConnectionImpossible = isConnectionImpossible;
            PlasmaChainEndpointsConfiguration =
                plasmaChainEndpointsConfiguration ?? throw new ArgumentNullException(nameof(plasmaChainEndpointsConfiguration));
        }

        public string AuthHost { get; }

        public string ReaderHost { get; }

        public string WriterHost { get; }

        public string VaultHost { get; }

        public string DataVersion { get; }

        public bool IsMaintenanceMode { get; }

        public bool IsForceUpdate { get; }

        public bool IsConnectionImpossible { get; }

        public PlasmaChainEndpointsConfiguration PlasmaChainEndpointsConfiguration { get; }
    }
}
