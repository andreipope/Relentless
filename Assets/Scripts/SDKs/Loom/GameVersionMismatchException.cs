using System;

namespace LoomNetwork.CZB.BackendCommunication
{
    public class GameVersionMismatchException : Exception
    {
        public string LocalVersion { get; }
        public string RemoteVersion { get; }

        public GameVersionMismatchException(string localVersion, string remoteVersion)
        {
            LocalVersion = localVersion;
            RemoteVersion = remoteVersion;
        }
    }
}
