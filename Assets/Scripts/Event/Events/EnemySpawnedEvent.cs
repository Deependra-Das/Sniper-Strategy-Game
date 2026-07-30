using UnityEngine;

namespace SniperStrategyGame.Enemy
{
    public struct EnemySpawnedEvent
    {
        public BaseEnemy Enemy { get; }

        public EnemySpawnedEvent(BaseEnemy enemy)
        {
            Enemy = enemy;
        }
    }
}