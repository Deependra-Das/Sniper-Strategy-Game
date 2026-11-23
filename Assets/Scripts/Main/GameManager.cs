using SniperStrategyGame.Bullet;
using SniperStrategyGame.Enemy;
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
        [SerializeField] private Enemy_SO _enemySO;

        public ServiceLocator Services { get; private set; }
        private EventBusService _eventBusService;
        private SceneLoaderService _sceneLoaderService;
        private BulletService _bulletService;
        private EnemyService _enemyService;

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
            _eventBusService = new EventBusService();
            _sceneLoaderService = new SceneLoaderService(_eventBusService);
            _bulletService = new BulletService(_bulletPrefab);
            _enemyService = new EnemyService(_enemySO, _eventBusService);
        }

        private void RegisterServices()
        {
            Services.Register(_eventBusService);
            Services.Register(_sceneLoaderService);
            Services.Register(_bulletService);
            Services.Register(_enemyService);
        }

        private IEnumerator LoadMainMenuScene()
        {
            yield return new WaitForSeconds(_sceneLoadDelay);
            Services.Get<SceneLoaderService>().LoadScene(SceneNameEnum.MainMenu);
        }
    }
}