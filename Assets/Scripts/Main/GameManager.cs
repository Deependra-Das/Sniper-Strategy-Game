using SniperStrategyGame.Utilities;
using SniperStrategyGame.Event;

namespace SniperStrategyGame.Main
{
    public class GameManager : GenericMonoSingleton<GameManager>
    {
        public ServiceLocator Services { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            InitializeServices();
        }

        private void InitializeServices()
        {
            Services = new ServiceLocator();

            Services.Register(new EventBusService());
        }
    }
}