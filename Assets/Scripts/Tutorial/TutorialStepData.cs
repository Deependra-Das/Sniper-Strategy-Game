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
        public string tutorialActionText;

        [Header("Enemies Required For This Step")]
        public List<TutorialEnemySpawnData> requiredEnemySpawnDataList;

        [TextArea(2, 4)]
        public string instruction;

        public int instructionContainerSize;
        public bool showOverlay = false;
        public bool allowShooting = false;
    }
}