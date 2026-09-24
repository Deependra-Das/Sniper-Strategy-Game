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
        public List<EnemyTypeEnum> requiredEnemyTypeList;

        [TextArea(2, 4)]
        public string instruction;

        [Header("Input")]
        [Tooltip("Optional button/key map icon displayed with the instruction.")]
        public Sprite instructionButtonMapSprite;

        public int instructionContainerSize;
        public bool showOverlay = false;
        public bool allowShooting = false;
    }
}