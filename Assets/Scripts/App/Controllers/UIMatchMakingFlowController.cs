using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;

namespace Loom.ZombieBattleground
{
    /// <summary>
    /// Executes the matchmaking sequences, notices about state changes and successful match.
    /// Shows an error dialog in case of unhandleable exceptions.
    /// </summary>
    public class UIMatchMakingFlowController : MatchMakingFlowController
    {
        public UIMatchMakingFlowController(
            BackendFacade backendFacade,
            UserDataModel userDataModel) : base(backendFacade, userDataModel)
        { }

        protected override Task ErrorSecondChanceHandler (Exception exception) {
            if (CancellationTokenSource.IsCancellationRequested)
                return Task.CompletedTask;

            GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(exception);
            return Task.CompletedTask;
        }
    }
}
