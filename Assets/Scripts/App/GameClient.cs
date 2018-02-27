using GrandDevs.CZB.Gameplay;

namespace GrandDevs.CZB
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
            AddService<IUIManager>(new UIManager());
            AddService<ICameraManager>(new CameraManager()); 
            AddService<IPlayerManager>(new PlayerManager());
            AddService<ISoundManager>(new SoundManager());
            AddService<INotificationManager>(new NotificationManager());         
            AddService<IScreenOrientationManager>(new ScreenOrientationManager());
            AddService<INavigationManager>(new NavigationManager()); 
        }

        public static T Get<T>()
        {
            return Instance.GetService<T>();
        }
    }
}