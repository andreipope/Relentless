using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;

namespace Loom.ZombieBattleground
{
    public class GameClient : ServiceLocatorBase
    {
        private static readonly object Sync = new object();

        private static GameClient _instance;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameClient" /> class.
        /// </summary>
        internal GameClient()
        {
#if (UNITY_EDITOR || USE_LOCAL_BACKEND) && !USE_PRODUCTION_BACKEND && !USE_STAGING_BACKEND
            const BackendPurpose backend = BackendPurpose.Local;
#elif USE_PRODUCTION_BACKEND
            const BackendPurpose backend = BackendPurpose.Production;
#else
            const BackendPurpose backend = BackendPurpose.Staging;
#endif

            BackendEndpointsContainer.BackendEndpoint backendEndpoint = BackendEndpointsContainer.Endpoints[backend];
            LoadObjectsManager loadObjectsManager = new LoadObjectsManager();
            loadObjectsManager.LoadAssetBundleFromFile(Constants.AssetBundleMain);

            AddService<IApplicationSettingsManager>(new ApplicationSettingsManager());
            AddService<ILoadObjectsManager>(loadObjectsManager);
            AddService<ITimerManager>(new TimerManager());
            AddService<IInputManager>(new InputManager());
            AddService<ILocalizationManager>(new LocalizationManager());
            AddService<IDataManager>(new DataManager());
            AddService<IScenesManager>(new ScenesManager());
            AddService<IAppStateManager>(new AppStateManager());
            AddService<ICameraManager>(new CameraManager());
            AddService<IPlayerManager>(new PlayerManager());
            AddService<ISoundManager>(new SoundManager());
            AddService<INavigationManager>(new NavigationManager());
            AddService<IGameplayManager>(new GameplayManager());
            AddService<IOverlordManager>(new OverlordManager());
            AddService<ITutorialManager>(new TutorialManager());
            AddService<IMatchManager>(new MatchManager());
            AddService<IUIManager>(new UIManager());
            AddService<BackendFacade>(new BackendFacade(backendEndpoint.AuthHost, backendEndpoint.ReaderHost, backendEndpoint.WriterHost));
            AddService<ActionCollectorUploader>(new ActionCollectorUploader());
            AddService<BackendDataControlMediator>(new BackendDataControlMediator());
            AddService<IAnalyticsManager>(new AnalyticsManager());
            AddService<IPvPManager>(new PvPManager());
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
