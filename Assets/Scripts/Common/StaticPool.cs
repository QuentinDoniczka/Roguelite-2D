using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Common
{
    public class StaticPool<T> where T : Component
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private Func<T> _factory;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        public void Initialize(Func<T> factory, int initialSize)
        {
            _factory = factory;

            for (int i = 0; i < initialSize; i++)
            {
                _queue.Enqueue(factory());
            }

            _isInitialized = true;
        }

        public T Get()
        {
            return _queue.Count > 0
                ? _queue.Dequeue()
                : _factory();
        }

        public void Return(T instance)
        {
            _queue.Enqueue(instance);
        }

        public void Clear()
        {
            _queue.Clear();
            _factory = null;
            _isInitialized = false;
        }
    }
}
