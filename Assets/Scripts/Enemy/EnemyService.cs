using SniperStrategyGame.Event;
using System.Collections.Generic;
using UnityEngine;

namespace SniperStrategyGame.Enemy
{
    public class EnemyService
    {
        private Dictionary<EnemyTypeEnum, BaseEnemy> _enemyPrefabLookup;

        public EnemyService(Enemy_SO enemySO, EventBusService eventBus) 
        {
            CreateEnemyLookup(enemySO);
        }

        private void CreateEnemyLookup(Enemy_SO enemySO)
        {
            _enemyPrefabLookup = new Dictionary<EnemyTypeEnum, BaseEnemy>();

            foreach (var enemyData in enemySO.enemyDataList)
            {
                if (_enemyPrefabLookup.ContainsKey(enemyData.enemyType))
                {
                    Debug.LogError($"Duplicate enemy type: {enemyData.enemyType}");
                    continue;
                }

                _enemyPrefabLookup.Add(enemyData.enemyType, enemyData.enemyPrefab);
            }
        }

        public bool TryGetEnemyPrefab(EnemyTypeEnum enemyType, out BaseEnemy enemyPrefab)
        {
            return _enemyPrefabLookup.TryGetValue(enemyType, out enemyPrefab);
        }

    }
}
