using Loom.Newtonsoft.Json;

namespace LoomNetwork.CZB.BackendCommunication
{
    public class GlobalConfig
    {
        [JsonProperty(PropertyName = "id")]
        public int Id;
            
        [JsonProperty(PropertyName = "android_latest_version")]
        public string AndroidLatestVersion;
            
        [JsonProperty(PropertyName = "ios_latest_version")]
        public string IOSLatestVersion;
            
        [JsonProperty(PropertyName = "macos_latest_version")]
        public string MacOSLatestVersion;
            
        [JsonProperty(PropertyName = "linux_latest_version")]
        public string LinuxLatestVersion;
            
        [JsonProperty(PropertyName = "windows_latest_version")]
        public string WindowsLatestVersion;
    }
}