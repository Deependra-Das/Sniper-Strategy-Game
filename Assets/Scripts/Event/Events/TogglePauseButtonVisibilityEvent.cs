using UnityEngine;

namespace SniperStrategyGame.Event
{
    public class TogglePauseButtonVisibilityEvent
    {
        public bool IsVisible { get; }

        public TogglePauseButtonVisibilityEvent(bool isVisible)
        {
            IsVisible = isVisible;
        }
    }
}
