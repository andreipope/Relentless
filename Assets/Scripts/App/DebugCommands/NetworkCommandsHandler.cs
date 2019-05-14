using Loom.ZombieBattleground.BackendCommunication;
using Opencoding.CommandHandlerSystem;

namespace Loom.ZombieBattleground
{
    public class NetworkCommandsHandler
    {
        public static void Initialize()
        {
            CommandHandlers.RegisterCommandHandlers(typeof(NetworkCommandsHandler));
        }

        [CommandHandler(Description = "Disconnect from backend")]
        public static  void Disconnect()
        {
            GameClient.Get<BackendFacade>().Dispose();
        }
    }
}
