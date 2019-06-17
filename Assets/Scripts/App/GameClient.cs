using System;
using System.IO;
using DG.Tweening;
using log4net;
using log4net.Core;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Iap;
using Newtonsoft.Json;
using UnityEngine;
using Logger = log4net.Repository.Hierarchy.Logger;

namespace Loom.ZombieBattleground
{
    public class GameClient : ServiceLocatorBase
    {
        private static readonly ILog Log = Logging.GetLog(nameof(GameClient));

        public event Action ServicesInitialized;

        public bool UpdateServices { get; set; } = true;

        private static GameClient _instance;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameClient" /> class.
        /// </summary>
        internal GameClient()
        {
            Log.Info($"Starting game, version {BuildMetaInfo.Instance.FullVersionName} {BuildMetaInfo.Instance.GitBranchName}");

            DOTween.KillAll();
            LoadObjectsManager loadObjectsManager = new LoadObjectsManager();
            loadObjectsManager.LoadAssetBundleFromFile(Constants.AssetBundleMain);

            BackendEndpoint backendEndpoint = GetDefaultBackendEndpoint();

            Func<RawChainEventContract, IContractCallProxy> contractCallProxyFactory =
                contract => new ThreadedContractCallProxyWrapper(new CustomContractCallProxy(contract, true, true));

            AddService<IApplicationSettingsManager>(new ApplicationSettingsManager());
            AddService<ILoadObjectsManager>(loadObjectsManager);
            AddService<ITimerManager>(new TimerManager());
            AddService<IInputManager>(new InputManager());
            AddService<ILocalizationManager>(new LocalizationManager());
            AddService<IScenesManager>(new ScenesManager());
            AddService<IAppStateManager>(new AppStateManager());
            AddService<ICameraManager>(new CameraManager());
            AddService<IPlayerManager>(new PlayerManager());
            AddService<ISoundManager>(new SoundManager());
            AddService<INavigationManager>(new NavigationManager());
            AddService<IGameplayManager>(new GameplayManager());
            AddService<IOverlordExperienceManager>(new OverlordExperienceManager());
            AddService<ITutorialManager>(new TutorialManager());
            AddService<IMatchManager>(new MatchManager());
            AddService<IUIManager>(new UIManager());
            AddService<IDataManager>(new DataManager(GetConfigData()));
            AddService<ActionCollectorUploader>(new ActionCollectorUploader());
            AddService<BackendDataControlMediator>(new BackendDataControlMediator());
            AddService<IFacebookManager>(new FacebookManager());
            AddService<IAnalyticsManager>(new AnalyticsManager());
            AddService<IPvPManager>(new PvPManager());
            AddService<BackendFacade>(
                new BackendFacade(
                    backendEndpoint,
                    contractCallProxyFactory,
                    Logging.GetLog(nameof(BackendFacade)),
                    Logging.GetLog(nameof(BackendFacade) + "Rpc")
                ));
            AddService<INetworkActionManager>(new NetworkActionManager());
            AddService<DebugCommandsManager>(new DebugCommandsManager());
            AddService<PushNotificationManager>(new PushNotificationManager());
            AddService<AuthFiatApiFacade>(new AuthFiatApiFacade());
            AddService<IIapPlatformStoreFacade>(new IapPlatformStoreFacade());
            AddService<IapMediator>(new IapMediator());
            AddService<PlasmaChainBackendFacade>(new PlasmaChainBackendFacade());
            AddService<TutorialRewardManager>(new TutorialRewardManager());
        }

        public override void InitServices() {
            base.InitServices();

            ServicesInitialized?.Invoke();
        }

        public override void Update()
        {
            if (!UpdateServices)
                return;

            base.Update();
        }

        public static BackendEndpoint GetDefaultBackendEndpoint()
        {
            ConfigData configData = GetConfigData();
            if (configData.Backend != null)
            {
                return configData.Backend;
            }

            BackendPurpose backend = GetDefaultBackendPurpose();

#if UNITY_EDITOR
            const string envVarBackendEndpointName = "ZB_BACKEND_ENDPOINT_NAME";
            string backendString = Environment.GetEnvironmentVariable(envVarBackendEndpointName);
            if (!String.IsNullOrEmpty(backendString))
            {
                backend = (BackendPurpose) Enum.Parse(typeof(BackendPurpose), backendString);
            }
#endif

            BackendEndpoint backendEndpoint = BackendEndpointsContainer.Endpoints[backend];
            return backendEndpoint;
        }

        public static BackendPurpose GetDefaultBackendPurpose()
        {
#if (UNITY_EDITOR || USE_LOCAL_BACKEND) && !USE_PRODUCTION_BACKEND && !USE_STAGING_BACKEND && !USE_BRANCH_TESTING_BACKEND && !USE_REBALANCE_BACKEND
            const BackendPurpose defaultBackend = BackendPurpose.Local;
#elif USE_PRODUCTION_BACKEND
            const BackendPurpose defaultBackend = BackendPurpose.Production;
#elif USE_BRANCH_TESTING_BACKEND
            const BackendPurpose defaultBackend = BackendPurpose.BranchTesting;
#else
            const BackendPurpose defaultBackend = BackendPurpose.Staging;
#endif
            BackendPurpose backend = defaultBackend;
            return backend;
        }

        public static GameClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameClient();
                }

                return _instance;
            }
        }

        public static T Get<T>()
        {
            return Instance.GetService<T>();
        }

        public static bool InstanceExists => _instance != null;

        public static void ClearInstance()
        {
            _instance = null;
        }

        private static ConfigData GetConfigData()
        {
            string configDataFilePath = Path.Combine(Application.persistentDataPath, Constants.LocalConfigDataFileName);
            if (File.Exists(configDataFilePath))
            {
                return JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(configDataFilePath));
            }

            return new ConfigData();
        }
    }
}
