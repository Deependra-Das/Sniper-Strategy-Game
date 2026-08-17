using SniperStrategyGame.Enemy;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using SniperStrategyGame.Path;
using System.Collections.Generic;
using UnityEngine;

namespace SniperStrategyGame.Gameplay
{
    public class GameplayManager : MonoBehaviour
    {
        [SerializeField] private Enemy_SO _enemySO;
        [SerializeField] private List<Transform> _guardSpawnPointList;
        [SerializeField] private List<Transform> _patrolSpawnPointList;
        [SerializeField] private List<Transform> _shieldSpawnPointList;
        [SerializeField] private List<PatrolPath> _patrolPathList;

        private Dictionary<EnemyTypeEnum, BaseEnemy> _enemyPrefabLookup;
        private readonly List<BaseEnemy> _aliveEnemies = new();
        private EventBusService _eventBusServiceObj;

        private void Awake()
        {
            CreateEnemyLookup();
        }

        private void CreateEnemyLookup()
        {
            _enemyPrefabLookup = new Dictionary<EnemyTypeEnum, BaseEnemy>();

            foreach (var enemyData in _enemySO.enemyDataList)
            {
                if (_enemyPrefabLookup.ContainsKey(enemyData.enemyType))
                {
                    Debug.LogError($"Duplicate enemy type: {enemyData.enemyType}");
                    continue;
                }

                _enemyPrefabLookup.Add(enemyData.enemyType, enemyData.enemyPrefab);
            }
        }

        private void Start()
        {
            _eventBusServiceObj = GameManager.Instance.Services.Get<EventBusService>();
            SpawnEnemies();
            RaiseActivateEnemiesEvent();
            RaiseActivatePlayerTeleportAbilityEvent();
        }

        private void SpawnEnemies()
        {
            SpawnEnemyGroup(EnemyTypeEnum.Guard, _guardSpawnPointList);
            SpawnEnemyGroup(EnemyTypeEnum.Shield, _shieldSpawnPointList);
            SpawnEnemyGroup(EnemyTypeEnum.Patrol, _patrolSpawnPointList);
        }

        private void SpawnEnemyGroup(EnemyTypeEnum enemyType, List<Transform> spawnPointList)
        {
            if (!_enemyPrefabLookup.TryGetValue(enemyType, out BaseEnemy enemyPrefab))
            {
                Debug.LogError($"Missing prefab for {enemyType}");
                return;
            }
            for (int j = 0; j < spawnPointList.Count; j++)
            {
                Transform spawnPoint = spawnPointList[j];

                BaseEnemy enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                enemy.Initialize(this);

                if (enemy is PatrolEnemy patrolEnemy)
                {
                    patrolEnemy.SetPatrolPath(GetPatrolPath(j));
                }

                _aliveEnemies.Add(enemy);
                RaiseEnemySpawnedEvent(enemy);
            }
        }

        private PatrolPath GetPatrolPath(int index)
        {
            if (index >= _patrolPathList.Count)
            {
                Debug.LogWarning("Missing patrol path");
                return null;
            }

            return _patrolPathList[index];
        }

        public void EnemyDied(BaseEnemy enemy)
        {
            if (!_aliveEnemies.Remove(enemy))
                return;

            Destroy(enemy.gameObject);

            if (_aliveEnemies.Count == 0)
            {
                StageCompleted();
            }
        }

        private void StageCompleted()
        {
            Debug.Log("Stage Complete");
        }

        private void RaiseEnemySpawnedEvent(BaseEnemy enemy)
        {
            _eventBusServiceObj.Publish(new EnemySpawnedEvent(enemy));
        }

        private void RaiseActivateEnemiesEvent()
        {
            _eventBusServiceObj.Publish(new ActivateEnemiesEvent());
        }

        private void RaiseActivatePlayerTeleportAbilityEvent()
        {
            _eventBusServiceObj.Publish(new ActivatePlayerTeleportAbilityEvent());
        }
    }
}
