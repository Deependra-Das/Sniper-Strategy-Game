using System;
using UnityEngine;

namespace SniperStrategyGame.Enemy
{
    [Serializable]    
    public class EnemySpawnData
    {
        public Transform spawnPoint;
        public EnemyTypeEnum enemyType;
    }
}
