using System;
using UnityEngine;

namespace SniperStrategyGame.Enemy
{
    [Serializable]
    public class EnemyData
    {
        public EnemyTypeEnum enemyType;
        public BaseEnemy enemyPrefab;
    }
}
