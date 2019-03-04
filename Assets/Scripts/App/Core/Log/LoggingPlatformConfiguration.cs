using log4net.Core;
using Loom.ZombieBattleground.BackendCommunication;
using UnityEditor.Callbacks;
using UnityEngine;
using Logger = log4net.Repository.Hierarchy.Logger;

namespace Loom.ZombieBattleground
{
    public static class LoggingPlatformConfiguration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [DidReloadScripts]
        public static void Setup()
        {
            Logging.Setup();

#if UNITY_EDITOR && !FORCE_ENABLE_ALL_LOGS
            // Disable non-essential logs in Editor
            Logger backendFacadeRpc = Logging.GetLogger(nameof(BackendFacade) + "Rpc");
            backendFacadeRpc.Level = Level.Warn;

            Logger timeMetricsContractCallProxy = Logging.GetLogger(nameof(TimeMetricsContractCallProxy));
            timeMetricsContractCallProxy.Level = Level.Warn;
#endif
        }
    }
}
