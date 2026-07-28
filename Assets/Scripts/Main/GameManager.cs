using SniperStrategyGame.Utilities;
using SniperStrategyGame.Event;
using SniperStrategyGame.SceneLoader;

namespace SniperStrategyGame.Main
{
    public class GameManager : GenericMonoSingleton<GameManager>
    {
        public ServiceLocator Services { get; private set; }
        private EventBusService _eventBus;
        private SceneLoaderService _sceneLoader;

        protected override void Awake()
        {
            base.Awake();
            InitializeServices();
            RegisterServices();
        }

        private void InitializeServices()
        {
            Services = new ServiceLocator();
            _eventBus = new EventBusService();
            _sceneLoader = new SceneLoaderService(_eventBus);
        }

        private void RegisterServices()
        {
            Services.Register(_eventBus);
            Services.Register(_sceneLoader);
        }

    }
}