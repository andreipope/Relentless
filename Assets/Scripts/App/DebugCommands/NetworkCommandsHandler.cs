using Loom.ZombieBattleground.BackendCommunication;

namespace Loom.ZombieBattleground
{
    public class NetworkCommandsHandler
    {
        public static void Initialize()
        {
        }

        public static  void Disconnect()
        {
            GameClient.Get<BackendFacade>().Dispose();
        }
    }
}
