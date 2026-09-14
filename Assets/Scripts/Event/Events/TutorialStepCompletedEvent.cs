using SniperStrategyGame.Tutorial;
using UnityEngine;

namespace SniperStrategyGame.Event
{
    public class TutorialStepCompletedEvent
    {
        public TutorialActionEnum TutorialAction { get; }

        public TutorialStepCompletedEvent(TutorialActionEnum tutorialAction)
        {
            TutorialAction = tutorialAction;
        }
    }
}
