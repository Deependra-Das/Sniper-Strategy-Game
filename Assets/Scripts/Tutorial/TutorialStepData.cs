using SniperStrategyGame.Enemy;
using System.Collections.Generic;
using UnityEngine;

namespace SniperStrategyGame.Tutorial
{
    [System.Serializable]
    public class TutorialStepData
    {
        [Header("Expected Action")]
        public TutorialActionEnum tutorialAction;

        [Header("Enemies Required For This Step")]
        public List<EnemyTypeEnum> requiredEnemyTypeList;

        [Header("UI")]
        public string title;

        [TextArea(3, 6)]
        public string description;

        [TextArea(2, 4)]
        public string instruction;
    }
}