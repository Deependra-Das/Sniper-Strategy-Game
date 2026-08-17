using UnityEngine;
using SniperStrategyGame.Enemy;

namespace SniperStrategyGame.Event
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