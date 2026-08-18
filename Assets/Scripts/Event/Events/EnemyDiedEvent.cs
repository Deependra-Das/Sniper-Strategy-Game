using UnityEngine;
using SniperStrategyGame.Enemy;

namespace SniperStrategyGame.Event
{
    public struct EnemyDiedEvent
    {
        public readonly BaseEnemy Enemy;

        public EnemyDiedEvent(BaseEnemy enemy)
        {
            Enemy = enemy;
        }
    }
}
