using UnityEngine;

namespace SniperStrategyGame.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayStageMissionGoal_SO", menuName = "ScriptableObjects/GameplayStageMissionGoal_SO")]
    public class GameplayStageMissionGoal_SO : ScriptableObject
    {
        [Header("Mission Name")]
        public string missionName;

        [Header("Mission Goal Info")]
        public string missionGoalInfo;
    }
}