using Loom.ZombieBattleground.BackendCommunication;

namespace Loom.ZombieBattleground.Data
{
    public class ConfigData
    {
        public bool EncryptData = true;
        public bool SkipBackendCardData;
        public bool EnablePvP;
        public BackendEndpoint Backend;
    }
}
