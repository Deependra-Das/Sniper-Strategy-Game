using System.Collections.Generic;
using UnityEngine;

namespace SniperStrategyGame.Tutorial
{
    [CreateAssetMenu(fileName = "Tutorial_SO", menuName = "ScriptableObjects/Tutorial_SO")]
    public class Tutorial_SO : ScriptableObject
    {
        public List<TutorialStepData> tutorialStepsList = new();
    }
}