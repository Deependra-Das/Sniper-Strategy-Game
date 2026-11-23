using UnityEngine;

namespace SniperStrategyGame.Event
{
    public class TutorialGroupCompletedEvent
    {
        public readonly string CompletedGroupName;
        public readonly string NextGroupName;

        public TutorialGroupCompletedEvent(string groupName, string nextGroupName)
        {
            CompletedGroupName = groupName;
            NextGroupName = nextGroupName;
        }
    }
}
