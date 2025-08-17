using System;
using System.Collections.Generic;

namespace PathOfFaith.Fondation.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _map = new();

        public static void Register<T>(T instance) where T : class
        {
            _map[typeof(T)] = instance;
        }

        public static bool TryGet<T>(out T instance) where T : class
        {
            if (_map.TryGetValue(typeof(T), out var obj) && obj is T cast)
            {
                instance = cast;
                return true;
            }
            instance = null;
            return false;
        }

        public static T Get<T>() where T : class
        {
            if (TryGet<T>(out var i)) return i;
            throw new InvalidOperationException($"[ServiceLocator] Service non enregistré: {typeof(T).Name}");
        }
    }
}
