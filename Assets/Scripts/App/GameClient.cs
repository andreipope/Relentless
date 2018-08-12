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
            AddService<BackendFacade>(new BackendFacade());
            AddService<ActionLogCollectorUploader>(new ActionLogCollectorUploader());
        }

        public static T Get<T>()
        {
            return Instance.GetService<T>();
        }
    }
}