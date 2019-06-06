using System.Linq;
using log4net.Core;
using log4net.Filter;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Iap;
using UnityEngine;
using Logger = log4net.Repository.Hierarchy.Logger;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

namespace Loom.ZombieBattleground
{
    public static class LoggingPlatformConfiguration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [DidReloadScripts]
#endif
        public static void Setup()
        {
            Logging.Configure();

            if (Logging.NonEssentialLogsDisabled)
            {
                Logger backendFacadeRpc = Logging.GetLogger(nameof(BackendFacade) + "Rpc");
                backendFacadeRpc.Level = Level.Warn;

                Logger plasmaChainBackendFacade = Logging.GetLogger(nameof(PlasmaChainBackendFacade) + "Rpc");
                plasmaChainBackendFacade.Level = Level.Warn;

                Logger customContractCallProxy = Logging.GetLogger(nameof(CustomContractCallProxy));
                customContractCallProxy.Level = Level.Warn;

                Logger callExecutionTrace = Logging.GetLogger("CallExecutionTrace");
                callExecutionTrace.Level = Level.Warn;
            }
        }

        public static IFilter[] CreateSpammyLogsFilters()
        {
            string[] strings =
            {
                "A ping was received.",
                "A pong to this ping has been sent.",
                "The current output action has been changed",
                "Not a WebSocket handshake response",
                "Could not produce class with ID" // harmless (for now) side effect of Strip Engine Code
            };

            return
                strings
                    .Select(s => new StringMatchFilter
                    {
                        StringToMatch = s,
                        AcceptOnMatch = false
                    })
                    .Cast<IFilter>()
                    .ToArray();
        }
    }
}
