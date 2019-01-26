namespace Loom.ZombieBattleground.BackendCommunication
{
    public class BackendEndpoint
    {
        public BackendEndpoint(string authHost, string readerHost, string writerHost, string vaultHost, string dataVersion, bool isMaintenanceMode, bool isForceUpdate, bool isConnectionImpossible)
        {
            AuthHost = authHost;
            ReaderHost = readerHost;
            WriterHost = writerHost;
            VaultHost = vaultHost;
            DataVersion = dataVersion;
            IsMaintenaceMode = isMaintenanceMode;
            IsForceUpdate = isForceUpdate;
            IsConnectionImpossible = isConnectionImpossible;
        }

        public string AuthHost { get; set; }

        public string ReaderHost { get; set; }

        public string WriterHost { get; set; }

        public string VaultHost { get; set; }

        public string DataVersion { get; set; }

        public bool IsMaintenaceMode { get; set; }

        public bool IsForceUpdate { get; set; }

        public bool IsConnectionImpossible { get; set; }

        public BackendEndpoint(BackendEndpoint source)
        {
            AuthHost = source.AuthHost;
            ReaderHost = source.ReaderHost;
            WriterHost = source.WriterHost;
            VaultHost = source.VaultHost;
            DataVersion = source.DataVersion;
            IsMaintenaceMode = source.IsMaintenaceMode;
            IsForceUpdate = source.IsForceUpdate;
            IsConnectionImpossible = source.IsConnectionImpossible;
        }
    }
}
