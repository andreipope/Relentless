using System.IO;
using System;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using Newtonsoft.Json;
using UnityEngine;
using System.Threading.Tasks;
using Plugins.AsyncAwaitUtil.Source;

namespace Loom.ZombieBattleground
{
    public class GameClient : ServiceLocatorBase
    {
        private static readonly object Sync = new object();

        private static GameClient _instance;

        private static BackendEndpoint backendEndpoint;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameClient" /> class.
        /// </summary>
        internal GameClient()
        {
            LoadObjectsManager loadObjectsManager = new LoadObjectsManager();
            loadObjectsManager.LoadAssetBundleFromFile(Constants.AssetBundleMain);

            PopulateBackendEndpoint();

            string configDataFilePath = Path.Combine(Application.persistentDataPath, Constants.LocalConfigDataFileName);
            ConfigData configData = new ConfigData();
            if (File.Exists(configDataFilePath))
            {
                configData = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(configDataFilePath));
                if (configData.Backend != null)
                {
                    Debug.Log("Backend overriden by config file.");
                    backendEndpoint = configData.Backend;
                }
            }

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
            AddService<IDataManager>(new DataManager(configData));
            AddService<BackendFacade>(new BackendFacade(backendEndpoint, contract => new ThreadedTimeMetricsContractCallProxy(contract, false, true)));
            AddService<ActionCollectorUploader>(new ActionCollectorUploader());
            AddService<BackendDataControlMediator>(new BackendDataControlMediator());
            AddService<IFacebookManager>(new FacebookManager());
            AddService<IAnalyticsManager>(new AnalyticsManager());
            AddService<IPvPManager>(new PvPManager());
            AddService<IQueueManager>(new QueueManager());
            AddService<DebugCommandsManager>( new DebugCommandsManager());
            AddService<PushNotificationManager>(new PushNotificationManager());
            AddService<FiatBackendManager>(new FiatBackendManager());
            AddService<FiatPlasmaManager>(new FiatPlasmaManager());
            AddService<OpenPackPlasmaManager>(new OpenPackPlasmaManager());
        }

        public static async void PopulateBackendEndpoint()
        {
#if (UNITY_EDITOR || USE_LOCAL_BACKEND) && !USE_PRODUCTION_BACKEND && !USE_STAGING_BACKEND && !USE_PVP_BACKEND && !USE_REBALANCE_BACKEND
            const BackendPurpose backend = BackendPurpose.Local;
#elif USE_PRODUCTION_BACKEND
            const BackendPurpose backend = BackendPurpose.Production;
            try 
            {
                backendEndpoint = await GetServerURLs();
            }
            catch (Exception e) 
            {
                Debug.LogWarning(e.Message);
            }
#elif USE_REBALANCE_BACKEND
            const BackendPurpose backend = BackendPurpose.Rebalance;
#else
            const BackendPurpose backend = BackendPurpose.Staging;
#endif
            backendEndpoint = backendEndpoint ?? BackendEndpointsContainer.Endpoints[backend];
        }

        private static async Task<BackendEndpoint> GetServerURLs()
        {
            const string queryURLsEndPoint = "/zbversion";

            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = "http://stage-auth.loom.games" + queryURLsEndPoint + "?version=" + Constants.CurrentVersionBase + "&environment=staging";

            Debug.Log(webrequestCreationInfo.Url);

            HttpResponseMessage httpResponseMessage =
                await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new Exception($"{nameof(GetServerURLs)} failed with error code {httpResponseMessage.StatusCode}");

            ServerUrlsResponse serverInfo = JsonConvert.DeserializeObject<ServerUrlsResponse>(
                httpResponseMessage.ReadToEnd()
            );

            return new BackendEndpoint(
                serverInfo.version.auth_url,
                serverInfo.version.read_url,
                serverInfo.version.write_url,
                serverInfo.version.vault_url,
                serverInfo.version.data_version,
                serverInfo.version.is_maintenace_mode,
                serverInfo.version.is_force_update
            );
        }

        private struct ServerUrlsResponse
        {
            public ServerUrlsData version;
        }

        private struct ServerUrlsData
        {
            public int id;
            public int major;
            public int minor;
            public int patch;
            public string environment;
            public string auth_url;
            public string read_url;
            public string write_url;
            public string vault_url;
            public string data_version;
            public bool is_maintenace_mode;
            public bool is_force_update;
        }

        public static BackendEndpoint GetBackendEndpoint(){
            return backendEndpoint;
        }

        public static GameClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Sync)
                    {
                        _instance = new GameClient();
                    }
                }

                return _instance;
            }
        }

        public static T Get<T>()
        {
            return Instance.GetService<T>();
        }
    }
}
