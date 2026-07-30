using System;
using System.Collections.Generic;

namespace SniperStrategyGame.Event
{
    public class EventBusService
    {
        private readonly Dictionary<Type, Delegate> _events = new();

        public void Subscribe<TEvent>(Action<TEvent> listener)
        {
            var eventType = typeof(TEvent);

            if (_events.TryGetValue(eventType, out var existing))
            {
                _events[eventType] = Delegate.Combine(existing, listener);
            }
            else
            {
                _events[eventType] = listener;
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> listener)
        {
            var eventType = typeof(TEvent);

            if (!_events.TryGetValue(eventType, out var existing))
                return;

            var current = Delegate.Remove(existing, listener);

            if (current == null)
                _events.Remove(eventType);
            else
                _events[eventType] = current;
        }

        public void Publish<TEvent>(TEvent eventData)
        {
            var eventType = typeof(TEvent);

            if (_events.TryGetValue(eventType, out var existing))
            {
                ((Action<TEvent>)existing)?.Invoke(eventData);
            }
        }

        public void Clear<TEvent>()
        {
            _events.Remove(typeof(TEvent));
        }


        public void ClearAll()
        {
            _events.Clear();
        }

        public bool HasSubscribers<TEvent>()
        {
            return _events.ContainsKey(typeof(TEvent));
        }
    }
}