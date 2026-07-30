using System.Collections.Generic;
using UnityEngine;

namespace SniperStrategyGame.Enemy
{
    [CreateAssetMenu(fileName = "Enemy_SO", menuName = "ScriptableObjects/Enemy_SO")]
    public class Enemy_SO : ScriptableObject
    {
        public List<EnemyData> enemyDataList;
    }
}
