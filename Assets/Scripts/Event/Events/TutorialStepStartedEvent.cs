using UnityEngine;
using SniperStrategyGame.Tutorial;

namespace SniperStrategyGame.Event
{
    public class TutorialStepStartedEvent
    {
        public readonly TutorialStepData TutorialStepData;

        public TutorialStepStartedEvent(TutorialStepData tutorialStepData)
        {
            TutorialStepData = tutorialStepData;
        }
    }
}
