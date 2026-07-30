using SniperStrategyGame.Bullet;
using SniperStrategyGame.Event;
using SniperStrategyGame.SceneLoader;
using SniperStrategyGame.Utilities;
using System.Collections;
using UnityEngine;

namespace SniperStrategyGame.Main
{
    public class GameManager : GenericMonoSingleton<GameManager>
    {
        [SerializeField] private float _sceneLoadDelay = 1f;
        [SerializeField] private PlayerBullet _bulletPrefab;

        public ServiceLocator Services { get; private set; }
        private EventBusService _eventBus;
        private SceneLoaderService _sceneLoader;
        private BulletService _bulletService;

        protected override void Awake()
        {
            base.Awake();
            InitializeServices();
            RegisterServices();
        }

        private void Start()
        {
            StartCoroutine(LoadMainMenuScene());
        }

        private void InitializeServices()
        {
            Services = new ServiceLocator();
            _eventBus = new EventBusService();
            _sceneLoader = new SceneLoaderService(_eventBus);
            _bulletService = new BulletService(_bulletPrefab);
        }

        private void RegisterServices()
        {
            Services.Register(_eventBus);
            Services.Register(_sceneLoader);
            Services.Register(_bulletService);
        }

        private IEnumerator LoadMainMenuScene()
        {
            yield return new WaitForSeconds(_sceneLoadDelay);
            Services.Get<SceneLoaderService>().LoadScene(SceneNameEnum.MainMenu);
        }
    }
}