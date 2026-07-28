using System;
using System.Collections.Generic;
using UnityEngine;

namespace SniperStrategyGame.Main
{
    public class ServiceLocator
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<T>(T service)
        {
            var type = typeof(T);

            if (_services.ContainsKey(type))
                throw new InvalidOperationException($"Service {type.Name} already registered.");

            _services[type] = service;
        }

        public T Get<T>()
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
                return (T)service;

            throw new InvalidOperationException($"Service {type.Name} not registered.");
        }

        public void Unregister<T>()
        {
            _services.Remove(typeof(T));
        }
    }
}