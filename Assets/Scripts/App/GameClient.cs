// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.BackendCommunication;
using LoomNetwork.CZB.Gameplay;

namespace LoomNetwork.CZB
{
    public class GameClient : ServiceLocatorBase
    {
        private static object _sync = new object();

        private static GameClient _Instance;
        public static GameClient Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (_sync)
                    {
                        _Instance = new GameClient();
                    }
                }
                return _Instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameClient"/> class.
        /// </summary>
        internal GameClient() : base()
        {
#if (UNITY_EDITOR || USE_LOCAL_BACKEND) && !USE_PUBLIC_BACKEND && !USE_STAGING_BACKEND
            BackendPurpose backend = BackendPurpose.Local;
#elif USE_PUBLIC_BACKEND
            BackendPurpose backend = BackendPurpose.Public;
#else
            BackendPurpose backend = BackendPurpose.Staging;
#endif

            BackendEndpointsContainer.BackendEndpoint backendEndpoint = BackendEndpointsContainer.Endpoints[backend];

            AddService<ITimerManager>(new TimerManager());
            AddService<ILoadObjectsManager>(new LoadObjectsManager());
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
            AddService<IContentManager>(new ContentManager());
            AddService<ITutorialManager>(new TutorialManager());
            AddService<IMatchManager>(new MatchManager());
            AddService<IUIManager>(new UIManager());
            AddService<BackendFacade>(new BackendFacade(
                backendEndpoint.AuthHost,
                backendEndpoint.ReaderHost,
                backendEndpoint.WriterHost
                ));
            AddService<ActionLogCollectorUploader>(new ActionLogCollectorUploader());
            AddService<BackendDataControlMediator>(new BackendDataControlMediator());
            AddService<IAnalyticsManager>(new AnalyticsManager());
        }

        public static T Get<T>()
        {
            return Instance.GetService<T>();
        }
    }
}