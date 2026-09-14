using UnityEngine;


namespace SniperStrategyGame.Event
{
    public class ToggleCursorEvent
    {
        public bool IsEnabled { get; }

        public ToggleCursorEvent(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}