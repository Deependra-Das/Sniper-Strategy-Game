using UnityEngine;


namespace SniperStrategyGame.Event
{
    public class TogglePlyerShootAbilityEvent
    {
        public bool IsEnabled { get; }

        public TogglePlyerShootAbilityEvent(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}
