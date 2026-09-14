using UnityEngine;

namespace SniperStrategyGame.Event
{
    public class TogglePlayerInputEvent
    {
        public bool IsEnabled { get; }

        public TogglePlayerInputEvent(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}
