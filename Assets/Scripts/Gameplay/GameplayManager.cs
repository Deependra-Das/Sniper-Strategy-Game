using SniperStrategyGame.Enemy;
using SniperStrategyGame.Event;
using SniperStrategyGame.Main;
using System.Collections.Generic;
using UnityEngine;

namespace SniperStrategyGame.Gameplay
{
    public class GameplayManager : MonoBehaviour
    {
        [SerializeField] private Enemy_SO _enemySO;
        [SerializeField] private List<EnemySpawnData> _enemySpawns;

        private Dictionary<EnemyTypeEnum, BaseEnemy> _enemyPrefabLookup;
        private readonly List<BaseEnemy> _aliveEnemies = new();

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
            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            foreach (var spawnData in _enemySpawns)
            {
                if (!_enemyPrefabLookup.TryGetValue(spawnData.enemyType, out BaseEnemy enemyPrefab))
                {
                    Debug.LogError($"Enemy prefab not found for type {spawnData.enemyType}");
                    continue;
                }

                BaseEnemy enemy = Instantiate(enemyPrefab, spawnData.spawnPoint.position, spawnData.spawnPoint.rotation);

                enemy.Initialize(this);
                _aliveEnemies.Add(enemy);

                GameManager.Instance.Services.Get<EventBusService>().Publish(new EnemySpawnedEvent(enemy));
            }

            GameManager.Instance.Services.Get<EventBusService>().Publish(new ActivateEnemiesEvent());
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
    }
}
