using System.Collections.Generic;
using UnityEngine;

namespace SniperStrategyGame.Tutorial
{
    [System.Serializable]
    public class TutorialGroupData
    {
        [Header("TutorialGroup")]
        public string tutorialGroupName;

        [TextArea(2, 4)]
        public string tutorialGoalInfo;

        [Header("Tutorial Steps")]
        public List<TutorialStepData> tutorialStepsList = new();
    }
}