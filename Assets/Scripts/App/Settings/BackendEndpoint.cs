namespace Loom.ZombieBattleground.BackendCommunication
{
    public class BackendEndpoint
    {
        public BackendEndpoint(string authHost, string readerHost, string writerHost, string dataVersion)
        {
            AuthHost = authHost;
            ReaderHost = readerHost;
            WriterHost = writerHost;
            DataVersion = dataVersion;
        }

        public string AuthHost { get; set; }

        public string ReaderHost { get; set; }

        public string WriterHost { get; set; }

        public string DataVersion { get; set; }

        public BackendEndpoint(BackendEndpoint source)
        {
            AuthHost = source.AuthHost;
            ReaderHost = source.ReaderHost;
            WriterHost = source.WriterHost;
            DataVersion = source.DataVersion;
        }
    }
}
